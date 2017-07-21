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
		public void OrderService_PlaceOutofStockOrder_OutofStockOrderException()
		{
			//Arrange
			var order = Get_Order_WithoutDuplicates();
			var productRepo = Get_AlwaysOutOfStock_MockProductRepo();
			var sut = Get_OrderService(order, productRepo);

			try
			{
				//Act
				sut.PlaceOrder(order);
				Assert.Fail();
			}
			catch (OrderException e)
			{
				//Assert
				e.Reasons.Count.ShouldBe(1);
				e.Reasons.FirstOrDefault().ShouldBeOfType<OrderRuleViolation>();
				e.Reasons.First().ErrorMessage.ShouldBe("A product is out of stock");
			}
		}

		[Test]
		public void OrderService_PlaceSameSkuOrder_OutofStockOrderException()
		{
			//Arrange
			var order = Get_Order_WithRepeatedSkus();
			var productRepo = Get_AlwaysStocked_MockProductRepo();
			var sut = Get_OrderService(order, productRepo);
			
			try
			{
				//Act
				sut.PlaceOrder(order);
				Assert.Fail();
			}
			catch (OrderException e)
			{
				//Assert
				e.Reasons.Count.ShouldBe(1);
				e.Reasons.FirstOrDefault().ShouldBeOfType<OrderRuleViolation>();
				e.Reasons.First().ErrorMessage.ShouldBe("Products are not unique");
			}
		}

		[Test]
		public void OrderService_PlaceOrder_OutOfStockAndDuplicateSkuOrderException()
		{
			//Arrange
			var order = Get_Order_WithRepeatedSkus();
			var productRepo = Get_AlwaysOutOfStock_MockProductRepo();
			var sut = Get_OrderService(order, productRepo);

			try
			{
				//Act
				sut.PlaceOrder(order);
				Assert.Fail();
			}
			catch (OrderException e)
			{
				//Assert
				e.Reasons.ShouldBeUnique();
				e.Reasons.Count.ShouldBe(2);
			}
		}

		[Test]
		public void OrderService_PlaceOrder_OrderSummaryReturned()
		{
			//Arrange
			var order = Get_Order_WithoutDuplicates();
			var productRepo = Get_AlwaysStocked_MockProductRepo();
			var sut = Get_OrderService(order, productRepo);

			//Act
			var orderSummary = sut.PlaceOrder(order);

			//Assert
			orderSummary.ShouldBeOfType<OrderSummary>();
		}

		[Test]
		public void OrderService_PlaceOrder_SubmitsToOrderFullfillmentService()
		{
			//Arrange
			var order = Get_Order_WithoutDuplicates();
			var productRepo = Get_AlwaysStocked_MockProductRepo();

			var orderFulfillment = Substitute.For<IOrderFulfillmentService>();
			var taxRate = Substitute.For<ITaxRateService>();
			var customerRepo = Substitute.For<ICustomerRepository>();
			var email = Substitute.For<IEmailService>();

			Set_MockedServiceReturns(taxRate, customerRepo, orderFulfillment, order);

			//Act
			var sut = new OrderService(productRepo, orderFulfillment, taxRate, customerRepo, email);
			sut.PlaceOrder(order);

			//Assert
			orderFulfillment.Received().Fulfill(order);
		}

		[Test]
		public void OrderService_PlaceOrder_OrderSummaryHasOrderNumber()
		{
			//Arrange
			var order = Get_Order_WithoutDuplicates();
			var productRepo = Get_AlwaysStocked_MockProductRepo();
			var sut = Get_OrderService(order, productRepo);

			//Act
			var orderSummary = sut.PlaceOrder(order);

			//Assert
			orderSummary.OrderNumber.ShouldBe("1337");
		}

		[Test]
		public void OrderService_PlaceOrder_OrderSummaryHasOrderId()
		{
			//Arrange
			var order = Get_Order_WithoutDuplicates();
			var productRepo = Get_AlwaysStocked_MockProductRepo();
			var sut = Get_OrderService(order, productRepo);

			//Act
			var orderSummary = sut.PlaceOrder(order);

			//Assert
			orderSummary.OrderId.ShouldBe(2);
		}

		[Test]
		public void OrderService_PlaceOrder_OrderSummaryHasTaxes()
		{
			//Arrange
			var order = Get_Order_WithoutDuplicates();
			var productRepository = Get_AlwaysStocked_MockProductRepo();

			var orderFulfillmentService = Substitute.For<IOrderFulfillmentService>();
			var taxRateService = Substitute.For<ITaxRateService>();
			var customerRepository = Substitute.For<ICustomerRepository>();
			var emailService = Substitute.For<IEmailService>();

			Set_MockedServiceReturns(taxRateService, customerRepository, orderFulfillmentService, order);
			var sut = new OrderService(productRepository, orderFulfillmentService, taxRateService, customerRepository, emailService);

			//Act
			var orderSummary = sut.PlaceOrder(order);
			var customer = customerRepository.Get(1);

			//Assert
			orderSummary.Taxes.ShouldBe(taxRateService.GetTaxEntries(customer.PostalCode, customer.Country));
		}

		[Test]
		public void OrderService_PlaceOrder_NullCustomerIdThrowsException()
		{
			//Arrange
			var order = Get_OrderWithNullCustomerId();
			var productRepo = Get_AlwaysStocked_MockProductRepo();
			var sut = Get_OrderService(order, productRepo);

			//Act and assert
			Should.Throw<ArgumentException>(() => { sut.PlaceOrder(order); });
		}

		[Test]
		public void OrderService_PlaceOrder_OrderSummaryHasNetTotal()
		{
			//Arrange
			var order = Get_Order_WithoutDuplicates();
			var productRepo = Get_AlwaysStocked_MockProductRepo();
			var sut = Get_OrderService(order, productRepo);

			//Act
			var orderSummary = sut.PlaceOrder(order);

			//Assert
			orderSummary.NetTotal.ShouldBe(90m);
		}

		[Test]
		public void OrderService_PlaceOrder_OrderSummaryHasTotal()
		{
			//Arrange
			var order = Get_Order_WithoutDuplicates();
			var productRepo = Get_AlwaysStocked_MockProductRepo();
			var sut = Get_OrderService(order, productRepo);

			//Act
			var orderSummary = sut.PlaceOrder(order);

			//Assert
			orderSummary.Total.ShouldBe(1242m);
		}

		[Test]
		public void OrderService_PlaceOrder_SendsConfirmationEmail()
		{
			//Arrange
			var order = Get_Order_WithoutDuplicates();
			var productRepo = Get_AlwaysStocked_MockProductRepo();
			var orderFulfillment = Substitute.For<IOrderFulfillmentService>();
			var taxRate = Substitute.For<ITaxRateService>();
			var customerRepo = Substitute.For<ICustomerRepository>();
			var email = Substitute.For<IEmailService>();
			Set_MockedServiceReturns(taxRate, customerRepo, orderFulfillment, order);

			var sut = new OrderService(productRepo, orderFulfillment, taxRate, customerRepo, email);
			
			//Act
			sut.PlaceOrder(order);

			//Assert
			email.Received().SendOrderConfirmationEmail(customerId: 1, orderId: 2);
		}

		[Test]
		public void OrderService_PlaceOrder_FullOrderSummary()
		{
			//Arrange
			var order = Get_RealisticInStockOrderWithoutDuplicates();
			var productRepo = Get_RealisticAndStocked_MockProductRepo();
			var sut = Get_OrderService(order, productRepo);

			//Act
			var orderSummary = sut.PlaceOrder(order);

			//Assert
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
