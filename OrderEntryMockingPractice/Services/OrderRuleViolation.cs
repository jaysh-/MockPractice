namespace OrderEntryMockingPractice.Services
{
	public class OrderRuleViolation : IRuleViolation
	{
		public string ErrorMessage { get; private set; }
		public string PropertyMessage { get; private set; }


		public OrderRuleViolation(string errorMessage, string propertyName)
		{
			ErrorMessage = errorMessage;
			PropertyMessage = propertyName;
		}
	}
}