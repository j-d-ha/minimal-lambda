using JetBrains.Annotations;

namespace MinimalLambda.Testing.UnitTests;

[TestSubject(typeof(DictionaryExtensions))]
public class DictionaryExtensionsTest
{
    private readonly Dictionary<string, string> _dictionary = new()
    {
        ["key1"] = "value1",
        ["key2"] = "value2",
    };

    [Fact]
    public void GetRequired_WhenKeyExists_ReturnsValue()
    {
        _dictionary.GetRequired("key1", out var value);

        value.Should().Be("value1");
    }

    [Fact]
    public void AddRequired_WhenKeyAlreadyExists_ThrowsInvalidOperationException()
    {
        var act = () => _dictionary.AddRequired("key1", "newValue");

        act.Should().Throw<InvalidOperationException>().WithMessage("Key 'key1' already exists.");
    }

    [Fact]
    public void AddRequired_WhenKeyIsNew_AddsValue()
    {
        var act = () => _dictionary.AddRequired("key3", "value3");

        act.Should().NotThrow();
        _dictionary["key3"].Should().Be("value3");
    }

    [Fact]
    public void GetRequired_WhenKeyIsMissing_ThrowsInvalidOperationException()
    {
        var act = () => _dictionary.GetRequired("missing", out _);

        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("Key 'missing' is null or does not exist.");
    }
}
