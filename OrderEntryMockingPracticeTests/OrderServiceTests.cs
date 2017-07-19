using NSubstitute;
using Shouldly;
using NUnit.Framework;
using OrderEntryMockingPractice.Services;

namespace OrderEntryMockingPracticeTests
{
	public class OrderServiceTests
	{
		[Test]
		public void Products_AreAllInStock_True()
		{
			//Setup
			var productRepository = Substitute.For<IProductRepository>();
			productRepository.IsInStock("valid product sku").Returns(true);

			//Action
			productRepository.IsInStock("valid product sku");

			//Result
			productRepository.Received().IsInStock(Arg.Any<string>());
			productRepository.IsInStock("valid product sku").ShouldBe(true);
		}

		[Test]
		public void Products_AreAllInStock_False()
		{

		}

	}
}
