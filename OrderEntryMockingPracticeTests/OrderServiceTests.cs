using System;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using OrderEntryMockingPractice.Services;
using Shouldly;
using static OrderEntryMockingPracticeTests.OrderServiceTestsExtensions;

namespace OrderEntryMockingPracticeTests
{
	internal class OrderServiceTests
	{
		private IOrderFulfillmentService _orderFulfillment;
		private ITaxRateService _taxRateService;
		private ICustomerRepository _customerRepository;
		private IEmailService _emailService;
		private IProductRepository _productRepo;
		private OrderService _sut;

		[SetUp]
		public void BeforeEach()
		{
			_orderFulfillment = Substitute.For<IOrderFulfillmentService>();
			_taxRateService = Substitute.For<ITaxRateService>();
			_taxRateService.SetTaxRateServiceReturn();
			_customerRepository = Substitute.For<ICustomerRepository>();
			_customerRepository.SetCustomerRepoReturn();
			_emailService = Substitute.For<IEmailService>();
			_productRepo = Substitute.For<IProductRepository>();
			_sut = new OrderService(_productRepo, _orderFulfillment, _taxRateService, _customerRepository, _emailService);
		}

		[Test]
		public void OrderService_PlaceOutofStockOrder_OutofStockOrderException()
		{
			//Arrange
			var order = Get_Order_WithoutDuplicates();
			_productRepo.AllItemsAreOutOfStock();

			//Act
			var e = Assert.Throws<OrderException>(() =>
			{
				_sut.PlaceOrder(order);
			});

			//Assert
			e.Reasons.Count.ShouldBe(1);
			e.Reasons.FirstOrDefault().ShouldBeOfType<OrderRuleViolation>();
			e.Reasons.First().ErrorMessage.ShouldBe("A product is out of stock");
		}

		[Test]
		public void OrderService_PlaceSameSkuOrder_OutofStockOrderException()
		{
			//Arrange
			var order = Get_Order_WithRepeatedSkus();
			_productRepo.AllItemsAreStocked();

			var e = Assert.Throws<OrderException>(() =>
			{
				_sut.PlaceOrder(order);
			});

			//Assert
			e.Reasons.Count.ShouldBe(1);
			e.Reasons.FirstOrDefault().ShouldBeOfType<OrderRuleViolation>();
			e.Reasons.First().ErrorMessage.ShouldBe("Products are not unique");
		}

		[Test]
		public void OrderService_PlaceOrder_OutOfStockAndDuplicateSkuOrderException()
		{
			//Arrange
			var order = Get_Order_WithRepeatedSkus();
			_productRepo.AllItemsAreOutOfStock();

			var e = Assert.Throws<OrderException>(() =>
			{
				_sut.PlaceOrder(order);
			});
			//Assert
			e.Reasons.ShouldBeUnique();
			e.Reasons.Count.ShouldBe(2);
		}

		[Test]
		public void OrderService_PlaceOrder_OrderSummaryReturned()
		{
			//Arrange
			var order = Get_Order_WithoutDuplicates();
			_productRepo.AllItemsAreStocked();
			_orderFulfillment.SetOrderFullfillmentReturn(order);

			//Act
			var orderSummary = _sut.PlaceOrder(order);

			//Assert
			orderSummary.ShouldNotBeNull();
		}

		[Test]
		public void OrderService_PlaceOrder_SubmitsToOrderFullfillmentService()
		{
			//Arrange
			var order = Get_Order_WithoutDuplicates();
			_productRepo.AllItemsAreStocked();
			_orderFulfillment.SetOrderFullfillmentReturn(order);

			//Act
			_sut.PlaceOrder(order);

			//Assert
			_orderFulfillment.Received().Fulfill(order);
		}

		[Test]
		public void OrderService_PlaceOrder_OrderSummaryHasOrderNumber()
		{
			//Arrange
			var order = Get_Order_WithoutDuplicates();
			_productRepo.AllItemsAreStocked();
			_orderFulfillment.SetOrderFullfillmentReturn(order);

			//Act
			var orderSummary = _sut.PlaceOrder(order);

			//Assert
			orderSummary.OrderNumber.ShouldBe("1337");
		}

		[Test]
		public void OrderService_PlaceOrder_OrderSummaryHasOrderId()
		{
			//Arrange
			var order = Get_Order_WithoutDuplicates();
			_productRepo.AllItemsAreStocked();
			_orderFulfillment.SetOrderFullfillmentReturn(order);

			//Act
			var orderSummary = _sut.PlaceOrder(order);

			//Assert
			orderSummary.OrderId.ShouldBe(2);
		}

		[Test]
		public void OrderService_PlaceOrder_OrderSummaryHasTaxes()
		{
			//Arrange
			var order = Get_Order_WithoutDuplicates();
			_productRepo.AllItemsAreStocked();
			_orderFulfillment.SetOrderFullfillmentReturn(order);

			//Act
			var orderSummary = _sut.PlaceOrder(order);
			var customer = _customerRepository.Get(1);

			//Assert
			orderSummary.Taxes.ShouldBe(_taxRateService.GetTaxEntries(customer.PostalCode, customer.Country));
		}

		[Test]
		public void OrderService_PlaceOrder_NullCustomerIdThrowsException()
		{
			//Arrange
			var order = Get_OrderWithNullCustomerId();
			_productRepo.AllItemsAreStocked();
			_orderFulfillment.SetOrderFullfillmentReturn(order);

			//Act
			var e = Assert.Throws<ArgumentException>(() => _sut.PlaceOrder(order));

			//Assert
			e.ShouldNotBeNull();
		}

		[Test]
		public void OrderService_PlaceOrder_OrderSummaryHasNetTotal()
		{
			//Arrange
			var order = Get_Order_WithoutDuplicates();

			_productRepo.AllItemsAreStocked();
			_orderFulfillment.SetOrderFullfillmentReturn(order);

			//Act
			var orderSummary = _sut.PlaceOrder(order);

			//Assert
			orderSummary.NetTotal.ShouldBe(90m);
		}

		[Test]
		public void OrderService_PlaceOrder_OrderSummaryHasTotal()
		{
			//Arrange
			var order = Get_Order_WithoutDuplicates();
			_productRepo.AllItemsAreStocked();
			_orderFulfillment.SetOrderFullfillmentReturn(order);

			//Act
			var orderSummary = _sut.PlaceOrder(order);

			//Assert
			orderSummary.Total.ShouldBe(1242m);
		}

		[Test]
		public void OrderService_PlaceOrder_SendsConfirmationEmail()
		{
			//Arrange
			var order = Get_Order_WithoutDuplicates();
			_productRepo.AllItemsAreStocked();
			_orderFulfillment.SetOrderFullfillmentReturn(order);

			//Act
			_sut.PlaceOrder(order);

			//Assert
			_emailService.Received().SendOrderConfirmationEmail(customerId: 1, orderId: 2);
		}

		[Test]
		public void OrderService_PlaceOrder_FullOrderSummary()
		{
			//Arrange
			var order = GetRealisticOrder();
			_productRepo.Set_RealisticItems_InStock();
			_orderFulfillment.SetOrderFullfillmentReturn(order);

			//Act
			var orderSummary = _sut.PlaceOrder(order);

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
