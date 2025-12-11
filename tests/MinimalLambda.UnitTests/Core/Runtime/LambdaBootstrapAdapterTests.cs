using Microsoft.Extensions.Options;

namespace MinimalLambda.UnitTests.Core.Runtime;

[TestSubject(typeof(LambdaBootstrapAdapter))]
public class LambdaBootstrapAdapterTest
{
    [Fact]
    public void Constructor_WithNullOptions_ThrowsArgumentNullException()
    {
        // Act & Assert
        var act = () => new LambdaBootstrapAdapter(null!);
        act.Should().ThrowExactly<ArgumentNullException>();
    }

    [Theory]
    [AutoNSubstituteData]
    public void Constructor_WithValidOptions_Succeeds(IOptions<LambdaHostOptions> options)
    {
        // Act
        var adapter = new LambdaBootstrapAdapter(options);

        // Assert
        adapter.Should().BeAssignableTo<ILambdaBootstrapOrchestrator>();
    }
}
