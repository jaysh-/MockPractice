﻿using System;
using OrderEntryMockingPractice.Models;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace OrderEntryMockingPractice.Services
{
	public class OrderService
	{
		private readonly IProductRepository _productRepository;
		private readonly IOrderFulfillmentService _orderFulfillment;
		private readonly ITaxRateService _taxRateService;
		private readonly ICustomerRepository _customerRepository;
		private readonly IEmailService _emailService;

		public OrderService(IProductRepository productRepository, IOrderFulfillmentService orderFulfillment, ITaxRateService taxRateService, ICustomerRepository customerRepository, IEmailService emailService)
		{
			_productRepository = productRepository;
			_orderFulfillment = orderFulfillment;
			_taxRateService = taxRateService;
			_customerRepository = customerRepository;
			_emailService = emailService;
		}

        public OrderSummary PlaceOrder(Order order)
        {
	        //Valid if all items are unique and stocked
	        if (!IsValid(order))
	        {
		        throw new OrderException(GetOrderValidationErrors(order).AsReadOnly());
	        }

	        return GetOrderSummary(order);
        }

		private List<OrderRuleViolation> GetOrderValidationErrors(Order order)
		{
			var errorList = new List<OrderRuleViolation>();
			if (!AreProductsInStock(order.OrderItems))
			{
				errorList.Add(new OrderRuleViolation("A product is out of stock"));
			}
			if (!AreProductsUnique(order.OrderItems))
			{
				errorList.Add(new OrderRuleViolation("Products are not unique"));
			}
			return errorList;
		}

		[SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
		private OrderSummary GetOrderSummary(Order order)
		{
			var orderConfirmation = _orderFulfillment.Fulfill(order);
			var customer = GetCustomerFromId(order.CustomerId);
			var taxes = _taxRateService.GetTaxEntries(customer.PostalCode, customer.Country);
			var netTotal = GetNetTotal(order.OrderItems);
			var total = GetTotal(taxes, netTotal);

			_emailService.SendOrderConfirmationEmail(orderConfirmation.CustomerId, orderConfirmation.OrderId);
			return new OrderSummary()
			{
				OrderId = orderConfirmation.OrderId,
				OrderNumber = orderConfirmation.OrderNumber,
				CustomerId = orderConfirmation.CustomerId,
				Taxes = taxes,
				NetTotal = netTotal,
				Total = total,
				OrderItems = order.OrderItems,
				EstimatedDeliveryDate = DateTime.Today.AddDays(7)
			};
		}

		private static decimal GetTotal(IEnumerable<TaxEntry> taxes, decimal pretax)
		{
			var total = 0.0m;
			foreach (var tax in taxes)
			{
				total += (tax.Rate * pretax);
			}

			return total;
		}

		private static decimal GetNetTotal(IEnumerable<OrderItem> orderItems)
		{
			var total = 0.0m;
			foreach (var item in orderItems)
			{
				var product = item.Product;
				var quantity = item.Quantity;

				total += (product.Price * quantity);
			}
			return total;
		}

		private Customer GetCustomerFromId(int? id)
		{
			Customer customer;
			if (id != null)
			{
				customer = _customerRepository.Get((int) id);
			}
			else
			{
				throw new ArgumentException(message: "Customer ID was null");
			}
			return customer;
		}


		public bool IsValid(Order order)
	    {
		    return AreProductsUnique(order.OrderItems) && AreProductsInStock(order.OrderItems);
	    }

		public bool AreProductsInStock(List<OrderItem> orderItems)
		{
			foreach (var item in orderItems)
			{
				if (!_productRepository.IsInStock(item.Product.Sku))
				{
					return false;
				}
			}
			return true;
		}

		public bool AreProductsUnique(List<OrderItem> products)
		{
			return products.GroupBy(x => x.Product.Sku).Select(x => x.First()).ToList().Count == products.Count;
		}
	}
}
