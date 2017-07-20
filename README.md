# Purpose
The purpose of this project is to provide a practice area for learning to use a mocking framework in C#.

# Instructions
Fork this repository into your own account.
Using your favorite unit test library and mocking framework, write tests for the following requirements.
Do not implement any of the existing interfaces.
Any implementation code you write should exist primarily in the methods being tested.

# Test Cases
## OrderService.PlaceOrder Specification
### Order Validity
An order is valid if
1. OrderItems are unique by product sku
2. All products are in stock

Otherwise, an exception should be thrown containing a list of reasons why the order is not valid.

### On Valid Order
* If order is valid, an OrderSummary is returned
  * it is submitted to the OrderFulfillmentService.
  * containing the order fulfillment confirmation number.
  * containing the id generated by the OrderFulfillmentService
  * containing applicable taxes for the customer
  * NetTotal = SUM(Product.Quantity * Product.Price)
  * OrderTotal = SUM(TaxEntry.Rate * NetTotal)
  * an confirmation email is sent to the customer.
* Customer information can be retrieved from the CustomerRepository
* Taxes can be retrieved from the TaxRateService
* The ProductRepository can be used to determine if the products are in stock.
