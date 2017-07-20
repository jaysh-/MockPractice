using OrderEntryMockingPractice.Models;
using System.Collections.Generic;
using System.Linq;

namespace OrderEntryMockingPractice.Services
{
	public class OrderService
	{
		private IProductRepository _productRepository;

		public OrderService(IProductRepository productRepository)
		{
			_productRepository = productRepository;
		}

        public OrderSummary PlaceOrder(Order order)
        {
			//Valid if all items are unique and stocked
	        if (IsValid(order))
	        {
				return new OrderSummary();
	        }
			throw new OrderException(new List<OrderRuleViolation>());
        }


	    private bool IsValid(Order order)
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
