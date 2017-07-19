using OrderEntryMockingPractice.Models;
using static OrderEntryMockingPractice.Services.OrderValidator;

namespace OrderEntryMockingPractice.Services
{
	public class OrderService
    {
        public OrderSummary PlaceOrder(Order order)
        {
			//TODO Implementation
	        if (IsValid(order))
	        {
		        //place order
		        return new OrderSummary()
		        {
			        CustomerId = 1
		        };
	        }
	        else
	        {
		        //get rule violations 
		        return null;
	        }
        }
    }
}
