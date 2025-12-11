using System.Collections;
using AwesomeAssertions;
using JetBrains.Annotations;

namespace MinimalLambda.SourceGenerators.UnitTests.Types;

[TestSubject(typeof(EquatableArray<>))]
public class EquatableArrayTests
{
    #region Construction & Initialization Tests

    [Fact]
    public void Constructor_WithValidArray_CreatesInstance()
    {
        // Arrange
        var values = new[] { 1, 2, 3 };

        // Act
        var array = new EquatableArray<int>(values);

        // Assert
        array.Count.Should().Be(3);
    }

    [Fact]
    public void Constructor_WithNullArray_CreatesInstance()
    {
        // Arrange & Act
        var array = new EquatableArray<int>(null!);

        // Assert
        array.Count.Should().Be(0);
    }

    [Fact]
    public void Constructor_WithSingleElement_CreatesInstance()
    {
        // Arrange
        var values = new[] { 42 };

        // Act
        var array = new EquatableArray<int>(values);

        // Assert
        array.Count.Should().Be(1);
    }

    [Fact]
    public void Empty_ReturnsEmptyInstance()
    {
        // Arrange & Act
        var empty = EquatableArray<int>.Empty;

        // Assert
        empty.Count.Should().Be(0);
    }

    [Fact]
    public void Constructor_WithEmptyArray_CreatesInstance()
    {
        // Arrange
        var values = Array.Empty<int>();

        // Act
        var array = new EquatableArray<int>(values);

        // Assert
        array.Count.Should().Be(0);
    }

    #endregion

    #region Equality Tests

    [Fact]
    public void Equals_WithSameValues_ReturnsTrue()
    {
        // Arrange
        var array1 = new EquatableArray<int>(new[] { 1, 2, 3 });
        var array2 = new EquatableArray<int>(new[] { 1, 2, 3 });

        // Act
        var result = array1.Equals(array2);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentValues_ReturnsFalse()
    {
        // Arrange
        var array1 = new EquatableArray<int>(new[] { 1, 2, 3 });
        var array2 = new EquatableArray<int>(new[] { 1, 2, 4 });

        // Act
        var result = array1.Equals(array2);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Equals_WithDifferentLengths_ReturnsFalse()
    {
        // Arrange
        var array1 = new EquatableArray<int>(new[] { 1, 2 });
        var array2 = new EquatableArray<int>(new[] { 1, 2, 3 });

        // Act
        var result = array1.Equals(array2);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Equals_BothEmpty_ReturnsTrue()
    {
        // Arrange
        var array1 = new EquatableArray<int>(null!);
        var array2 = new EquatableArray<int>(Array.Empty<int>());

        // Act
        var result = array1.Equals(array2);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void EqualsObject_WithSameValues_ReturnsTrue()
    {
        // Arrange
        var array1 = new EquatableArray<int>(new[] { 1, 2, 3 });
        object array2 = new EquatableArray<int>(new[] { 1, 2, 3 });

        // Act
        var result = array1.Equals(array2);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void EqualsObject_WithDifferentType_ReturnsFalse()
    {
        // Arrange
        var array = new EquatableArray<int>(new[] { 1, 2, 3 });
        object other = "not an array";

        // Act
        var result = array.Equals(other);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void EqualsObject_WithNull_ReturnsFalse()
    {
        // Arrange
        var array = new EquatableArray<int>(new[] { 1, 2, 3 });

        // Act
        var result = array.Equals(null);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void EqualityOperator_WithSameValues_ReturnsTrue()
    {
        // Arrange
        var array1 = new EquatableArray<int>(new[] { 1, 2, 3 });
        var array2 = new EquatableArray<int>(new[] { 1, 2, 3 });

        // Act
        var result = array1 == array2;

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void EqualityOperator_WithDifferentValues_ReturnsFalse()
    {
        // Arrange
        var array1 = new EquatableArray<int>(new[] { 1, 2, 3 });
        var array2 = new EquatableArray<int>(new[] { 1, 2, 4 });

        // Act
        var result = array1 == array2;

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void InequalityOperator_WithSameValues_ReturnsFalse()
    {
        // Arrange
        var array1 = new EquatableArray<int>(new[] { 1, 2, 3 });
        var array2 = new EquatableArray<int>(new[] { 1, 2, 3 });

        // Act
        var result = array1 != array2;

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void InequalityOperator_WithDifferentValues_ReturnsTrue()
    {
        // Arrange
        var array1 = new EquatableArray<int>(new[] { 1, 2, 3 });
        var array2 = new EquatableArray<int>(new[] { 1, 2, 4 });

        // Act
        var result = array1 != array2;

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Equals_WithStringArrays_WorksCorrectly()
    {
        // Arrange
        var array1 = new EquatableArray<string>(new[] { "a", "b", "c" });
        var array2 = new EquatableArray<string>(new[] { "a", "b", "c" });
        var array3 = new EquatableArray<string>(new[] { "a", "b", "d" });

        // Act & Assert
        array1.Equals(array2).Should().BeTrue();
        array1.Equals(array3).Should().BeFalse();
    }

    #endregion

    #region GetHashCode Tests

    [Fact]
    public void GetHashCode_WithSameValues_ReturnsSameHashCode()
    {
        // Arrange
        var array1 = new EquatableArray<int>(new[] { 1, 2, 3 });
        var array2 = new EquatableArray<int>(new[] { 1, 2, 3 });

        // Act
        var hash1 = array1.GetHashCode();
        var hash2 = array2.GetHashCode();

        // Assert
        hash1.Should().Be(hash2);
    }

    [Fact]
    public void GetHashCode_WithDifferentValues_ReturnsDifferentHashCode()
    {
        // Arrange
        var array1 = new EquatableArray<int>(new[] { 1, 2, 3 });
        var array2 = new EquatableArray<int>(new[] { 1, 2, 4 });

        // Act
        var hash1 = array1.GetHashCode();
        var hash2 = array2.GetHashCode();

        // Assert
        hash1.Should().NotBe(hash2);
    }

    [Fact]
    public void GetHashCode_WithNullArray_ReturnsZero()
    {
        // Arrange
        var array = new EquatableArray<int>(null!);

        // Act
        var hash = array.GetHashCode();

        // Assert
        hash.Should().Be(0);
    }

    [Fact]
    public void GetHashCode_IsConsistent_CalledMultipleTimes()
    {
        // Arrange
        var array = new EquatableArray<int>(new[] { 1, 2, 3 });

        // Act
        var hash1 = array.GetHashCode();
        var hash2 = array.GetHashCode();
        var hash3 = array.GetHashCode();

        // Assert
        hash1.Should().Be(hash2);
        hash2.Should().Be(hash3);
    }

    #endregion

    #region Enumeration Tests

    [Fact]
    public void GetEnumerator_WithValues_EnumeratesAllElements()
    {
        // Arrange
        var values = new[] { 1, 2, 3, 4, 5 };
        var array = new EquatableArray<int>(values);

        // Act
        var enumerated = array.ToList();

        // Assert
        enumerated.Should().Equal(1, 2, 3, 4, 5);
    }

    [Fact]
    public void GetEnumerator_WithEmptyArray_EnumeratesNoElements()
    {
        // Arrange
        var array = new EquatableArray<int>(Array.Empty<int>());

        // Act
        var enumerated = new List<int>();
        foreach (var item in array)
            enumerated.Add(item);

        // Assert
        enumerated.Should().BeEmpty();
    }

    [Fact]
    public void GetEnumerator_WithNullArray_EnumeratesNoElements()
    {
        // Arrange
        var array = new EquatableArray<int>(null!);

        // Act
        var enumerated = new List<int>();
        foreach (var item in array)
            enumerated.Add(item);

        // Assert
        enumerated.Should().BeEmpty();
    }

    [Fact]
    public void GetEnumerator_WithStringArray_EnumeratesCorrectly()
    {
        // Arrange
        var values = new[] { "a", "b", "c" };
        var array = new EquatableArray<string>(values);

        // Act
        var enumerated = array.ToList();

        // Assert
        enumerated.Should().Equal(values);
    }

    [Fact]
    public void IEnumerable_SupportsLinqOperations()
    {
        // Arrange
        var array = new EquatableArray<int>(new[] { 1, 2, 3, 4, 5 });

        // Act
        var evenNumbers = array.Where(x => x % 2 == 0).ToList();

        // Assert
        evenNumbers.Should().Equal(2, 4);
    }

    #endregion

    #region Span & Array Access Tests

    [Fact]
    public void AsSpan_ReturnsReadOnlySpan()
    {
        // Arrange
        var values = new[] { 1, 2, 3 };
        var array = new EquatableArray<int>(values);

        // Act
        var span = array.AsSpan();

        // Assert
        span.Length.Should().Be(3);
        span[0].Should().Be(1);
        span[1].Should().Be(2);
        span[2].Should().Be(3);
    }

    [Fact]
    public void AsSpan_WithEmptyArray_ReturnsEmptySpan()
    {
        // Arrange
        var array = new EquatableArray<int>(Array.Empty<int>());

        // Act
        var span = array.AsSpan();

        // Assert
        span.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void GetArray_ReturnsUnderlyingArray()
    {
        // Arrange
        var values = new[] { 1, 2, 3 };
        var array = new EquatableArray<int>(values);

        // Act
        var result = array.GetArray();

        // Assert
        result.Should().BeSameAs(values);
    }

    [Fact]
    public void GetArray_WithNullArray_ReturnsNull()
    {
        // Arrange
        var array = new EquatableArray<int>(null!);

        // Act
        var result = array.GetArray();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void IntIndexer_AccessesElementAtIndex()
    {
        // Arrange
        var array = new EquatableArray<int>(new[] { 10, 20, 30 });

        // Act & Assert
        array[0].Should().Be(10);
        array[1].Should().Be(20);
        array[2].Should().Be(30);
    }

    [Fact]
    public void IntIndexer_WithNegativeIndex_ThrowsIndexOutOfRangeException()
    {
        // Arrange
        var array = new EquatableArray<int>(new[] { 10, 20, 30 });

        // Act & Assert
        var act = () => _ = array[-1];
        act.Should().ThrowExactly<IndexOutOfRangeException>();
    }

    [Fact]
    public void IntIndexer_WithIndexOutOfBounds_ThrowsIndexOutOfRangeException()
    {
        // Arrange
        var array = new EquatableArray<int>(new[] { 10, 20, 30 });

        // Act & Assert
        var act = () => _ = array[3];
        act.Should().ThrowExactly<IndexOutOfRangeException>();
    }

    [Fact]
    public void IntIndexer_AccessesFirstAndLastElements()
    {
        // Arrange
        var array = new EquatableArray<int>(new[] { 10, 20, 30 });

        // Act & Assert
        array[0].Should().Be(10);
        array[1].Should().Be(20);
        array[2].Should().Be(30);
    }

    [Fact]
    public void IntIndexer_WithNullArray_AccessingIndex_ThrowsIndexOutOfRangeException()
    {
        // Arrange
        // Tests: public T this[int index] => (_array ?? [])[index];
        // When _array is null, it coalesces to empty array, which throws on any index access
        var array = new EquatableArray<int>(null!);

        // Act & Assert
        var act = () => _ = array[0];
        act.Should().ThrowExactly<IndexOutOfRangeException>();
    }

    [Fact]
    public void IntIndexer_WithValidIndex_ReturnsCorrectElement()
    {
        // Arrange
        // Tests: public T this[int index] => (_array ?? [])[index];
        var array = new EquatableArray<string>(new[] { "alpha", "beta", "gamma", "delta" });

        // Act & Assert
        array[0].Should().Be("alpha");
        array[1].Should().Be("beta");
        array[2].Should().Be("gamma");
        array[3].Should().Be("delta");
    }

    [Fact]
    public void AsSpan_CanBeSliced()
    {
        // Arrange
        var array = new EquatableArray<int>(new[] { 1, 2, 3, 4, 5 });
        var span = array.AsSpan();

        // Act
        var sliced = span.Slice(1, 3).ToArray();

        // Assert
        sliced.Should().Equal(2, 3, 4);
    }

    [Fact]
    public void AsSpan_ReturnsFullSpan()
    {
        // Arrange
        var array = new EquatableArray<int>(new[] { 1, 2, 3, 4, 5 });

        // Act
        var span = array.AsSpan();

        // Assert
        span.Length.Should().Be(5);
        span.SequenceEqual(new[] { 1, 2, 3, 4, 5 }).Should().BeTrue();
    }

    [Fact]
    public void AsSpan_CanAccessByIndex()
    {
        // Arrange
        var array = new EquatableArray<int>(new[] { 10, 20, 30, 40, 50 });
        var span = array.AsSpan();

        // Act & Assert
        span[0].Should().Be(10);
        span[2].Should().Be(30);
        span[4].Should().Be(50);
    }

    [Fact]
    public void RangeIndexer_WithValidRange_ReturnsSlice()
    {
        // Arrange
        var array = new EquatableArray<int>(new[] { 1, 2, 3, 4, 5 });

        // Act
        var slice = array.AsSpan().Slice(1, 3);

        // Assert
        slice.Length.Should().Be(3);
        slice.SequenceEqual(new[] { 2, 3, 4 }).Should().BeTrue();
    }

    [Fact]
    public void RangeIndexer_WithStartOnly_ReturnsFromStartToEnd()
    {
        // Arrange
        var array = new EquatableArray<int>(new[] { 10, 20, 30, 40, 50 });

        // Act
        var slice = array.AsSpan().Slice(2);

        // Assert
        slice.Length.Should().Be(3);
        slice.SequenceEqual(new[] { 30, 40, 50 }).Should().BeTrue();
    }

    [Fact]
    public void RangeIndexer_WithEndOnly_ReturnsFromStartToEnd()
    {
        // Arrange
        var array = new EquatableArray<int>(new[] { 10, 20, 30, 40, 50 });

        // Act
        var slice = array.AsSpan().Slice(0, 3);

        // Assert
        slice.Length.Should().Be(3);
        slice.SequenceEqual(new[] { 10, 20, 30 }).Should().BeTrue();
    }

    [Fact]
    public void RangeIndexer_WithFullRange_ReturnsFullSpan()
    {
        // Arrange
        var array = new EquatableArray<int>(new[] { 1, 2, 3 });

        // Act
        var slice = array.AsSpan();

        // Assert
        slice.Length.Should().Be(3);
        slice.SequenceEqual(new[] { 1, 2, 3 }).Should().BeTrue();
    }

    [Fact]
    public void RangeIndexer_WithEmptyArray_ReturnsEmptySpan()
    {
        // Arrange
        var array = new EquatableArray<int>(Array.Empty<int>());

        // Act
        var slice = array.AsSpan();

        // Assert
        slice.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void RangeIndexer_WithNullArray_ReturnsEmptySpan()
    {
        // Arrange
        var array = new EquatableArray<int>(null!);

        // Act
        var slice = array.AsSpan();

        // Assert
        slice.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void RangeIndexer_WithFromEndIndex_ReturnsLastElements()
    {
        // Arrange
        var array = new EquatableArray<int>(new[] { 1, 2, 3, 4, 5 });

        // Act
        var slice = array.AsSpan().Slice(3, 2);

        // Assert
        slice.Length.Should().Be(2);
        slice.SequenceEqual(new[] { 4, 5 }).Should().BeTrue();
    }

    [Fact]
    public void RangeIndexer_WithComplexRange_ReturnsCorrectSlice()
    {
        // Arrange
        var array = new EquatableArray<string>(new[] { "a", "b", "c", "d", "e" });

        // Act
        var slice = array.AsSpan().Slice(1, 3);

        // Assert
        slice.Length.Should().Be(3);
        slice.SequenceEqual(new[] { "b", "c", "d" }).Should().BeTrue();
    }

    [Fact]
    public void RangeIndexer_DirectCall_WithRange_ReturnsCorrectSlice()
    {
        // Arrange
        var array = new EquatableArray<int>(new[] { 10, 20, 30, 40, 50 });

        // Act
        // Direct call to the Range indexer: public ReadOnlySpan<T> this[Range range] =>
        // AsSpan()[range];
        var span = array.AsSpan();
        var slice = span.Slice(1, 3);

        // Assert
        slice.Length.Should().Be(3);
        slice.SequenceEqual(new[] { 20, 30, 40 }).Should().BeTrue();
    }

    #endregion

    #region IEnumerable (Non-Generic) Tests

    [Fact]
    public void IEnumerable_NonGenericGetEnumerator_WithValues_EnumeratesAllElements()
    {
        // Arrange
        var values = new[] { 1, 2, 3, 4, 5 };
        var array = new EquatableArray<int>(values);

        // Act
        var enumerated = new List<int>();
        IEnumerable nonGenericEnumerable = array;
        foreach (var item in nonGenericEnumerable)
            enumerated.Add((int)item);

        // Assert
        enumerated.Should().Equal(1, 2, 3, 4, 5);
    }

    [Fact]
    public void IEnumerable_NonGenericGetEnumerator_WithEmptyArray_EnumeratesNoElements()
    {
        // Arrange
        var array = new EquatableArray<int>(Array.Empty<int>());

        // Act
        var enumerated = new List<int>();
        IEnumerable nonGenericEnumerable = array;
        foreach (var item in nonGenericEnumerable)
            enumerated.Add((int)item);

        // Assert
        enumerated.Should().BeEmpty();
    }

    [Fact]
    public void IEnumerable_NonGenericGetEnumerator_WithNullArray_EnumeratesNoElements()
    {
        // Arrange
        var array = new EquatableArray<int>(null!);

        // Act
        var enumerated = new List<int>();
        IEnumerable nonGenericEnumerable = array;
        foreach (var item in nonGenericEnumerable)
            enumerated.Add((int)item);

        // Assert
        enumerated.Should().BeEmpty();
    }

    [Fact]
    public void IEnumerable_NonGenericGetEnumerator_WithStringArray_EnumeratesCorrectly()
    {
        // Arrange
        var values = new[] { "x", "y", "z" };
        var array = new EquatableArray<string>(values);

        // Act
        var enumerated = new List<string>();
        IEnumerable nonGenericEnumerable = array;
        foreach (var item in nonGenericEnumerable)
            enumerated.Add((string)item);

        // Assert
        enumerated.Should().Equal(values);
    }

    [Fact]
    public void IEnumerable_NonGenericGetEnumerator_ReturnsCorrectType()
    {
        // Arrange
        var array = new EquatableArray<int>(new[] { 1, 2, 3 });

        // Act
        IEnumerable nonGenericEnumerable = array;
        var enumerator = nonGenericEnumerable.GetEnumerator();

        // Assert
        enumerator.Should().NotBeNull();
    }

    [Fact]
    public void IEnumerable_NonGenericGetEnumerator_CanBeCalledMultipleTimes()
    {
        // Arrange
        var array = new EquatableArray<int>(new[] { 1, 2, 3 });

        // Act
        IEnumerable nonGenericEnumerable = array;
        var first = new List<int>();
        var second = new List<int>();

        foreach (var item in nonGenericEnumerable)
            first.Add((int)item);

        foreach (var item in nonGenericEnumerable)
            second.Add((int)item);

        // Assert
        first.Should().Equal(second);
        first.Should().Equal(1, 2, 3);
    }

    #endregion

    #region Count Property Tests

    [Fact]
    public void Count_WithPopulatedArray_ReturnsCorrectLength()
    {
        // Arrange
        var array = new EquatableArray<int>(new[] { 1, 2, 3, 4, 5 });

        // Act
        var count = array.Count;

        // Assert
        count.Should().Be(5);
    }

    [Fact]
    public void Count_WithEmptyArray_ReturnsZero()
    {
        // Arrange
        var array = new EquatableArray<int>(Array.Empty<int>());

        // Act
        var count = array.Count;

        // Assert
        count.Should().Be(0);
    }

    [Fact]
    public void Count_WithNullArray_ReturnsZero()
    {
        // Arrange
        var array = new EquatableArray<int>(null!);

        // Act
        var count = array.Count;

        // Assert
        count.Should().Be(0);
    }

    [Fact]
    public void Count_WithSingleElement_ReturnsOne()
    {
        // Arrange
        var array = new EquatableArray<int>(new[] { 42 });

        // Act
        var count = array.Count;

        // Assert
        count.Should().Be(1);
    }

    #endregion

    #region Edge Cases & Special Scenarios

    [Fact]
    public void Constructor_WithLargeArray_HandlesCorrectly()
    {
        // Arrange
        var values = Enumerable.Range(0, 10000).ToArray();

        // Act
        var array = new EquatableArray<int>(values);

        // Assert
        array.Count.Should().Be(10000);
        array[0].Should().Be(0);
        array[9999].Should().Be(9999);
    }

    [Fact]
    public void Equals_WithLargeArrays_ComparesCorrectly()
    {
        // Arrange
        var values = Enumerable.Range(0, 1000).ToArray();
        var array1 = new EquatableArray<int>(values);
        var array2 = new EquatableArray<int>(values.ToArray()); // Same values, different reference

        // Act
        var result = array1.Equals(array2);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Empty_CanBeUsedInDictionary()
    {
        // Arrange
        var dict = new Dictionary<EquatableArray<int>, string>();
        var empty = EquatableArray<int>.Empty;

        // Act
        dict[empty] = "empty";
        var retrieved = dict.TryGetValue(EquatableArray<int>.Empty, out var value);

        // Assert
        retrieved.Should().BeTrue();
        value.Should().Be("empty");
    }

    [Fact]
    public void Equals_WithSelfReference_ReturnsTrue()
    {
        // Arrange
        var array = new EquatableArray<int>(new[] { 1, 2, 3 });

        // Act
        var result = array.Equals(array);

        // Assert
        result.Should().BeTrue();
    }

    #endregion
}
