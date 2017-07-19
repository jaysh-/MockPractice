using System.Collections.Generic;
using OrderEntryMockingPractice.Models;

namespace OrderEntryMockingPractice.Services
{
	//TODO I think this should be an interface
	public static class OrderValidator
	{
		public static bool IsValid(Order order)
		{
			//TODO Add validation logic
			throw new System.NotImplementedException();
		}

		public static List<OrderRuleViolation> GetOrderRuleViolations()
		{
			//Yield structure to get all rules
			throw new System.NotImplementedException();
		}
	}
}