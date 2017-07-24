using System;
using OrderEntryMockingPractice.Models;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

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
	        if (!Order.IsValid(_productRepository, order))
	        {
		        throw new OrderException(Order.GetValidationErrors(_productRepository, order));
	        }
	        return GetOrderSummary(order);
        }

		/*
		 * Private Helpers
		 */

		[SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
		private OrderSummary GetOrderSummary(Order order)
		{
			var customer = GetCustomerFromId(_customerRepository, order.CustomerId);
			var taxes = GetTaxes(_taxRateService, customer);

			var netTotal = GetNetTotal(order.OrderItems);
			var total = GetTotal(taxes, netTotal);

			var orderConfirmation = _orderFulfillment.Fulfill(order);
			SendConfirmationEmail(_emailService, orderConfirmation);

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

		private static IEnumerable<TaxEntry> GetTaxes(ITaxRateService taxRateService, Customer customer)
		{
			return taxRateService.GetTaxEntries(customer.PostalCode, customer.Country);
		}

		private static void SendConfirmationEmail(IEmailService emailService, OrderConfirmation orderConfirmation)
		{
			emailService.SendOrderConfirmationEmail(orderConfirmation.CustomerId, orderConfirmation.OrderId);
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


		private static Customer GetCustomerFromId(ICustomerRepository customerRepo, int? id)
		{
			Customer customer;
			if (id != null)
			{
				customer = customerRepo.Get((int)id);
			}
			else
			{
				throw new ArgumentException(message: "Customer ID was null");
			}
			return customer;
		}
	}
}
