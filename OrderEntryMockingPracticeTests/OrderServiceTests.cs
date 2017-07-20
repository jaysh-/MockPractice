using NSubstitute;
using NUnit.Framework;
using OrderEntryMockingPractice.Models;
using OrderEntryMockingPractice.Services;
using Shouldly;
using System.Collections.Generic;
using Assert = NUnit.Framework.Assert;

namespace OrderEntryMockingPracticeTests
{
	public class OrderServiceTests
	{
		[Test]
		public void OrderService_PlaceInvalidOrder_OrderException()
		{
			//Arrange
			var orderItems = GetOrderItemsWithDuplicates();
			var invalidOrder = GetOrder(orderItems);

			var sut = new OrderService(GetBasicMockProductRepo(false));

			//Act and assert
			Should.Throw<OrderException>(() => { sut.PlaceOrder(invalidOrder); });
		}

		[Test]
		public void OrderService_PlaceValidOrder_OrderSummary()
		{
			//Arrange
			var validOrder = GetOrder(GetOrderItemsWithoutDuplicates());
			var sut = new OrderService(GetBasicMockProductRepo(true));

			//Act
			var orderSummary = sut.PlaceOrder(validOrder);

			//Assert
			Assert.IsInstanceOf<OrderSummary>(orderSummary);
		}

		

		[Test]
		public void OrderService_AllProductsInStock_False()
		{
			var sut = new OrderService(Get_InAndOutOfStock_MockProductRepo());

			sut.AreProductsInStock(GetOutOfStockItems()).ShouldBe(false);
		}

		[Test]
		public void OrderService_AllProductsInStock_True()
		{
			var sut = new OrderService(Get_InAndOutOfStock_MockProductRepo());

			sut.AreProductsInStock(GetInStockItems()).ShouldBe(true);
		}

		[Test]
		public void OrderService_AllProductsUnique_True()
		{
			var validOrder = GetOrderItemsWithoutDuplicates();
			var sut = new OrderService(GetBasicMockProductRepo(true));

			sut.AreProductsUnique(validOrder).ShouldBe(true);
		}

		[Test]
		public void OrderService_AllProductsUnique_False()
		{
			var validOrder = GetOrderItemsWithDuplicates();
			var sut = new OrderService(GetBasicMockProductRepo(true));

			sut.AreProductsUnique(validOrder).ShouldBe(false);
		}




		/*
		 * Private helpers
		 */
		private static IProductRepository GetBasicMockProductRepo(bool alwaysInStock)
		{
			var productRepo = Substitute.For<IProductRepository>();
			productRepo.IsInStock(Arg.Any<string>()).Returns(alwaysInStock);

			return productRepo;
		}

		private static IProductRepository Get_InAndOutOfStock_MockProductRepo()
		{
			var productRepo = Substitute.For<IProductRepository>();
			productRepo.IsInStock("not in stock").Returns(false);
			productRepo.IsInStock("in stock").Returns(true);
			return productRepo;
		}

		private static List<OrderItem> GetInStockItems()
		{
			var orderItems = new List<OrderItem>()
			{
				new OrderItem()
				{
					Product = new Product()
					{
						Sku = "in stock"
					},
					Quantity = 1
				},
				new OrderItem()
				{
					Product = new Product()
					{
						Sku = "in stock"
					},
					Quantity = 1
				}
			};

			return orderItems;
		}

		private static List<OrderItem> GetOutOfStockItems()
		{
			var orderItems = new List<OrderItem>()
			{
				new OrderItem()
				{
					Product = new Product()
					{
						Sku = "not in stock"
					},
					Quantity = 1
				},
				new OrderItem()
				{
					Product = new Product()
					{
						Sku = "in stock"
					},
					Quantity = 1
				}
			};

			return orderItems;
		}
		private static Order GetOrder(List<OrderItem> orderItems)
		{
			return new Order()
			{
				CustomerId = 1,
				OrderItems = orderItems
			};
		}

		private static List<OrderItem> GetOrderItemsWithDuplicates()
		{
			var orderItems = new List<OrderItem>()
			{
				new OrderItem()
				{
					Product = new Product()
					{
						Description = "a",
						Name = "a",
						Price = 1,
						ProductId = 1,
						Sku = "1"
					},
					Quantity = 1
				},
				new OrderItem()
				{
					Product = new Product()
					{
						Description = "A",
						Name = "A",
						Price = 1,
						ProductId = 1,
						Sku = "1"
					},
					Quantity = 1
				}
			};
			return orderItems;
		}

		private static List<OrderItem> GetOrderItemsWithoutDuplicates()
		{
			var orderItems = new List<OrderItem>()
			{
				new OrderItem()
				{
					Product = new Product()
					{
						Description = "a",
						Name = "a",
						Price = 1,
						ProductId = 1,
						Sku = "1"
					},
					Quantity = 1
				},
				new OrderItem()
				{
					Product = new Product()
					{
						Description = "b",
						Name = "b",
						Price = 1,
						ProductId = 2,
						Sku = "2"
					},
					Quantity = 1
				}
			};

			return orderItems;
		}
	}
}
