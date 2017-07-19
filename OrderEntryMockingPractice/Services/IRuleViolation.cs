namespace OrderEntryMockingPractice.Services
{
	public interface IRuleViolation
	{
		string ErrorMessage { get; }
		string PropertyMessage { get; }
	}
}