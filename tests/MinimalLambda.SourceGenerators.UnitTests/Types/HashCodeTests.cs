using AwesomeAssertions;
using JetBrains.Annotations;
using HashCode = MinimalLambda.Host.SourceGenerators.Types.HashCode;

namespace MinimalLambda.SourceGenerators.UnitTests.Types;

[TestSubject(typeof(HashCode))]
public class HashCodeTests
{
    #region Combine<T1> Method Tests

    [Fact]
    public void Combine_WithSingleIntValue_ReturnsValidHashCode()
    {
        // Arrange
        var value = 42;

        // Act
        var hash = HashCode.Combine(value);

        // Assert
        hash.Should().NotBe(0);
    }

    [Fact]
    public void Combine_WithSingleNullValue_ReturnsValidHashCode()
    {
        // Arrange
        string? value = null;

        // Act
        var hash = HashCode.Combine(value);

        // Assert
        hash.Should().NotBe(0);
    }

    [Fact]
    public void Combine_WithSingleStringValue_ReturnsValidHashCode()
    {
        // Arrange
        var value = "test";

        // Act
        var hash = HashCode.Combine(value);

        // Assert
        hash.Should().NotBe(0);
    }

    [Fact]
    public void Combine_WithSameIntValue_ReturnsSameHash()
    {
        // Arrange
        var value = 42;

        // Act
        var hash1 = HashCode.Combine(value);
        var hash2 = HashCode.Combine(value);

        // Assert
        hash1.Should().Be(hash2);
    }

    [Fact]
    public void Combine_WithDifferentIntValues_ReturnsDifferentHashes()
    {
        // Arrange
        var value1 = 42;
        var value2 = 43;

        // Act
        var hash1 = HashCode.Combine(value1);
        var hash2 = HashCode.Combine(value2);

        // Assert
        hash1.Should().NotBe(hash2);
    }

    [Fact]
    public void Combine_WithDefaultIntValue_ReturnsValidHashCode()
    {
        // Arrange
        var value = default(int);

        // Act
        var hash = HashCode.Combine(value);

        // Assert
        hash.Should().NotBe(0);
    }

    #endregion

    #region Combine<T1, T2> Method Tests

    [Fact]
    public void Combine_WithTwoIntValues_ReturnsValidHashCode()
    {
        // Arrange
        var value1 = 1;
        var value2 = 2;

        // Act
        var hash = HashCode.Combine(value1, value2);

        // Assert
        hash.Should().NotBe(0);
    }

    [Fact]
    public void Combine_WithSameTwoIntValues_ReturnsSameHash()
    {
        // Arrange
        var value1 = 1;
        var value2 = 2;

        // Act
        var hash1 = HashCode.Combine(value1, value2);
        var hash2 = HashCode.Combine(value1, value2);

        // Assert
        hash1.Should().Be(hash2);
    }

    [Fact]
    public void Combine_WithDifferentTwoIntValues_ReturnsDifferentHashes()
    {
        // Arrange
        var hash1 = HashCode.Combine(1, 2);
        var hash2 = HashCode.Combine(1, 3);

        // Act & Assert
        hash1.Should().NotBe(hash2);
    }

    [Fact]
    public void Combine_WithMixedTypeTwoValues_ReturnsValidHashCode()
    {
        // Arrange
        var value1 = 42;
        var value2 = "test";

        // Act
        var hash = HashCode.Combine(value1, value2);

        // Assert
        hash.Should().NotBe(0);
    }

    [Fact]
    public void Combine_WithNullAndIntValues_ReturnsValidHashCode()
    {
        // Arrange
        string? value1 = null;
        var value2 = 42;

        // Act
        var hash = HashCode.Combine(value1, value2);

        // Assert
        hash.Should().NotBe(0);
    }

    #endregion

    #region Combine<T1, T2, T3> Method Tests

    [Fact]
    public void Combine_WithThreeIntValues_ReturnsValidHashCode()
    {
        // Arrange
        var value1 = 1;
        var value2 = 2;
        var value3 = 3;

        // Act
        var hash = HashCode.Combine(value1, value2, value3);

        // Assert
        hash.Should().NotBe(0);
    }

    [Fact]
    public void Combine_WithSameThreeIntValues_ReturnsSameHash()
    {
        // Arrange
        var value1 = 1;
        var value2 = 2;
        var value3 = 3;

        // Act
        var hash1 = HashCode.Combine(value1, value2, value3);
        var hash2 = HashCode.Combine(value1, value2, value3);

        // Assert
        hash1.Should().Be(hash2);
    }

    [Fact]
    public void Combine_WithDifferentThreeIntValues_ReturnsDifferentHashes()
    {
        // Arrange
        var hash1 = HashCode.Combine(1, 2, 3);
        var hash2 = HashCode.Combine(1, 2, 4);

        // Act & Assert
        hash1.Should().NotBe(hash2);
    }

    [Fact]
    public void Combine_WithMixedTypeThreeValues_ReturnsValidHashCode()
    {
        // Arrange
        var value1 = 42;
        var value2 = "test";
        var value3 = 3.14;

        // Act
        var hash = HashCode.Combine(value1, value2, value3);

        // Assert
        hash.Should().NotBe(0);
    }

    #endregion

    #region Combine<T1, T2, T3, T4> Method Tests

    [Fact]
    public void Combine_WithFourIntValues_ReturnsValidHashCode()
    {
        // Arrange
        var value1 = 1;
        var value2 = 2;
        var value3 = 3;
        var value4 = 4;

        // Act
        var hash = HashCode.Combine(value1, value2, value3, value4);

        // Assert
        hash.Should().NotBe(0);
    }

    [Fact]
    public void Combine_WithSameFourIntValues_ReturnsSameHash()
    {
        // Arrange
        var value1 = 1;
        var value2 = 2;
        var value3 = 3;
        var value4 = 4;

        // Act
        var hash1 = HashCode.Combine(value1, value2, value3, value4);
        var hash2 = HashCode.Combine(value1, value2, value3, value4);

        // Assert
        hash1.Should().Be(hash2);
    }

    [Fact]
    public void Combine_WithDifferentFourIntValues_ReturnsDifferentHashes()
    {
        // Arrange
        var hash1 = HashCode.Combine(1, 2, 3, 4);
        var hash2 = HashCode.Combine(1, 2, 3, 5);

        // Act & Assert
        hash1.Should().NotBe(hash2);
    }

    #endregion

    #region Combine<T1, T2, T3, T4, T5> Method Tests

    [Fact]
    public void Combine_WithFiveIntValues_ReturnsValidHashCode()
    {
        // Arrange
        var value1 = 1;
        var value2 = 2;
        var value3 = 3;
        var value4 = 4;
        var value5 = 5;

        // Act
        var hash = HashCode.Combine(value1, value2, value3, value4, value5);

        // Assert
        hash.Should().NotBe(0);
    }

    [Fact]
    public void Combine_WithSameFiveIntValues_ReturnsSameHash()
    {
        // Arrange
        var value1 = 1;
        var value2 = 2;
        var value3 = 3;
        var value4 = 4;
        var value5 = 5;

        // Act
        var hash1 = HashCode.Combine(value1, value2, value3, value4, value5);
        var hash2 = HashCode.Combine(value1, value2, value3, value4, value5);

        // Assert
        hash1.Should().Be(hash2);
    }

    #endregion

    #region Combine<T1, T2, T3, T4, T5, T6> Method Tests

    [Fact]
    public void Combine_WithSixIntValues_ReturnsValidHashCode()
    {
        // Arrange
        var value1 = 1;
        var value2 = 2;
        var value3 = 3;
        var value4 = 4;
        var value5 = 5;
        var value6 = 6;

        // Act
        var hash = HashCode.Combine(value1, value2, value3, value4, value5, value6);

        // Assert
        hash.Should().NotBe(0);
    }

    [Fact]
    public void Combine_WithSameSixIntValues_ReturnsSameHash()
    {
        // Arrange
        var value1 = 1;
        var value2 = 2;
        var value3 = 3;
        var value4 = 4;
        var value5 = 5;
        var value6 = 6;

        // Act
        var hash1 = HashCode.Combine(value1, value2, value3, value4, value5, value6);
        var hash2 = HashCode.Combine(value1, value2, value3, value4, value5, value6);

        // Assert
        hash1.Should().Be(hash2);
    }

    #endregion

    #region Combine<T1, T2, T3, T4, T5, T6, T7> Method Tests

    [Fact]
    public void Combine_WithSevenIntValues_ReturnsValidHashCode()
    {
        // Arrange
        var value1 = 1;
        var value2 = 2;
        var value3 = 3;
        var value4 = 4;
        var value5 = 5;
        var value6 = 6;
        var value7 = 7;

        // Act
        var hash = HashCode.Combine(value1, value2, value3, value4, value5, value6, value7);

        // Assert
        hash.Should().NotBe(0);
    }

    [Fact]
    public void Combine_WithSameSevenIntValues_ReturnsSameHash()
    {
        // Arrange
        var value1 = 1;
        var value2 = 2;
        var value3 = 3;
        var value4 = 4;
        var value5 = 5;
        var value6 = 6;
        var value7 = 7;

        // Act
        var hash1 = HashCode.Combine(value1, value2, value3, value4, value5, value6, value7);
        var hash2 = HashCode.Combine(value1, value2, value3, value4, value5, value6, value7);

        // Assert
        hash1.Should().Be(hash2);
    }

    #endregion

    #region Combine<T1, T2, T3, T4, T5, T6, T7, T8> Method Tests

    [Fact]
    public void Combine_WithEightIntValues_ReturnsValidHashCode()
    {
        // Arrange
        var value1 = 1;
        var value2 = 2;
        var value3 = 3;
        var value4 = 4;
        var value5 = 5;
        var value6 = 6;
        var value7 = 7;
        var value8 = 8;

        // Act
        var hash = HashCode.Combine(value1, value2, value3, value4, value5, value6, value7, value8);

        // Assert
        hash.Should().NotBe(0);
    }

    [Fact]
    public void Combine_WithSameEightIntValues_ReturnsSameHash()
    {
        // Arrange
        var value1 = 1;
        var value2 = 2;
        var value3 = 3;
        var value4 = 4;
        var value5 = 5;
        var value6 = 6;
        var value7 = 7;
        var value8 = 8;

        // Act
        var hash1 = HashCode.Combine(
            value1,
            value2,
            value3,
            value4,
            value5,
            value6,
            value7,
            value8
        );
        var hash2 = HashCode.Combine(
            value1,
            value2,
            value3,
            value4,
            value5,
            value6,
            value7,
            value8
        );

        // Assert
        hash1.Should().Be(hash2);
    }

    [Fact]
    public void Combine_WithDifferentEightIntValues_ReturnsDifferentHashes()
    {
        // Arrange
        var hash1 = HashCode.Combine(1, 2, 3, 4, 5, 6, 7, 8);
        var hash2 = HashCode.Combine(1, 2, 3, 4, 5, 6, 7, 9);

        // Act & Assert
        hash1.Should().NotBe(hash2);
    }

    #endregion

    #region Instance Method Add Tests

    [Fact]
    public void Add_WithSingleInt_CreatesValidHash()
    {
        // Arrange
        var hashCode = new HashCode();
        var value = 42;

        // Act
        hashCode.Add(value);
        var hash = hashCode.ToHashCode();

        // Assert
        hash.Should().NotBe(0);
    }

    [Fact]
    public void Add_WithSingleNull_CreatesValidHash()
    {
        // Arrange
        var hashCode = new HashCode();
        string? value = null;

        // Act
        hashCode.Add(value);
        var hash = hashCode.ToHashCode();

        // Assert
        hash.Should().NotBe(0);
    }

    [Fact]
    public void Add_WithMultipleValues_CreatesValidHash()
    {
        // Arrange
        var hashCode = new HashCode();

        // Act
        hashCode.Add(1);
        hashCode.Add(2);
        hashCode.Add(3);
        var hash = hashCode.ToHashCode();

        // Assert
        hash.Should().NotBe(0);
    }

    [Fact]
    public void Add_WithMultipleValuesConsistent_ReturnsSameHash()
    {
        // Arrange & Act
        var hashCode1 = new HashCode();
        hashCode1.Add(1);
        hashCode1.Add(2);
        hashCode1.Add(3);
        var hash1 = hashCode1.ToHashCode();

        var hashCode2 = new HashCode();
        hashCode2.Add(1);
        hashCode2.Add(2);
        hashCode2.Add(3);
        var hash2 = hashCode2.ToHashCode();

        // Assert
        hash1.Should().Be(hash2);
    }

    [Fact]
    public void Add_WithDifferentValues_ReturnsDifferentHashes()
    {
        // Arrange
        var hashCode1 = new HashCode();
        hashCode1.Add(1);
        hashCode1.Add(2);
        hashCode1.Add(3);
        var hash1 = hashCode1.ToHashCode();

        var hashCode2 = new HashCode();
        hashCode2.Add(1);
        hashCode2.Add(2);
        hashCode2.Add(4);
        var hash2 = hashCode2.ToHashCode();

        // Act & Assert
        hash1.Should().NotBe(hash2);
    }

    [Fact]
    public void Add_WithOrderedValues_DifferentOrderProducesDifferentHash()
    {
        // Arrange
        var hashCode1 = new HashCode();
        hashCode1.Add(1);
        hashCode1.Add(2);
        hashCode1.Add(3);
        var hash1 = hashCode1.ToHashCode();

        var hashCode2 = new HashCode();
        hashCode2.Add(3);
        hashCode2.Add(2);
        hashCode2.Add(1);
        var hash2 = hashCode2.ToHashCode();

        // Act & Assert
        hash1.Should().NotBe(hash2);
    }

    [Fact]
    public void Add_WithFourValues_CreatesValidHash()
    {
        // Arrange
        var hashCode = new HashCode();

        // Act
        hashCode.Add(1);
        hashCode.Add(2);
        hashCode.Add(3);
        hashCode.Add(4);
        var hash = hashCode.ToHashCode();

        // Assert
        hash.Should().NotBe(0);
    }

    [Fact]
    public void Add_WithEightValues_CreatesValidHash()
    {
        // Arrange
        var hashCode = new HashCode();

        // Act
        hashCode.Add(1);
        hashCode.Add(2);
        hashCode.Add(3);
        hashCode.Add(4);
        hashCode.Add(5);
        hashCode.Add(6);
        hashCode.Add(7);
        hashCode.Add(8);
        var hash = hashCode.ToHashCode();

        // Assert
        hash.Should().NotBe(0);
    }

    [Fact]
    public void Add_WithManyValues_CreatesValidHash()
    {
        // Arrange
        var hashCode = new HashCode();

        // Act
        for (var i = 0; i < 20; i++)
            hashCode.Add(i);

        var hash = hashCode.ToHashCode();

        // Assert
        hash.Should().NotBe(0);
    }

    [Fact]
    public void Add_WithStringValues_CreatesValidHash()
    {
        // Arrange
        var hashCode = new HashCode();

        // Act
        hashCode.Add("test1");
        hashCode.Add("test2");
        hashCode.Add("test3");
        var hash = hashCode.ToHashCode();

        // Assert
        hash.Should().NotBe(0);
    }

    [Fact]
    public void Add_WithMixedTypes_CreatesValidHash()
    {
        // Arrange
        var hashCode = new HashCode();

        // Act
        hashCode.Add(42);
        hashCode.Add("test");
        hashCode.Add(3.14);
        var hash = hashCode.ToHashCode();

        // Assert
        hash.Should().NotBe(0);
    }

    #endregion

    #region ToHashCode Method Tests

    [Fact]
    public void ToHashCode_OnEmptyInstance_ReturnsValidHashCode()
    {
        // Arrange
        var hashCode = new HashCode();

        // Act
        var hash = hashCode.ToHashCode();

        // Assert
        hash.Should().NotBe(0);
    }

    [Fact]
    public void ToHashCode_CallMultipleTimes_ReturnsSameValue()
    {
        // Arrange
        var hashCode = new HashCode();
        hashCode.Add(42);

        // Act
        var hash1 = hashCode.ToHashCode();
        var hash2 = hashCode.ToHashCode();
        var hash3 = hashCode.ToHashCode();

        // Assert
        hash1.Should().Be(hash2);
        hash2.Should().Be(hash3);
    }

    [Fact]
    public void ToHashCode_WithNoAddCalls_ReturnsValidHashCode()
    {
        // Arrange
        var hashCode = new HashCode();

        // Act
        var hash = hashCode.ToHashCode();

        // Assert
        hash.Should().BeOfType(typeof(int));
    }

    [Fact]
    public void ToHashCode_ProducesValidInt32_WithinRange()
    {
        // Arrange
        var hashCode = new HashCode();
        hashCode.Add(42);

        // Act
        var hash = hashCode.ToHashCode();

        // Assert
        hash.Should().BeGreaterThanOrEqualTo(int.MinValue);
        hash.Should().BeLessThanOrEqualTo(int.MaxValue);
    }

    #endregion

    #region Edge Cases & Boundary Tests

    [Fact]
    public void Add_WithEqualityComparerNull_UsesDefaultComparison()
    {
        // Arrange
        var hashCode = new HashCode();
        var value = "test";

        // Act
        hashCode.Add(value, null);
        var hash = hashCode.ToHashCode();

        // Assert
        hash.Should().NotBe(0);
    }

    [Fact]
    public void Add_WithEqualityComparerNullValue_UsesDefaultComparison()
    {
        // Arrange
        var hashCode = new HashCode();
        string? value = null;

        // Act
        hashCode.Add(value, null);
        var hash = hashCode.ToHashCode();

        // Assert
        hash.Should().NotBe(0);
    }

    [Fact]
    public void Combine_WithAllNullValues_ReturnsValidHashCode()
    {
        // Arrange & Act
#pragma warning disable CS8625
        var hash = HashCode.Combine<string, string, string>(null, null, null);
#pragma warning restore CS8625

        // Assert
        hash.Should().NotBe(0);
    }

    [Fact]
    public void Add_WithZeroValue_CreatesValidHash()
    {
        // Arrange
        var hashCode = new HashCode();

        // Act
        hashCode.Add(0);
        var hash = hashCode.ToHashCode();

        // Assert
        hash.Should().NotBe(0);
    }

    [Fact]
    public void Add_WithNegativeValue_CreatesValidHash()
    {
        // Arrange
        var hashCode = new HashCode();

        // Act
        hashCode.Add(-42);
        var hash = hashCode.ToHashCode();

        // Assert
        hash.Should().NotBe(0);
    }

    [Fact]
    public void Add_WithMaxIntValue_CreatesValidHash()
    {
        // Arrange
        var hashCode = new HashCode();

        // Act
        hashCode.Add(int.MaxValue);
        var hash = hashCode.ToHashCode();

        // Assert
        hash.Should().NotBe(0);
    }

    [Fact]
    public void Add_WithMinIntValue_CreatesValidHash()
    {
        // Arrange
        var hashCode = new HashCode();

        // Act
        hashCode.Add(int.MinValue);
        var hash = hashCode.ToHashCode();

        // Assert
        hash.Should().NotBe(0);
    }

    [Fact]
    public void Add_WithEmptyString_CreatesValidHash()
    {
        // Arrange
        var hashCode = new HashCode();

        // Act
        hashCode.Add(string.Empty);
        var hash = hashCode.ToHashCode();

        // Assert
        hash.Should().NotBe(0);
    }

    #endregion

    #region Type Variation Tests

    [Fact]
    public void Combine_WithLongValue_ReturnsValidHashCode()
    {
        // Arrange
        var value = 42L;

        // Act
        var hash = HashCode.Combine(value);

        // Assert
        hash.Should().NotBe(0);
    }

    [Fact]
    public void Combine_WithDoubleValue_ReturnsValidHashCode()
    {
        // Arrange
        var value = 3.14;

        // Act
        var hash = HashCode.Combine(value);

        // Assert
        hash.Should().NotBe(0);
    }

    [Fact]
    public void Combine_WithBoolValue_ReturnsValidHashCode()
    {
        // Arrange
        var value = true;

        // Act
        var hash = HashCode.Combine(value);

        // Assert
        hash.Should().NotBe(0);
    }

    [Fact]
    public void Combine_WithGuidValue_ReturnsValidHashCode()
    {
        // Arrange
        var value = Guid.NewGuid();

        // Act
        var hash = HashCode.Combine(value);

        // Assert
        hash.Should().NotBe(0);
    }

    [Fact]
    public void Add_WithLongValue_CreatesValidHash()
    {
        // Arrange
        var hashCode = new HashCode();
        var value = 42L;

        // Act
        hashCode.Add(value);
        var hash = hashCode.ToHashCode();

        // Assert
        hash.Should().NotBe(0);
    }

    [Fact]
    public void Add_WithDoubleValue_CreatesValidHash()
    {
        // Arrange
        var hashCode = new HashCode();
        var value = 3.14;

        // Act
        hashCode.Add(value);
        var hash = hashCode.ToHashCode();

        // Assert
        hash.Should().NotBe(0);
    }

    [Fact]
    public void Add_WithGuidValue_CreatesValidHash()
    {
        // Arrange
        var hashCode = new HashCode();
        var value = Guid.NewGuid();

        // Act
        hashCode.Add(value);
        var hash = hashCode.ToHashCode();

        // Assert
        hash.Should().NotBe(0);
    }

    #endregion

    #region Consistency & Determinism Tests

    [Fact]
    public void Combine_AcrossMultipleCalls_IsConsistent()
    {
        // Arrange
        var value1 = 42;
        var value2 = "test";
        var value3 = 3.14;

        // Act
        var hashes = new int[5];
        for (var i = 0; i < 5; i++)
            hashes[i] = HashCode.Combine(value1, value2, value3);

        // Assert
        hashes[0].Should().Be(hashes[1]);
        hashes[1].Should().Be(hashes[2]);
        hashes[2].Should().Be(hashes[3]);
        hashes[3].Should().Be(hashes[4]);
    }

    [Fact]
    public void Add_AcrossMultipleCalls_IsConsistent()
    {
        // Arrange
        var hashes = new int[5];

        // Act
        for (var i = 0; i < 5; i++)
        {
            var hashCode = new HashCode();
            hashCode.Add(1);
            hashCode.Add(2);
            hashCode.Add(3);
            hashes[i] = hashCode.ToHashCode();
        }

        // Assert
        hashes[0].Should().Be(hashes[1]);
        hashes[1].Should().Be(hashes[2]);
        hashes[2].Should().Be(hashes[3]);
        hashes[3].Should().Be(hashes[4]);
    }

    [Fact]
    public void Combine_WithZero_IsDeterministic()
    {
        // Arrange & Act
        var hash1 = HashCode.Combine(0);
        var hash2 = HashCode.Combine(0);
        var hash3 = HashCode.Combine(0);

        // Assert
        hash1.Should().Be(hash2);
        hash2.Should().Be(hash3);
    }

    [Fact]
    public void Add_WithSameSequenceInDifferentInstances_ProducesSameHash()
    {
        // Arrange
        var values = new[] { 1, 2, 3, 4, 5 };

        // Act
        var hashCode1 = new HashCode();
        foreach (var value in values)
            hashCode1.Add(value);

        var hash1 = hashCode1.ToHashCode();

        var hashCode2 = new HashCode();
        foreach (var value in values)
            hashCode2.Add(value);

        var hash2 = hashCode2.ToHashCode();

        // Assert
        hash1.Should().Be(hash2);
    }

    #endregion

    #region Combination Method Consistency Tests

    [Fact]
    public void Combine_InstanceMethodEquivalence_SingleValue()
    {
        // Arrange
        var value = 42;

        // Act
        var staticHash = HashCode.Combine(value);
        var instanceHash = new HashCode();
        instanceHash.Add(value);
        var result = instanceHash.ToHashCode();

        // Assert
        staticHash.Should().Be(result);
    }

    [Fact]
    public void Combine_InstanceMethodEquivalence_TwoValues()
    {
        // Arrange
        var value1 = 1;
        var value2 = 2;

        // Act
        var staticHash = HashCode.Combine(value1, value2);
        var instanceHash = new HashCode();
        instanceHash.Add(value1);
        instanceHash.Add(value2);
        var result = instanceHash.ToHashCode();

        // Assert
        staticHash.Should().Be(result);
    }

    [Fact]
    public void Combine_InstanceMethodEquivalence_ThreeValues()
    {
        // Arrange
        var value1 = 1;
        var value2 = 2;
        var value3 = 3;

        // Act
        var staticHash = HashCode.Combine(value1, value2, value3);
        var instanceHash = new HashCode();
        instanceHash.Add(value1);
        instanceHash.Add(value2);
        instanceHash.Add(value3);
        var result = instanceHash.ToHashCode();

        // Assert
        staticHash.Should().Be(result);
    }

    [Fact]
    public void Combine_InstanceMethodEquivalence_FourValues()
    {
        // Arrange
        var value1 = 1;
        var value2 = 2;
        var value3 = 3;
        var value4 = 4;

        // Act
        var staticHash = HashCode.Combine(value1, value2, value3, value4);
        var instanceHash = new HashCode();
        instanceHash.Add(value1);
        instanceHash.Add(value2);
        instanceHash.Add(value3);
        instanceHash.Add(value4);
        var result = instanceHash.ToHashCode();

        // Assert
        staticHash.Should().Be(result);
    }

    #endregion
}
