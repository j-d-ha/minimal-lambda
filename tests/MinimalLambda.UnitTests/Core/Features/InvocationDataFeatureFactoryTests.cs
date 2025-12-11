namespace MinimalLambda.UnitTests.Core.Features;

[TestSubject(typeof(InvocationDataFeatureFactory))]
public class InvocationDataFeatureFactoryTests
{
    [Fact]
    public void Create_ReturnsIInvocationDataFeature()
    {
        // Arrange
        var factory = new InvocationDataFeatureFactory();
        using var eventStream = new MemoryStream();

        // Act
        var feature = factory.Create(eventStream);

        // Assert
        feature.Should().BeAssignableTo<IInvocationDataFeature>();
    }

    [Fact]
    public void Create_SetsEventStreamToProvidedStream()
    {
        // Arrange
        var factory = new InvocationDataFeatureFactory();
        using var eventStream = new MemoryStream();

        // Act
        var feature = factory.Create(eventStream);

        // Assert
        feature.EventStream.Should().BeSameAs(eventStream);
    }

    [Fact]
    public void Create_InitializesResponseStreamToMemoryStream()
    {
        // Arrange
        var factory = new InvocationDataFeatureFactory();
        using var eventStream = new MemoryStream();

        // Act
        var feature = factory.Create(eventStream);

        // Assert
        feature.ResponseStream.Should().BeAssignableTo<MemoryStream>();
    }

    [Fact]
    public void Create_ReturnsNewInstanceEachCall()
    {
        // Arrange
        var factory = new InvocationDataFeatureFactory();
        using var eventStream1 = new MemoryStream();
        using var eventStream2 = new MemoryStream();

        // Act
        var feature1 = factory.Create(eventStream1);
        var feature2 = factory.Create(eventStream2);

        // Assert
        feature1.Should().NotBeSameAs(feature2);
    }

    [Fact]
    public void Create_ReturnsIDisposable()
    {
        // Arrange
        var factory = new InvocationDataFeatureFactory();
        using var eventStream = new MemoryStream();

        // Act
        var feature = factory.Create(eventStream);

        // Assert
        feature.Should().BeAssignableTo<IDisposable>();
    }
}
