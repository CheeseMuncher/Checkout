# The Brief:

### Part 1:
Your company has decided to create a new line of business.  As a start to this effort, they’ve come to you to help develop a prototype.  It is expected that this prototype will be part of a beta test with some actual customers, and if successful, it is likely that the prototype will be expanded into a full product.

Your part of the prototype will be to develop a Web API that will be used by customers to manage a basket of items. The business describes the basic workflow is as follows:

_This API will allow our users to set up and manage an order of items.  The API will allow users to add and remove items and change the quantity of the items they want.  They should also be able to simply clear out all items from their order and start again._

The functionality to complete the purchase of the items will be handled separately and will be written by a different team once this prototype is complete.  

For the purpose of this exercise, you can assume there’s an existing data storage solution that the API will use, so you can either create stubs for that functionality or simply hold data in memory.

Feel free to make any assumptions whenever you are not certain about the requirements, but make sure your assumptions are made clear either through the design or additional documentation.

### Part 2:
Create a client library that makes use of the API endpoints created in Part 1.  The purpose of this code to provide authors of client applications a simple framework to use in their applications.

If we decide to bring you in for further discussions, you should be prepared to explain and defend any coding and design decisions you make as a part of this exercise.

**All code should be written in C# and target the .NET framework library version 4.5 or higher, or .NET core.  Please check all code into a publicly accessible repository on GitHub and send us a link to your repository.**


# The Solution

### Instructions for the demo:
1. Clone this repository: https://github.com/CheeseMuncher/Checkout
2. Build solution and run - it should show some orders on https://localhost:44315/api/orders
3. There is a Postman collection in the CheckoutApi directory to test the api calls directly
4. Install node packages:
	* Point command line to the CheckoutWebClient directory
	* npm install express
	* npm install jquery
5. Run web client project in CheckoutWebClient directory with VS code
6. You should be able to load the landing page: http://localhost:45000/default.html
7. Password is .Test1ng
 
### General comments

##### Style: 

* My personal preference is to be as consistent as possible throughout a project. I've used my personal preference in terms of code style. I don't have a problem adhering to existing coding standards when joining a new project - consistency is arguably more important than personal preference
* I believe in the principle of clean code. Specifically, naming should be descriptive and unambiguous yet as concise as possible to the point where documentation headers become unnecessary. The exception to this is where a third party might be consuming our code, in which case I believe it is best to lean towards a little more information than is stricly necessary. As such, plenty of my methods have limited documentation headers or none at all.  
    For example, if I have a method called `GetOrder(int id)`, I don't see the value of specifying the fact that the `id` parameter is an identifier because it should be obvious. If anyone disagrees with my individual choices at the PR stage I'm happy to discuss it. If a fellow developer with less domain knowledge tells me "It's not obvious", then I would review the situation and try to make things more obvious or improve the documentation to make it so.
* I believe in the rule of three - for DRY violations and other situations. For example I have put a couple of `TODO`s in the `OrdersController` with comments pertaining to the use of an abstract base class. There is a DRY violation in that the SkusController is duplicating the code under one of these `TODO` items. The next time such a DRY violation occurs, then the rule of three would be met and that is when I would implement the abstract base class.  
Another example is the `RegisterApplicationComponents` in the `Startup` class. Different services would almost certainly have the same dependencies, such as `ILogger` and `IRepository`. Once a third module were added, the rule of three would be met and I would add code to handle duplicate registrations.

##### Defensive coding:

I believe that code should be written and designed to handle unexpected situations for the benefit of the consumer, for example:
* In the `OrderService.GetOrder()` method, I don't know if the `IRepository` implementation would return `null` or an empty list if no results are found, therefore I chose to handle both situations. If a new implementation was injected instead with slightly different behaviour, it should still work.
* Likewise, I would expect the `IRepository` implementation to return single objects when queried by `id`, but I chose to handle duplicates being returned. 
* There is some duplicate validation in the API in that some of the code validated by a controller is also validated at service level. I think it's better to invest a little time to ensure an optimum experience for the consumer beforehand than to deal with the aftermath of a system failure.

### Specific comments on the implementation

##### Assumptions:
* I've made the assumption that the `IRepository` updates lines when saving orders, including deleting lines rather than the service being responsible for multiple calls to modify order lines.

##### Design Choices:
* I've used the concept of public and private error messages. This is intended as a customer facing API and as such will only return error messages to the client for `BadRequest` or `NotFound` responses. A different API could be easily built on the same service layer for internal consumption, for example for a Customer Services department which could return error messages for other response types. Full error information including stack traces are logged (once the `ILogger` is implemented)
* The controllers are there to authenticate (not implemented yet) and to orchestrate different service calls as is the case in the `OrdersController.Clear()` method. As mentioned above, different controllers could be built quickly to carry out the same activities yet service different types of consumer.
* I've defined a very simple `SkuModel`. The `DisplayName` property is to support internationalisation. It should be straight forward to extend this to include other properties such as stock levels, e.g. "Only 6 left in stock", consumption behaviour, e.g. "42 purchased this month" or product pricing models e.g. "1+ at unit price X, 10+ at unit price Y, etc" or "buy 1 at price X, buy Y units for price Z"
* I've created one controller for Skus, one for Orders which also handles line updates on an order. If more sophisticated line operations were required, a new controller could be added, so long as the API stayed the same from the consumer's point of view. Likewise, I would add another service dedicated to order line operations once this became necessary
* DI design: services define their own dependencies and expose them. If dependencies change, the controller layer doesn't need to change.
* Potential violations of SRP: it could be argued that there is some order line specific functionality on the `OrderService` and the `OrderController`. As mentioned above, order line specific classes may be more appropriate or may become so in future. This is the kind of thing I would discuss with a fellow developer with more experience, more domain knowledge or more visibility of the product roadmap before making a decision. The current implementation is, in my opinion, an acceptable version 1.0 product.  
    The `UpdateOrderLine()` method on the order service could be construed as doing two things and violating SRP. Arguably its main responsibility is to update lines on an order, which could include add or updating them. As above, this could be resolved with a quick discussion and is probably an acceptable situation for a prototype. Furthermore it is still a small method, if more complex business logic were required, it could be split up then.
* The brief states that I could "either create stubs for [data storage] functionality or simply hold data in memory". I've chosen to implement a file based storage solution for the demo as this is a little more persistent in the event of a run-time exception. On the downside, this would be more likely to cause issues with respect to different operating systems and environments (see my comments on CI below)

##### Extra Mile:

* Use of async await: I chose not to implement this, very deliberately. I've seen this used just for the sake of it in the past and it can add unnecessary complication. I tried to spam my application manually on the not very powerful windows machine I have access to at home, the user experience was fine. If the prototype was approved and scale up was planned, this would be the right time to discuss such issues. It might be that a caching layer would be a more appropriate solution to deal with additional load to the system. 
* There is currently no authentication at api level or in the client application (there's a placeholder for this on the landing page). Integrating with the company's existing authentication framework would be the highest priority next step for this prototype.
* Continuous Integration, Continuous Delivery and Continuous Deployment: I haven't included this in my submission for a few reasons: I don't have the infrastructure to set these up at present. I have experience in modifying these practices but not in setting them up from scratch. I am delivering a prototype as is that meets the brief. This prototype will undergo some beta testing. If a business decision is made to launch this as a formal product, then that would be the appropriate time to integrate the project with the company's existing software delivery infrastructure. If the beta test isn't successful then less effort would have been spent for nothing. In fact this argument can also apply to my comments on async await above.  
I created a file based implementation of the `IRepository`, with a handful of tests. The tests would almost certainly result in issues with automated test runs.


### Next Steps

Below is a list of actions that would be needed to convert this prototype to a formal product, in the order that I feel is appropriately prioritised. The prioritisation would have to be agreed with the business and technical colleagues of course.


* Authentication: integrate with existing company authentication framework
* CI/CD/CD: integrate with existing company infrastructure
* Integrate with existing company data storage infrastructure
* Update the API to provide stock levels and pricing models for products. Consume these on the client side.
* Implement logging or integrate with existing logging framework
* Internationalisation if appropriate - use resource keys instead of hard coded strings for the public error messages for example
* Implement the TODOs in the solution where appropriate
* Update the swagger documentation to match the company's branding

Thank you for taking the time to look at my code. All feedback, no matter how brutal, is always welcome.
