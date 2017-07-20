namespace OrderEntryMockingPractice.Services
{
	public class OrderRuleViolation : IRuleViolation
	{
		public string ErrorMessage { get; private set; }


		public OrderRuleViolation(string errorMessage)
		{
			ErrorMessage = errorMessage;
		}
	}
}