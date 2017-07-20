using System;
using System.Collections.Generic;

namespace OrderEntryMockingPractice.Services
{
	public class OrderException : Exception
	{
		public OrderException(IReadOnlyList<OrderRuleViolation> reasons)
		{
			Reasons = reasons;
		}

		public IReadOnlyList<OrderRuleViolation> Reasons { get; }
	}
}