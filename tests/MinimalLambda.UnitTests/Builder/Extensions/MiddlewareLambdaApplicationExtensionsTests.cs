using Microsoft.Extensions.Hosting;

namespace MinimalLambda.UnitTests.Application.Extensions;

[TestSubject(typeof(MiddlewareLambdaApplicationExtensions))]
public class MiddlewareLambdaApplicationExtensionsTests
{
    private static IHost CreateHostWithServices() =>
        new LambdaApplicationBuilder(new LambdaApplicationOptions()).Build();

    [Fact]
    public void UseMiddleware_WithNullApplication_ThrowsArgumentNullException()
    {
        // Arrange
        ILambdaInvocationBuilder? application = null;
        Func<ILambdaInvocationContext, LambdaInvocationDelegate, Task> middleware = async (_, _) =>
            await Task.CompletedTask;

        // Act
        var act = () => application!.UseMiddleware(middleware);

        // Assert
        act.Should().ThrowExactly<ArgumentNullException>();
    }

    [Fact]
    public void UseMiddleware_WithValidMiddleware_ReturnsBuilder()
    {
        // Arrange
        var host = CreateHostWithServices();
        var app = new LambdaApplication(host);
        Func<ILambdaInvocationContext, LambdaInvocationDelegate, Task> middleware = async (_, _) =>
            await Task.CompletedTask;

        // Act
        var result = app.UseMiddleware(middleware);

        // Assert
        result.Should().Be(app);
    }

    [Fact]
    public void UseMiddleware_WithValidMiddleware_AddsMiddlewareToApplication()
    {
        // Arrange
        var host = CreateHostWithServices();
        var app = new LambdaApplication(host);
        Func<ILambdaInvocationContext, LambdaInvocationDelegate, Task> middleware = async (_, _) =>
            await Task.CompletedTask;

        // Act
        app.UseMiddleware(middleware);

        // Assert
        app.Middlewares.Should().HaveCount(1);
    }

    [Fact]
    public void UseMiddleware_EnablesMethodChaining()
    {
        // Arrange
        var host = CreateHostWithServices();
        var app = new LambdaApplication(host);
        Func<ILambdaInvocationContext, LambdaInvocationDelegate, Task> middleware = async (_, _) =>
            await Task.CompletedTask;

        // Act
        var result = app.UseMiddleware(middleware).UseMiddleware(middleware);

        // Assert
        result.Should().Be(app);
        app.Middlewares.Should().HaveCount(2);
    }

    [Fact]
    public async Task UseMiddleware_CallsMiddlewareWithContextAndNext()
    {
        // Arrange
        var host = CreateHostWithServices();
        var app = new LambdaApplication(host);
        var middlewareWasCalled = false;
        ILambdaInvocationContext? capturedContext = null;
        LambdaInvocationDelegate? capturedNext = null;

        Func<ILambdaInvocationContext, LambdaInvocationDelegate, Task> middleware = async (
            context,
            next
        ) =>
        {
            middlewareWasCalled = true;
            capturedContext = context;
            capturedNext = next;
            await next(context);
        };

        app.UseMiddleware(middleware);
        LambdaInvocationDelegate handler = async _ => await Task.CompletedTask;
        app.Handle(handler);

        var builtPipeline = app.Build();

        // Act
        var mockContext = Substitute.For<ILambdaInvocationContext>();
        await builtPipeline(mockContext);

        // Assert
        middlewareWasCalled.Should().BeTrue();
        capturedContext.Should().Be(mockContext);
        capturedNext.Should().NotBeNull();
    }
}
