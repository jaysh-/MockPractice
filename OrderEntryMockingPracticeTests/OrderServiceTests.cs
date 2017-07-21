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
		public void OrderService_PlaceOutofStockOrder_OrderExceptionOutofStockReason()
		{
			//Arrange
			var invalidOrder = Get_ValidOrder();
			var productRepo = Get_AlwaysStocked_MockProductRepo(alwaysInStock: false);
			var sut = Get_InitializedOrderService(invalidOrder, productRepo);

			//Act and assert
			try
			{
				sut.PlaceOrder(invalidOrder);
			}
			catch (OrderException e)
			{
				e.Reasons.Count.ShouldBe(1);
				e.Reasons.FirstOrDefault().ShouldBeOfType<OrderRuleViolation>();
				e.Reasons.First().ErrorMessage.ShouldBe("A product is out of stock");
			}
		}

		[Test]
		public void OrderService_PlaceSameSkuOrder_OrderExceptionOutofStockReason()
		{
			//Arrange
			var invalidOrder = Get_InvalidOrder_RepeatedSkus();
			var productRepo = Get_AlwaysStocked_MockProductRepo(alwaysInStock:true);
			var sut = Get_InitializedOrderService(invalidOrder, productRepo);

			//Act and assert
			try
			{
				sut.PlaceOrder(invalidOrder);
			}
			catch (OrderException e)
			{
				e.Reasons.Count.ShouldBe(1);
				e.Reasons.FirstOrDefault().ShouldBeOfType<OrderRuleViolation>();
				e.Reasons.First().ErrorMessage.ShouldBe("Products are not unique");
			}
		}

		[Test]
		public void OrderService_PlaceInvalidOrder_OrderException()
		{
			//Arrange
			var invalidOrder = Get_InvalidOrder_RepeatedSkus();
			var productRepo = Get_AlwaysStocked_MockProductRepo(alwaysInStock: false);
			var sut = Get_InitializedOrderService(invalidOrder, productRepo);

			//Act and assert
			try
			{
				sut.PlaceOrder(invalidOrder);
			}
			catch (OrderException e)
			{
				e.Reasons.Count.ShouldBe(2);
				//Not checking the elements
			}
		}

		[Test]
		public void OrderService_PlaceValidOrder_OrderSummary()
		{
			//Arrange
			var validOrder = Get_ValidOrder();
			var productRepo = Get_AlwaysStocked_MockProductRepo(alwaysInStock: true);
			var sut = Get_InitializedOrderService(validOrder, productRepo);

			//Act
			var orderSummary = sut.PlaceOrder(validOrder);

			//Assert
			orderSummary.ShouldBeOfType<OrderSummary>();
		}

		[Test]
		public void OrderService_AllProductsInStock_False()
		{
			//Arange
			var validOrder = Get_ValidOrder();
			var productRepo = Get_InAndOutOfStock_MockProductRepo();
			var sut = Get_InitializedOrderService(validOrder, productRepo);

			//Act and Assert
			sut.AreProductsInStock(Get_OneOutOfStock_OrderItems()).ShouldBe(false);
		}

		[Test]
		public void OrderService_AllProductsInStock_True()
		{
			//Arrange
			var validOrder = Get_ValidOrder();
			var productRepo = Get_InAndOutOfStock_MockProductRepo();
			var sut = Get_InitializedOrderService(validOrder, productRepo);

			//Act and Assert
			sut.AreProductsInStock(Get_InStockNotUnique_OrderItems()).ShouldBe(true);
		}

		[Test]
		public void OrderService_AllProductsUnique_True()
		{
			//Arrange
			var uniqueOrderItems = Get_OrderItemsWithoutDuplicates();
			var productRepo = Get_AlwaysStocked_MockProductRepo(alwaysInStock: true);
			var sut = Get_InitializedOrderService(Get_ValidOrder(), productRepo);

			//Act and Assert
			sut.AreProductsUnique(uniqueOrderItems).ShouldBe(true);
		}

		[Test]
		public void OrderService_AllProductsUnique_False()
		{
			var notUniqueItems = Get_OrderItems_WithDuplicateSkus();
			var productRepo = Get_AlwaysStocked_MockProductRepo(alwaysInStock: true);
			var sut = Get_InitializedOrderService(Get_ValidOrder(), productRepo);

			sut.AreProductsUnique(notUniqueItems).ShouldBe(false);
		}

		[Test]
		public void OrderService_ValidOrder_SubmitsToOrderFullfillmentService()
		{
			//Arrange
			var validOrder = Get_OrderFromOrderItems(Get_OrderItemsWithoutDuplicates());
			var productRepo = Get_AlwaysStocked_MockProductRepo(alwaysInStock: true);
			var orderFulfillment = Substitute.For<IOrderFulfillmentService>();
			var taxRate = Substitute.For<ITaxRateService>();
			var customerRepo = Substitute.For<ICustomerRepository>();
			var email = Substitute.For<IEmailService>();

			SetServiceReturns(taxRate, customerRepo, orderFulfillment, validOrder);

			//Act
			var sut = new OrderService(productRepo, orderFulfillment, taxRate, customerRepo, email);
			sut.PlaceOrder(validOrder);

			//Assert
			orderFulfillment.Received().Fulfill(validOrder);
		}

		[Test]
		public void OrderService_OrderConfirmationNumber_MatchesOrderSummaryOrderNumber()
		{
			//Arrange
			var validOrder = Get_OrderFromOrderItems(Get_OrderItemsWithoutDuplicates());
			var productRepo = Get_AlwaysStocked_MockProductRepo(alwaysInStock: true);
			var sut = Get_InitializedOrderService(validOrder, productRepo);

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
			var productRepo = Get_AlwaysStocked_MockProductRepo(alwaysInStock: true);
			var sut = Get_InitializedOrderService(validOrder, productRepo);

			//Act
			var orderSummary = sut.PlaceOrder(validOrder);

			//Assert
			orderSummary.OrderId.ShouldBe(2);
		}

		[Test]
		public void OrderService_OrderSummaryTaxes_MatchesCustomerLocation()
		{
			//Arrange
			var validOrder = Get_OrderFromOrderItems(Get_OrderItemsWithoutDuplicates());
			var productRepo = Get_AlwaysStocked_MockProductRepo(alwaysInStock: true);
			var orderFulfillment = Substitute.For<IOrderFulfillmentService>();
			var taxRate = Substitute.For<ITaxRateService>();
			var customerRepo = Substitute.For<ICustomerRepository>();
			var email = Substitute.For<IEmailService>();

			SetServiceReturns(taxRate, customerRepo, orderFulfillment, validOrder);

			//Act
			var orderSummary =
				new OrderService(productRepo, orderFulfillment, taxRate, customerRepo, email).PlaceOrder(validOrder);
			var customer = customerRepo.Get(1);

			//Assert
			orderSummary.Taxes.ShouldBe(taxRate.GetTaxEntries(postalCode: customer.PostalCode, country: customer.Country));
		}

		[Test]
		public void OrderService_OrderSummaryTaxes_NullCustomerId()
		{
			//Arrange

			var validOrderNullCustomerId = Get_OrderWithNullCustomerId(Get_OrderItemsWithoutDuplicates());
			var productRepo = Get_AlwaysStocked_MockProductRepo(alwaysInStock: true);
			var sut = Get_InitializedOrderService(validOrderNullCustomerId, productRepo);

			//Act
			Should.Throw<ArgumentException>(() => { sut.PlaceOrder(validOrderNullCustomerId); });
		}

		[Test]
		public void OrderService_NetTotal_Equals90()
		{
			//Arrange
			var validOrder = Get_OrderFromOrderItems(Get_OrderItemsWithoutDuplicates());
			var productRepo = Get_AlwaysStocked_MockProductRepo(alwaysInStock: true);

			//Act
			var orderSummary = Get_InitializedOrderService(validOrder, productRepo).PlaceOrder(validOrder);

			//Assert
			orderSummary.NetTotal.ShouldBe(90m);
		}

		[Test]
		public void OrderService_Total_Equals()
		{
			//Arrange
			var validOrder = Get_OrderFromOrderItems(Get_OrderItemsWithoutDuplicates());
			var productRepo = Get_AlwaysStocked_MockProductRepo(alwaysInStock: true);

			var sut = Get_InitializedOrderService(validOrder, productRepo);

			//Act
			var orderSummary = sut.PlaceOrder(validOrder);
			orderSummary.Total.ShouldBe(1242m);
		}

		[Test]
		public void OrderService_SendConfirmationEmail_Equals()
		{
			//Arrange
			var validOrder = Get_OrderFromOrderItems(Get_OrderItemsWithoutDuplicates());
			var productRepo = Get_AlwaysStocked_MockProductRepo(alwaysInStock: true);
			var orderFulfillment = Substitute.For<IOrderFulfillmentService>();
			var taxRate = Substitute.For<ITaxRateService>();
			var customerRepo = Substitute.For<ICustomerRepository>();
			var email = Substitute.For<IEmailService>();

			SetServiceReturns(taxRate, customerRepo, orderFulfillment, validOrder);


			var sut = new OrderService(productRepo, orderFulfillment, taxRate, customerRepo, email);

			sut.PlaceOrder(validOrder);

			//Act
			email.Received().SendOrderConfirmationEmail(customerId: 1, orderId: 2);
		}

		[Test]
		public void OrderService_ValidRealisticOrder_FullOrderSummary()
		{
			var validOrder = Get_OrderFromOrderItems(Get_ValidRealisticOrderItems());
			var productRepo = Get_Realistic_MockProductRepo();
			var orderFulfillment = Substitute.For<IOrderFulfillmentService>();
			var taxRate = Substitute.For<ITaxRateService>();
			var customerRepo = Substitute.For<ICustomerRepository>();
			var email = Substitute.For<IEmailService>();

			Set_ValidRealisticItems_InStock(productRepo);
			SetTaxRateReturn(taxRate);
			Set_CustomerRepoReturn_ValidId(customerRepo);
			Set_OrderFullfillment_ReturnedConfirmation(orderFulfillment, validOrder);


			var sut = new OrderService(productRepo, orderFulfillment, taxRate, customerRepo, email);

			var orderSummary = sut.PlaceOrder(validOrder);

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
