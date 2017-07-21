using System.Collections.Generic;
using NSubstitute;
using OrderEntryMockingPractice.Models;
using OrderEntryMockingPractice.Services;
using Shouldly;

namespace OrderEntryMockingPracticeTests
{
	internal partial class OrderServiceTests
	{
		private static void SetServiceReturns(ITaxRateService taxRate, ICustomerRepository customerRepo,
			IOrderFulfillmentService orderFulfillment, Order validOrder)
		{
			SetTaxRateReturn(taxRate);
			Set_CustomerRepoReturn_ValidId(customerRepo);
			Set_OrderFullfillment_ReturnedConfirmation(orderFulfillment, validOrder);
		}

		private static OrderService Get_InitializedOrderService(Order validOrder, IProductRepository productRepo)
		{
			var orderFulfillment = Substitute.For<IOrderFulfillmentService>();
			var taxRate = Substitute.For<ITaxRateService>();
			var customerRepo = Substitute.For<ICustomerRepository>();
			var email = Substitute.For<IEmailService>();

			SetTaxRateReturn(taxRate);
			Set_CustomerRepoReturn_ValidId(customerRepo);
			Set_OrderFullfillment_ReturnedConfirmation(orderFulfillment, validOrder);


			var sut = new OrderService(productRepo, orderFulfillment, taxRate, customerRepo, email);
			return sut;
		}

		private static Order Get_ValidOrder()
		{
			var validOrder = Get_OrderFromOrderItems(Get_OrderItemsWithoutDuplicates());
			return validOrder;
		}

		private static Order Get_InvalidOrder_RepeatedSkus()
		{
			var orderItems = Get_OrderItems_WithDuplicateSkus();
			var invalidOrder = Get_OrderFromOrderItems(orderItems);
			return invalidOrder;
		}



		private static void Set_CustomerRepoReturn_ValidId(ICustomerRepository customerRepo)
		{
			customerRepo.Get(1).Returns(new Customer()
			{
				CustomerId = 1,
				EmailAddress = "test@test.com",
				PostalCode = "postal code",
				Country = "country"
			});
		}

		private static void SetTaxRateReturn(ITaxRateService taxRate)
		{
			taxRate.GetTaxEntries(Arg.Any<string>(), Arg.Any<string>()).Returns(new List<TaxEntry>()
			{
				new TaxEntry()
				{
					Description = "State Tax",
					Rate = 5.6m
				},
				new TaxEntry()
				{
					Description = "Federal Tax",
					Rate = 8.2m
				}
			});
		}

		private static void Set_OrderFullfillment_ReturnedConfirmation(IOrderFulfillmentService orderFulfillment,
			Order validOrder)
		{
			orderFulfillment.Fulfill(validOrder).Returns(new OrderConfirmation()
			{
				CustomerId = 1,
				OrderId = 2,
				OrderNumber = "1337"
			});
		}

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

		private static Order Get_OrderWithNullCustomerId(List<OrderItem> orderItems)
		{
			return new Order()
			{
				CustomerId = null,
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
						Price = 43.5m,
						ProductId = 1,
						Sku = "1"
					},
					Quantity = 2
				},
				new OrderItem()
				{
					Product = new Product()
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
						Price = 43.5m,
						ProductId = 1,
						Sku = "1"
					},
					Quantity = 2
				},
				new OrderItem()
				{
					Product = new Product()
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

		//TODO FINISH Below Here
		private static List<OrderItem> Get_ValidRealisticOrderItems()
		{
			var realisticOrderItems = new List<OrderItem>()
			{ 
				new OrderItem()
				{
					Product = new Product()
					{
						Description = "This is a riveting description of a lamp.",
						Name = "Lamp",
						Price = 24.99m,
						ProductId = 1,
						Sku = "1-1989-5"
					},
					Quantity = 2
				},
				new OrderItem()
				{
					Product = new Product()
					{
						Description = "This is another great description, but of a (big) fan!",
						Name = "Fan",
						Price = 389.99m,
						ProductId = 2,
						Sku = "1-1989-6" 
					},
					Quantity = 1
				},
				new OrderItem()
				{
					Product = new Product()
					{
						Description = "Photo album description",
						Name = "Photo Album",
						Price = 24.49m,
						ProductId = 3,
						Sku = "1-2032-89"
					},
					Quantity = 4
				},
				new OrderItem()
				{
					Product = new Product()
					{
						Description = "Sand Paper description",
						Name = "240 Grit Sandpaper",
						Price = 15.16m,
						ProductId = 4,
						Sku = "2-0001-43"
					},
					Quantity = 100
				},
				new OrderItem()
				{
					Product = new Product()
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

		private static IProductRepository Get_Realistic_MockProductRepo()
		{
			var productRepo = Substitute.For<IProductRepository>();
			Set_ValidRealisticItems_InStock(productRepo);

			return productRepo;
		}

		private static void Set_ValidRealisticItems_InStock(IProductRepository productRepo)
		{
			productRepo.IsInStock("3-2000-14").Returns(true);
			productRepo.IsInStock("2-0001-43").Returns(true);
			productRepo.IsInStock("1-2032-89").Returns(true);
			productRepo.IsInStock("1-1989-6").Returns(true);
			productRepo.IsInStock("1-1989-5").Returns(true);
		}

	}
}
 