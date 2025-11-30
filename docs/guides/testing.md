# Testing

!!! info "Coming Soon"
    This guide is currently under development. Check back soon for comprehensive testing documentation covering:

    - Unit testing Lambda handlers
    - Testing with xUnit, NSubstitute, and AutoFixture
    - Testing middleware components
    - Testing lifecycle handlers (OnInit, OnShutdown)
    - Integration testing strategies
    - Mocking AWS Lambda context
    - Best practices and patterns

---

## Temporary Resources

While this guide is being developed, you can refer to:

- **[CONTRIBUTING.md](https://github.com/j-d-ha/aws-lambda-host/blob/main/CONTRIBUTING.md)** - Testing patterns and conventions used in the project
- **[Example test files](https://github.com/j-d-ha/aws-lambda-host/tree/main/tests/AwsLambda.Host.UnitTests)** - Real test examples from the framework
- **[AutoNSubstituteData pattern](https://github.com/j-d-ha/aws-lambda-host/blob/main/tests/AwsLambda.Host.UnitTests/AutoNSubstituteDataAttribute.cs)** - Test data generation attribute

---

## Quick Testing Example

```csharp title="OrderServiceTests.cs" linenums="1"
using NSubstitute;
using Xunit;

public class OrderServiceTests
{
    [Fact]
    public async Task ProcessAsync_ValidOrder_ReturnsSuccess()
    {
        // Arrange
        var repository = Substitute.For<IOrderRepository>();
        repository.SaveAsync(Arg.Any<Order>())
            .Returns(new SaveResult { Success = true });

        var service = new OrderService(repository);
        var order = new Order("123", 99.99m);

        // Act
        var result = await service.ProcessAsync(order);

        // Assert
        Assert.True(result.Success);
        await repository.Received(1).SaveAsync(order);
    }
}
```

---

## Next Steps

- **[Deployment](deployment.md)** - Deploy your tested Lambda functions
- **[Error Handling](error-handling.md)** - Test error scenarios
- **[Handler Registration](handler-registration.md)** - Understand handler patterns for testing
