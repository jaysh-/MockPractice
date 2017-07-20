using OrderEntryMockingPractice.Models;
using System.Collections.Generic;
using System.Linq;

namespace OrderEntryMockingPractice.Services
{
	public class OrderService
	{
		private readonly IProductRepository _productRepository;
		private IOrderFulfillmentService _orderFulfillment;

		public OrderService(IProductRepository productRepository, IOrderFulfillmentService orderFulfillment)
		{
			_productRepository = productRepository;
			_orderFulfillment = orderFulfillment;
		}

        public OrderSummary PlaceOrder(Order order)
        {
			//Valid if all items are unique and stocked
	        if (IsValid(order))
	        {
		        var orderConfirmation = _orderFulfillment.Fulfill(order);

				//Add OrderNumber to summary
				return new OrderSummary()
				{
					OrderId = orderConfirmation.OrderId,
					OrderNumber = orderConfirmation.OrderNumber
				};
	        }
			throw new OrderException(new List<OrderRuleViolation>());
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
