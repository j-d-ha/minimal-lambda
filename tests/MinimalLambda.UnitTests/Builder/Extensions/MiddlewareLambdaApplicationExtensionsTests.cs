using Microsoft.Extensions.DependencyInjection;
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

    [Fact]
    public void UseMiddlewareFactory_WithNullApplication_ThrowsArgumentNullException()
    {
        // Arrange
        ILambdaInvocationBuilder? application = null;

        // Act
        var act = () => application!.UseMiddleware<TestMiddlewareFactory>();

        // Assert
        act.Should().ThrowExactly<ArgumentNullException>();
    }

    [Fact]
    public void UseMiddlewareFactory_WithValidFactory_AddsMiddlewareToApplication()
    {
        // Arrange
        var builder = new LambdaApplicationBuilder(new LambdaApplicationOptions());
        builder.Services.AddSingleton<MiddlewareTracker>();
        builder.Services.AddTransient<TestMiddlewareFactory>();
        var host = builder.Build();
        var app = new LambdaApplication(host);

        // Act
        app.UseMiddleware<TestMiddlewareFactory>();

        // Assert
        app.Middlewares.Should().HaveCount(1);
    }

    [Fact]
    public async Task UseMiddlewareFactory_ResolvesFactoryAndInvokesMiddleware()
    {
        // Arrange
        var builder = new LambdaApplicationBuilder(new LambdaApplicationOptions());
        var tracker = new MiddlewareTracker();
        builder.Services.AddSingleton(tracker);
        builder.Services.AddTransient<TestMiddlewareFactory>();
        var host = builder.Build();
        var app = new LambdaApplication(host);
        app.UseMiddleware<TestMiddlewareFactory>();
        app.Handle(_ => Task.CompletedTask);
        var pipeline = app.Build();
        var context = Substitute.For<ILambdaInvocationContext>();
        context.ServiceProvider.Returns(host.Services);

        // Act
        await pipeline(context);

        // Assert
        tracker.CreateCount.Should().Be(1);
        tracker.InvokeCount.Should().Be(1);
    }

    [Fact]
    public async Task UseMiddlewareFactory_DisposesDisposableMiddleware()
    {
        // Arrange
        var builder = new LambdaApplicationBuilder(new LambdaApplicationOptions());
        var tracker = new MiddlewareTracker();
        builder.Services.AddSingleton(tracker);
        builder.Services.AddTransient<DisposableMiddlewareFactory>();
        var host = builder.Build();
        var app = new LambdaApplication(host);
        app.UseMiddleware<DisposableMiddlewareFactory>();
        app.Handle(_ => Task.CompletedTask);
        var pipeline = app.Build();
        var context = Substitute.For<ILambdaInvocationContext>();
        context.ServiceProvider.Returns(host.Services);

        // Act
        await pipeline(context);

        // Assert
        tracker.DisposeCount.Should().Be(1);
    }

    [Fact]
    public async Task UseMiddlewareFactory_DisposesAsyncDisposableMiddleware()
    {
        // Arrange
        var builder = new LambdaApplicationBuilder(new LambdaApplicationOptions());
        var tracker = new MiddlewareTracker();
        builder.Services.AddSingleton(tracker);
        builder.Services.AddTransient<AsyncDisposableMiddlewareFactory>();
        var host = builder.Build();
        var app = new LambdaApplication(host);
        app.UseMiddleware<AsyncDisposableMiddlewareFactory>();
        app.Handle(_ => Task.CompletedTask);
        var pipeline = app.Build();
        var context = Substitute.For<ILambdaInvocationContext>();
        context.ServiceProvider.Returns(host.Services);

        // Act
        await pipeline(context);

        // Assert
        tracker.AsyncDisposeCount.Should().Be(1);
    }

    private sealed class MiddlewareTracker
    {
        public int CreateCount { get; private set; }
        public int InvokeCount { get; private set; }
        public int DisposeCount { get; private set; }
        public int AsyncDisposeCount { get; private set; }

        public void RecordCreate() => CreateCount++;

        public void RecordInvoke() => InvokeCount++;

        public void RecordDispose() => DisposeCount++;

        public void RecordAsyncDispose() => AsyncDisposeCount++;
    }

    private sealed class TestMiddlewareFactory(MiddlewareTracker tracker) : ILambdaMiddlewareFactory
    {
        public ILambdaMiddleware Create()
        {
            tracker.RecordCreate();
            return new TestMiddleware(tracker);
        }
    }

    private sealed class DisposableMiddlewareFactory(MiddlewareTracker tracker)
        : ILambdaMiddlewareFactory
    {
        public ILambdaMiddleware Create()
        {
            tracker.RecordCreate();
            return new DisposableMiddleware(tracker);
        }
    }

    private sealed class AsyncDisposableMiddlewareFactory(MiddlewareTracker tracker)
        : ILambdaMiddlewareFactory
    {
        public ILambdaMiddleware Create()
        {
            tracker.RecordCreate();
            return new AsyncDisposableMiddleware(tracker);
        }
    }

    private sealed class TestMiddleware(MiddlewareTracker tracker) : ILambdaMiddleware
    {
        public Task InvokeAsync(ILambdaInvocationContext context, LambdaInvocationDelegate next)
        {
            tracker.RecordInvoke();
            return next(context);
        }
    }

    private sealed class DisposableMiddleware(MiddlewareTracker tracker)
        : ILambdaMiddleware,
            IDisposable
    {
        public Task InvokeAsync(ILambdaInvocationContext context, LambdaInvocationDelegate next)
        {
            tracker.RecordInvoke();
            return next(context);
        }

        public void Dispose() => tracker.RecordDispose();
    }

    private sealed class AsyncDisposableMiddleware(MiddlewareTracker tracker)
        : ILambdaMiddleware,
            IAsyncDisposable
    {
        public Task InvokeAsync(ILambdaInvocationContext context, LambdaInvocationDelegate next)
        {
            tracker.RecordInvoke();
            return next(context);
        }

        public ValueTask DisposeAsync()
        {
            tracker.RecordAsyncDispose();
            return ValueTask.CompletedTask;
        }
    }
}
