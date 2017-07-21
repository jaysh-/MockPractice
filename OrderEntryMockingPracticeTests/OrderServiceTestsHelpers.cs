using System.Collections.Generic;
using NSubstitute;
using OrderEntryMockingPractice.Models;
using OrderEntryMockingPractice.Services;

namespace OrderEntryMockingPracticeTests
{
	internal partial class OrderServiceTests
	{
		private static IProductRepository Get_AlwaysOutOfStock_MockProductRepo()
		{
			var productRepo = Substitute.For<IProductRepository>();
			productRepo.IsInStock(Arg.Any<string>()).Returns(false);

			return productRepo;
		}

		private static IProductRepository Get_AlwaysStocked_MockProductRepo()
		{
			var productRepo = Substitute.For<IProductRepository>();
			productRepo.IsInStock(Arg.Any<string>()).Returns(true);

			return productRepo;
		}

		private static OrderService Get_OrderService(Order order, IProductRepository productRepo)
		{
			var orderFulfillment = Substitute.For<IOrderFulfillmentService>();
			var taxRate = Substitute.For<ITaxRateService>();
			var customerRepo = Substitute.For<ICustomerRepository>();
			var email = Substitute.For<IEmailService>();

			Set_TaxRateServiceReturn_GetTaxEntries(taxRate);
			Set_CustomerRepoReturn_CustomerWithId1(customerRepo);
			Set_OrderFullfillmentReturn_ForProvidedOrder(orderFulfillment, order);

			return new OrderService(productRepo, orderFulfillment, taxRate, customerRepo, email);
		}

		private static Order Get_RealisticInStockOrderWithoutDuplicates()
		{
			return new Order
			{
				CustomerId = 1,
				OrderItems = Get_ValidRealisticOrderItems()
			};
		}

		private static List<OrderItem> Get_OrderItems_WithDuplicateSkus()
		{
			var orderItems = new List<OrderItem>
			{
				new OrderItem
				{
					Product = new Product
					{
						Description = "a",
						Name = "a",
						Price = 43.5m,
						ProductId = 1,
						Sku = "1"
					},
					Quantity = 2
				},
				new OrderItem
				{
					Product = new Product
					{
						Description = "A",
						Name = "A",
						Price = 1.2m,
						ProductId = 1,
						Sku = "1"
					},
					Quantity = 2.5m
				}
			};
			return orderItems;
		}

		private static List<OrderItem> Get_OrderItems_WithoutDuplicateSkus()
		{
			var orderItems = new List<OrderItem>
			{
				new OrderItem
				{
					Product = new Product
					{
						Description = "a",
						Name = "a",
						Price = 43.5m,
						ProductId = 1,
						Sku = "1"
					},
					Quantity = 2
				},
				new OrderItem
				{
					Product = new Product
					{
						Description = "b",
						Name = "b",
						Price = 1.2m,
						ProductId = 2,
						Sku = "2"
					},
					Quantity = 2.5m
				}
			};

			return orderItems;
		}

		private static Order Get_OrderWithNullCustomerId()
		{
			return new Order
			{
				CustomerId = null,
				OrderItems = Get_OrderItems_WithoutDuplicateSkus()
			};
		}
		private static Order Get_Order(List<OrderItem> orderItems)
		{
			return new Order
			{
				CustomerId = 1,
				OrderItems = orderItems
			};
		}

		private static Order Get_Order_WithoutDuplicates()
		{
			return Get_Order(Get_OrderItems_WithoutDuplicateSkus());
		}

		private static Order Get_Order_WithRepeatedSkus()
		{
			return Get_Order(Get_OrderItems_WithDuplicateSkus());
		}

		private static IProductRepository Get_RealisticAndStocked_MockProductRepo()
		{
			var productRepo = Substitute.For<IProductRepository>();
			Set_RealisticItems_InStock(productRepo);

			return productRepo;
		}

		private static List<OrderItem> Get_ValidRealisticOrderItems()
		{
			var realisticOrderItems = new List<OrderItem>
			{
				new OrderItem
				{
					Product = new Product
					{
						Description = "This is a riveting description of a lamp.",
						Name = "Lamp",
						Price = 24.99m,
						ProductId = 1,
						Sku = "1-1989-5"
					},
					Quantity = 2
				},
				new OrderItem
				{
					Product = new Product
					{
						Description = "This is another great description, but of a (big) fan!",
						Name = "Fan",
						Price = 389.99m,
						ProductId = 2,
						Sku = "1-1989-6"
					},
					Quantity = 1
				},
				new OrderItem
				{
					Product = new Product
					{
						Description = "Photo album description",
						Name = "Photo Album",
						Price = 24.49m,
						ProductId = 3,
						Sku = "1-2032-89"
					},
					Quantity = 4
				},
				new OrderItem
				{
					Product = new Product
					{
						Description = "Sand Paper description",
						Name = "240 Grit Sandpaper",
						Price = 15.16m,
						ProductId = 4,
						Sku = "2-0001-43"
					},
					Quantity = 100
				},
				new OrderItem
				{
					Product = new Product
					{
						Description = "Couch description",
						Name = "Leather Couch",
						Price = 659.93m,
						ProductId = 5,
						Sku = "3-2000-14"
					},
					Quantity = 1
				}
			};

			return realisticOrderItems;
		}


		private static void Set_CustomerRepoReturn_CustomerWithId1(ICustomerRepository customerRepo)
		{
			customerRepo.Get(1).Returns(new Customer
			{
				CustomerId = 1,
				EmailAddress = "test@test.com",
				PostalCode = "postal code",
				Country = "country"
			});
		}

		private static void Set_OrderFullfillmentReturn_ForProvidedOrder(IOrderFulfillmentService orderFulfillment,
			Order order)
		{
			orderFulfillment.Fulfill(order).Returns(new OrderConfirmation
			{
				CustomerId = 1,
				OrderId = 2,
				OrderNumber = "1337"
			});
		}

		private static void Set_RealisticItems_InStock(IProductRepository productRepo)
		{
			//Could also iterate over the products and add their skus 
			productRepo.IsInStock("3-2000-14").Returns(true);
			productRepo.IsInStock("2-0001-43").Returns(true);
			productRepo.IsInStock("1-2032-89").Returns(true);
			productRepo.IsInStock("1-1989-6").Returns(true);
			productRepo.IsInStock("1-1989-5").Returns(true);
		}

		private static void Set_MockedServiceReturns(ITaxRateService taxRate, ICustomerRepository customerRepo,
			IOrderFulfillmentService orderFulfillment, Order order)
		{
			Set_TaxRateServiceReturn_GetTaxEntries(taxRate);
			Set_CustomerRepoReturn_CustomerWithId1(customerRepo);
			Set_OrderFullfillmentReturn_ForProvidedOrder(orderFulfillment, order);
		}

		private static void Set_TaxRateServiceReturn_GetTaxEntries(ITaxRateService taxRate)
		{
			taxRate.GetTaxEntries(Arg.Any<string>(), Arg.Any<string>()).Returns(new List<TaxEntry>
			{
				new TaxEntry
				{
					Description = "State Tax",
					Rate = 5.6m
				},
				new TaxEntry
				{
					Description = "Federal Tax",
					Rate = 8.2m
				}
			});
		}
	}
}