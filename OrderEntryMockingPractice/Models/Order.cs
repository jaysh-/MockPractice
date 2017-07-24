using System.Collections.Generic;
using System.Linq;
using OrderEntryMockingPractice.Services;

namespace OrderEntryMockingPractice.Models
{
    public class Order
    {
        public Order()
        {
            OrderItems = new List<OrderItem>();
        }

	    public int? CustomerId { get; set; }
	    public List<OrderItem> OrderItems { get; set; }

		public static bool IsValid(IProductRepository productRepo, Order order)
	    {
		    return AreProductsUnique(order.OrderItems) && ProductsAreInStock(productRepo, order.OrderItems);
	    }

	    public static List<OrderRuleViolation> GetValidationErrors(IProductRepository productRepo, Order order)
	    {
		    var errorList = new List<OrderRuleViolation>();
		    if (!ProductsAreInStock(productRepo, order.OrderItems))
		    {
			    errorList.Add(new OrderRuleViolation("A product is out of stock"));
		    }
		    if (!AreProductsUnique(order.OrderItems))
		    {
			    errorList.Add(new OrderRuleViolation("Products are not unique"));
		    }
		    return errorList;
	    }

		private static bool AreProductsUnique(IReadOnlyCollection<OrderItem> products)
	    {
		    return products.GroupBy(x => x.Product.Sku).Select(x => x.First()).ToList().Count == products.Count;
	    }


	    private static bool ProductsAreInStock(IProductRepository productRepo, IEnumerable<OrderItem> orderItems)
	    {
		    foreach (var item in orderItems)
		    {
			    if (!productRepo.IsInStock(item.Product.Sku))
			    {
				    return false;
			    }
		    }
		    return true;
	    }

	}
}
