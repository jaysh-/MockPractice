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
			productRepository.IsInStock("stocked product sku").Returns(true);
			
			//Action					 
			productRepository.IsInStock("stocked product sku");

			//Result
			productRepository.Received().IsInStock(Arg.Any<string>());
			productRepository.IsInStock("stocked product sku").ShouldBe(true);
		}

		[Test]
		public void Products_AreAllInStock_False()
		{
			var productRepository = Substitute.For<IProductRepository>();
			productRepository.IsInStock("unstocked product sku").Returns(false);

			//Action
			productRepository.IsInStock("unstocked product sku");

			//Result
			productRepository.Received().IsInStock(Arg.Any<string>());
			productRepository.IsInStock("unstocked product sku").ShouldBe(false);
		}



	}
}
