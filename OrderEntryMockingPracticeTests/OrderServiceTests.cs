using NSubstitute;
using NUnit.Framework;
using OrderEntryMockingPractice.Models;
using OrderEntryMockingPractice.Services;
using Shouldly;
using System.Collections.Generic;

namespace OrderEntryMockingPracticeTests
{
	public class OrderServiceTests
	{
		[Test]
		public void OrderService_PlaceInvalidOrder_OrderException()
		{
			//Arrange
			var orderItems = Get_OrderItems_WithDuplicateSkus();
			var invalidOrder = Get_OrderFromOrderItems(orderItems);

			var productRepo = Get_AlwaysStocked_MockProductRepo(false);
			var orderFulfillment = Substitute.For<IOrderFulfillmentService>();

			var sut = new OrderService(productRepo, orderFulfillment);

			//Act and assert
			Should.Throw<OrderException>(() => { sut.PlaceOrder(invalidOrder); });
		}

		[Test]
		public void OrderService_PlaceValidOrder_OrderSummary()
		{
			//Arrange
			var validOrder = Get_OrderFromOrderItems(Get_OrderItemsWithoutDuplicates());
			var productRepo = Get_AlwaysStocked_MockProductRepo(alwaysInStock:true);
			var orderFulfillment = Substitute.For<IOrderFulfillmentService>();

			var sut = new OrderService(productRepo, orderFulfillment);

			orderFulfillment.Fulfill(validOrder).Returns(new OrderConfirmation()
			{
				OrderNumber = ""
			});

			//Act
			var orderSummary = sut.PlaceOrder(validOrder);
			
			//Assert
			orderSummary.ShouldBeOfType<OrderSummary>();
		}

		

		[Test]
		public void OrderService_AllProductsInStock_False()
		{
			//Arange
			var productRepo = Get_InAndOutOfStock_MockProductRepo();
			var orderFulfillment = Substitute.For<IOrderFulfillmentService>();

			var sut = new OrderService(productRepo, orderFulfillment);

			//Act and Assert
			sut.AreProductsInStock(Get_OneOutOfStock_OrderItems()).ShouldBe(false);
		}

		[Test]
		public void OrderService_AllProductsInStock_True()
		{
			//Arrange
			var productRepo = Get_InAndOutOfStock_MockProductRepo();
			var orderFulfillment = Substitute.For<IOrderFulfillmentService>();

			var sut = new OrderService(productRepo, orderFulfillment);

			//Act and Assert
			sut.AreProductsInStock(Get_InStockNotUnique_OrderItems()).ShouldBe(true);
		}

		[Test]
		public void OrderService_AllProductsUnique_True()
		{
			var validOrder = Get_OrderItemsWithoutDuplicates();
			var orderFulfillment = Substitute.For<IOrderFulfillmentService>();
			var sut = new OrderService(Get_AlwaysStocked_MockProductRepo(alwaysInStock: true), orderFulfillment);

			sut.AreProductsUnique(validOrder).ShouldBe(true);
		}

		[Test]
		public void OrderService_AllProductsUnique_False()
		{
			var validOrder = Get_OrderItems_WithDuplicateSkus();
			var orderFulfillment = Substitute.For<IOrderFulfillmentService>();
			var sut = new OrderService(Get_AlwaysStocked_MockProductRepo(alwaysInStock: true), orderFulfillment);

			sut.AreProductsUnique(validOrder).ShouldBe(false);
		}

		[Test]
		public void OrderService_ValidOrder_SubmitsToOrderFullfillmentService()
		{
			//Arrange
			var validOrder = Get_OrderFromOrderItems(Get_OrderItemsWithoutDuplicates());
			var orderFulfillment = Substitute.For<IOrderFulfillmentService>();
			var sut = new OrderService(Get_AlwaysStocked_MockProductRepo(alwaysInStock: true), orderFulfillment);

			orderFulfillment.Fulfill(validOrder).Returns(new OrderConfirmation()
			{
				OrderNumber = ""
			});
			//Act
			var unused = sut.PlaceOrder(validOrder);

			//Assert
			orderFulfillment.Received().Fulfill(validOrder);
		}

		[Test]
		public void OrderService_OrderConfirmationNumber_MatchesOrderSummaryOrderNumber()
		{
			//Arrange
			var validOrder = Get_OrderFromOrderItems(Get_OrderItemsWithoutDuplicates());
			var orderFulfillment = Substitute.For<IOrderFulfillmentService>();
			var sut = new OrderService(Get_AlwaysStocked_MockProductRepo(alwaysInStock: true), orderFulfillment);

			orderFulfillment.Fulfill(validOrder).Returns(new OrderConfirmation()
			{
				CustomerId = 1,
				OrderId = 2,
				OrderNumber = "1337"
			});
			//Act
			var orderSummary = sut.PlaceOrder(validOrder);

			//Assert
			orderSummary.OrderNumber.ShouldBe("1337");
		}

		[Test]
		public void OrderService_OrderIdNumber_MatchesOrderSummaryOrderId()
		{
			//Arrange
			var validOrder = Get_OrderFromOrderItems(Get_OrderItemsWithoutDuplicates());
			var orderFulfillment = Substitute.For<IOrderFulfillmentService>();
			var sut = new OrderService(Get_AlwaysStocked_MockProductRepo(alwaysInStock: true), orderFulfillment);

			orderFulfillment.Fulfill(validOrder).Returns(new OrderConfirmation()
			{
				CustomerId = 1,
				OrderId = 2,
				OrderNumber = "1337"
			});
			//Act
			var orderSummary = sut.PlaceOrder(validOrder);

			//Assert
			orderSummary.OrderId.ShouldBe(2);
		}

		//[Test]
		//public void OrderService_OnValidOrder_ValidOrderSummary()
		//{
		//	//Arrange
		//	var validOrder = Get_OrderWithOrderItems(Get_OrderItemsWithoutDuplicates());
		//	var orderFullment = Substitute.For<IOrderFulfillmentService>();
		//	var sut = new OrderService(Get_AlwaysStocked_MockProductRepo(alwaysInStock: true), orderFullment);

		//	//Act
		//	var orderSummary = sut.PlaceOrder(validOrder);
		//	orderSummary.ShouldBeOfType<OrderSummary>();

		//	//Assert
		//	//TODO Check that all these things are happened
		//	/*
		//	 * IsSubmittedToOrderFullfillment
		//	 * HasOrderFullfillmentConfirmationNumber
		//	 * HasIDGenerdatedByOrderFullfillment
		//	 * HasCorrectTaxes
		//	 * HasCorrectNetTotal
		//	 * HasCorrectOrderTotal
		//	 * DidSendConfirmationEmail
		//	 */

		//}




		/*
		 * Private helpers
		 */
		private static IProductRepository Get_AlwaysStocked_MockProductRepo(bool alwaysInStock)
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

		private static List<OrderItem> Get_InStockNotUnique_OrderItems()
		{
			var orderItems = new List<OrderItem>()
			{
				Get_InStock_OrderItem(),
				Get_InStock_OrderItem()
			};

			orderItems.Count.ShouldBe(2);

			return orderItems;
		}

		private static List<OrderItem> Get_OneOutOfStock_OrderItems()
		{
			var orderItems = new List<OrderItem>()
			{
				Get_InStock_OrderItem(),
				Get_OutOfStock_OrderItem()
			};

			orderItems.Count.ShouldBe(2);

			return orderItems;
		}

		private static OrderItem Get_InStock_OrderItem() =>
		(new OrderItem()
		{
			Product = new Product()
			{
				Sku = "in stock"
			},
			Quantity = 1
		});

		private static OrderItem Get_OutOfStock_OrderItem() =>
		new OrderItem()
		{
			Product = new Product()
			{
				Sku = "not in stock"
			},
			Quantity = 1
		};

		private static Order Get_OrderFromOrderItems(List<OrderItem> orderItems)
		{
			return new Order()
			{
				CustomerId = 1,
				OrderItems = orderItems
			};
		}

		private static List<OrderItem> Get_OrderItems_WithDuplicateSkus()
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

		private static List<OrderItem> Get_OrderItemsWithoutDuplicates()
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
