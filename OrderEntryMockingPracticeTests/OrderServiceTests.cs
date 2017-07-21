using NSubstitute;
using NUnit.Framework;
using OrderEntryMockingPractice.Models;
using OrderEntryMockingPractice.Services;
using Shouldly;
using System;
using System.Linq;

namespace OrderEntryMockingPracticeTests
{
	internal partial class OrderServiceTests
	{
		[Test]
		public void OrderService_PlaceOutofStockOrder_OrderException_OutofStock()
		{
			//Arrange
			var orderWithoutDuplicates = Get_OrderWithoutDuplicates();
			var productRepo = Get_AlwaysOutOfStock_MockProductRepo();
			var sut = Get_InitializedOrderService(orderWithoutDuplicates, productRepo);

			//Act and assert
			try
			{
				sut.PlaceOrder(orderWithoutDuplicates);
			}
			catch (OrderException e)
			{
				e.Reasons.Count.ShouldBe(1);
				e.Reasons.FirstOrDefault().ShouldBeOfType<OrderRuleViolation>();
				e.Reasons.First().ErrorMessage.ShouldBe("A product is out of stock");
			}
		}

		[Test]
		public void OrderService_PlaceSameSkuOrder_OrderException_OutofStock()
		{
			//Arrange
			var orderWithRepeatedSkus = Get_OrderWithRepeatedSkus();
			var productRepo = Get_AlwaysStocked_MockProductRepo();
			var sut = Get_InitializedOrderService(orderWithRepeatedSkus, productRepo);

			//Act and assert
			try
			{
				sut.PlaceOrder(orderWithRepeatedSkus);
			}
			catch (OrderException e)
			{
				e.Reasons.Count.ShouldBe(1);
				e.Reasons.FirstOrDefault().ShouldBeOfType<OrderRuleViolation>();
				e.Reasons.First().ErrorMessage.ShouldBe("Products are not unique");
			}
		}

		[Test]
		public void OrderService_PlaceInvalidOrder_OrderException_OutOfStockAndDuplicate()
		{
			//Arrange
			var orderWithRepeatedSkus = Get_OrderWithRepeatedSkus();
			var productRepo = Get_AlwaysOutOfStock_MockProductRepo();
			var sut = Get_InitializedOrderService(orderWithRepeatedSkus, productRepo);

			//Act and assert
			try
			{
				sut.PlaceOrder(orderWithRepeatedSkus);
			}
			catch (OrderException e)
			{
				e.Reasons.Count.ShouldBe(2);
				//Read-only list
			}
		}

		[Test]
		public void OrderService_PlaceValidOrder_OrderSummaryReturned()
		{
			//Arrange
			var orderWithoutDuplicates = Get_OrderWithoutDuplicates();
			var productRepo = Get_AlwaysStocked_MockProductRepo();
			var sut = Get_InitializedOrderService(orderWithoutDuplicates, productRepo);

			//Act
			var orderSummary = sut.PlaceOrder(orderWithoutDuplicates);

			//Assert
			orderSummary.ShouldBeOfType<OrderSummary>();
		}

		[Test]
		public void OrderService_AreProductsInStock_False()
		{
			//Arange
			var orderWithoutDuplicates = Get_OrderWithoutDuplicates();
			var productRepo = Get_InAndOutOfStock_MockProductRepo();
			var sut = Get_InitializedOrderService(orderWithoutDuplicates, productRepo);

			//Act and Assert
			sut.AreProductsInStock(Get_OneOutOfStock_OrderItems()).ShouldBe(false);
		}

		[Test]
		public void OrderService_AreProductsInStock_True()
		{
			//Arrange
			var orderWithoutDuplicates = Get_OrderWithoutDuplicates();
			var productRepo = Get_InAndOutOfStock_MockProductRepo();
			var sut = Get_InitializedOrderService(orderWithoutDuplicates, productRepo);

			//Act and Assert
			sut.AreProductsInStock(Get_InStockNotUnique_OrderItems()).ShouldBe(true);
		}

		[Test]
		public void OrderService_AreProductsUnique_True()
		{
			//Arrange
			var uniqueOrderItems = Get_OrderItemsWithoutDuplicates();
			var productRepo = Get_AlwaysStocked_MockProductRepo();
			var sut = Get_InitializedOrderService(Get_OrderWithoutDuplicates(), productRepo);

			//Act and Assert
			sut.AreProductsUnique(uniqueOrderItems).ShouldBe(true);
		}

		[Test]
		public void OrderService_AreProductsUnique_False()
		{
			//Arrange
			var notUniqueOrderItems = Get_OrderItems_WithDuplicateSkus();
			var productRepo = Get_AlwaysStocked_MockProductRepo();
			var sut = Get_InitializedOrderService(Get_OrderWithoutDuplicates(), productRepo);

			//Act and Assert
			sut.AreProductsUnique(notUniqueOrderItems).ShouldBe(false);
		}

		[Test]
		public void OrderService_PlaceOrder_SubmitsToOrderFullfillmentService()
		{
			//Arrange
			var validOrder = Get_OrderFromOrderItems(Get_OrderItemsWithoutDuplicates());
			var productRepo = Get_AlwaysStocked_MockProductRepo();
			var orderFulfillment = Substitute.For<IOrderFulfillmentService>();
			var taxRate = Substitute.For<ITaxRateService>();
			var customerRepo = Substitute.For<ICustomerRepository>();
			var email = Substitute.For<IEmailService>();

			Set_ServiceReturns(taxRate, customerRepo, orderFulfillment, validOrder);

			//Act
			var sut = new OrderService(productRepo, orderFulfillment, taxRate, customerRepo, email);
			sut.PlaceOrder(validOrder);

			//Assert
			orderFulfillment.Received().Fulfill(validOrder);
		}

		[Test]
		public void OrderService_PlaceOrder_OrderSummaryHasOrderNumber()
		{
			//Arrange
			var validOrder = Get_OrderFromOrderItems(Get_OrderItemsWithoutDuplicates());
			var productRepo = Get_AlwaysStocked_MockProductRepo();
			var sut = Get_InitializedOrderService(validOrder, productRepo);

			//Act
			var orderSummary = sut.PlaceOrder(validOrder);

			//Assert
			orderSummary.OrderNumber.ShouldBe("1337");
		}

		[Test]
		public void OrderService_PlaceOrder_OrderSummaryHasOrderId()
		{
			//Arrange
			var validOrder = Get_OrderFromOrderItems(Get_OrderItemsWithoutDuplicates());
			var productRepo = Get_AlwaysStocked_MockProductRepo();
			var sut = Get_InitializedOrderService(validOrder, productRepo);

			//Act
			var orderSummary = sut.PlaceOrder(validOrder);

			//Assert
			orderSummary.OrderId.ShouldBe(2);
		}

		[Test]
		public void OrderService_PlaceOrder_OrderSummaryHasTaxes()
		{
			//Arrange
			var validOrder = Get_OrderFromOrderItems(Get_OrderItemsWithoutDuplicates());
			var productRepo = Get_AlwaysStocked_MockProductRepo();
			var orderFulfillment = Substitute.For<IOrderFulfillmentService>();
			var taxRate = Substitute.For<ITaxRateService>();
			var customerRepo = Substitute.For<ICustomerRepository>();
			var email = Substitute.For<IEmailService>();

			Set_ServiceReturns(taxRate, customerRepo, orderFulfillment, validOrder);

			//Act
			var sut = new OrderService(productRepo, orderFulfillment, taxRate, customerRepo, email);
			var orderSummary = sut.PlaceOrder(validOrder);
			var customer = customerRepo.Get(1);

			//Assert
			orderSummary.Taxes.ShouldBe(taxRate.GetTaxEntries(postalCode: customer.PostalCode, country: customer.Country));
		}

		[Test]
		public void OrderService_PlaceOrder_NullCustomerIdThrowsException()
		{
			//Arrange

			var validOrderNullCustomerId = Get_OrderWithNullCustomerId(Get_OrderItemsWithoutDuplicates());
			var productRepo = Get_AlwaysStocked_MockProductRepo();
			var sut = Get_InitializedOrderService(validOrderNullCustomerId, productRepo);

			//Act
			Should.Throw<ArgumentException>(() => { sut.PlaceOrder(validOrderNullCustomerId); });
		}

		[Test]
		public void OrderService_PlaceOrder_OrderSummaryHasNetTotal()
		{
			//Arrange
			var validOrder = Get_OrderFromOrderItems(Get_OrderItemsWithoutDuplicates());
			var productRepo = Get_AlwaysStocked_MockProductRepo();

			//Act
			var orderSummary = Get_InitializedOrderService(validOrder, productRepo).PlaceOrder(validOrder);

			//Assert
			orderSummary.NetTotal.ShouldBe(90m);
		}

		[Test]
		public void OrderService_PlaceOrder_OrderSummaryHasTotal()
		{
			//Arrange
			var validOrder = Get_OrderFromOrderItems(Get_OrderItemsWithoutDuplicates());
			var productRepo = Get_AlwaysStocked_MockProductRepo();

			var sut = Get_InitializedOrderService(validOrder, productRepo);

			//Act
			var orderSummary = sut.PlaceOrder(validOrder);

			//Assert
			orderSummary.Total.ShouldBe(1242m);
		}

		[Test]
		public void OrderService_PlaceOrder_SendsConfirmationEmail()
		{
			//Arrange
			var validOrder = Get_OrderFromOrderItems(Get_OrderItemsWithoutDuplicates());
			var productRepo = Get_AlwaysStocked_MockProductRepo();
			var orderFulfillment = Substitute.For<IOrderFulfillmentService>();
			var taxRate = Substitute.For<ITaxRateService>();
			var customerRepo = Substitute.For<ICustomerRepository>();
			var email = Substitute.For<IEmailService>();

			Set_ServiceReturns(taxRate, customerRepo, orderFulfillment, validOrder);


			var sut = new OrderService(productRepo, orderFulfillment, taxRate, customerRepo, email);

			sut.PlaceOrder(validOrder);

			//Act
			email.Received().SendOrderConfirmationEmail(customerId: 1, orderId: 2);
		}

		[Test]
		public void OrderService_PlaceOrder_FullOrderSummary()
		{
			var validOrder = Get_OrderFromOrderItems(Get_ValidRealisticOrderItems());
			var productRepo = Get_Realistic_MockProductRepo();
			var orderFulfillment = Substitute.For<IOrderFulfillmentService>();
			var taxRate = Substitute.For<ITaxRateService>();
			var customerRepo = Substitute.For<ICustomerRepository>();
			var email = Substitute.For<IEmailService>();

			Set_ServiceReturns(taxRate, customerRepo, orderFulfillment, validOrder);
			Set_ValidRealisticItems_InStock(productRepo);
			
			var orderSummary = new OrderService(productRepo, orderFulfillment, taxRate, customerRepo, email).PlaceOrder(validOrder);

			//Asserts
			email.Received().SendOrderConfirmationEmail(customerId: 1, orderId: 2);

			orderSummary.CustomerId.ShouldBe(1);
			orderSummary.OrderId.ShouldBe(2);
			orderSummary.OrderNumber.ShouldBe("1337");
			orderSummary.Taxes.Count().ShouldBe(2);
			orderSummary.NetTotal.ShouldBe(2713.86m);
			orderSummary.Total.ShouldBe(37451.268m);
			orderSummary.OrderItems.Count.ShouldBe(5);
			orderSummary.EstimatedDeliveryDate.ShouldBe(DateTime.Today.AddDays(7));
		}
	}
}
