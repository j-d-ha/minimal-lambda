using AutoFixture;
using AwesomeAssertions;
using JetBrains.Annotations;
using MinimalLambda.Envelopes.Alb;
using MinimalLambda.Options;
using Xunit;

namespace MinimalLambda.Envelopes.UnitTests;

[TestSubject(typeof(HttpResultExtensions))]
public class HttpResultExtensionsTests
{
    private readonly Fixture _fixture = new();

    [Fact]
    public void Ok_ReturnsStatus200()
    {
        // Act
        var result = AlbResult.Ok();

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(200);
    }

    [Fact]
    public void Ok_WithBodyContent_ReturnsStatus200WithJson()
    {
        // Arrange
        var payload = _fixture.Create<TestPayload>();
        var options = new EnvelopeOptions();

        // Act
        var result = AlbResult.Ok(payload);
        result.PackPayload(options);

        // Assert
        result.StatusCode.Should().Be(200);
        result.Headers.Should().ContainKey("Content-Type");
        result.Headers["Content-Type"].Should().Be("application/json; charset=utf-8");
        result.Body.Should().NotBeNull();
        result.Body.Should().Contain(payload.Name);
    }

    [Fact]
    public void Created_ReturnsStatus201()
    {
        // Act
        var result = AlbResult.Created();

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(201);
    }

    [Fact]
    public void Created_WithBodyContent_ReturnsStatus201WithJson()
    {
        // Arrange
        var payload = _fixture.Create<TestPayload>();
        var options = new EnvelopeOptions();

        // Act
        var result = AlbResult.Created(payload);
        result.PackPayload(options);

        // Assert
        result.StatusCode.Should().Be(201);
        result.Headers.Should().ContainKey("Content-Type");
        result.Headers["Content-Type"].Should().Be("application/json; charset=utf-8");
        result.Body.Should().NotBeNull();
        result.Body.Should().Contain(payload.Name);
    }

    [Fact]
    public void Accepted_ReturnsStatus202()
    {
        // Act
        var result = AlbResult.Accepted();

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(202);
    }

    [Fact]
    public void Accepted_WithBodyContent_ReturnsStatus202WithJson()
    {
        // Arrange
        var payload = _fixture.Create<TestPayload>();
        var options = new EnvelopeOptions();

        // Act
        var result = AlbResult.Accepted(payload);
        result.PackPayload(options);

        // Assert
        result.StatusCode.Should().Be(202);
        result.Headers.Should().ContainKey("Content-Type");
        result.Headers["Content-Type"].Should().Be("application/json; charset=utf-8");
        result.Body.Should().NotBeNull();
        result.Body.Should().Contain(payload.Name);
    }

    [Fact]
    public void NoContent_ReturnsStatus204()
    {
        // Act
        var result = AlbResult.NoContent();

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(204);
    }

    [Fact]
    public void MovedPermanently_ReturnsStatus301()
    {
        // Act
        var result = AlbResult.MovedPermanently();

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(301);
    }

    [Fact]
    public void MovedPermanently_WithLocation_ReturnsStatus301WithLocationHeader()
    {
        // Arrange
        var location = "https://example.com/new-location";

        // Act
        var result = AlbResult.MovedPermanently(location);

        // Assert
        result.StatusCode.Should().Be(301);
        result.Headers.Should().ContainKey("Location");
        result.Headers["Location"].Should().Be(location);
    }

    [Fact]
    public void Found_ReturnsStatus302()
    {
        // Act
        var result = AlbResult.Found();

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(302);
    }

    [Fact]
    public void Found_WithLocation_ReturnsStatus302WithLocationHeader()
    {
        // Arrange
        var location = "https://example.com/temporary-location";

        // Act
        var result = AlbResult.Found(location);

        // Assert
        result.StatusCode.Should().Be(302);
        result.Headers.Should().ContainKey("Location");
        result.Headers["Location"].Should().Be(location);
    }

    [Fact]
    public void BadRequest_ReturnsStatus400()
    {
        // Act
        var result = AlbResult.BadRequest();

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(400);
    }

    [Fact]
    public void BadRequest_WithBodyContent_ReturnsStatus400WithJson()
    {
        // Arrange
        var payload = _fixture.Create<TestPayload>();
        var options = new EnvelopeOptions();

        // Act
        var result = AlbResult.BadRequest(payload);
        result.PackPayload(options);

        // Assert
        result.StatusCode.Should().Be(400);
        result.Headers.Should().ContainKey("Content-Type");
        result.Headers["Content-Type"].Should().Be("application/json; charset=utf-8");
        result.Body.Should().NotBeNull();
        result.Body.Should().Contain(payload.Name);
    }

    [Fact]
    public void Unauthorized_ReturnsStatus401()
    {
        // Act
        var result = AlbResult.Unauthorized();

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(401);
    }

    [Fact]
    public void Forbidden_ReturnsStatus403()
    {
        // Act
        var result = AlbResult.Forbidden();

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(403);
    }

    [Fact]
    public void Forbidden_WithBodyContent_ReturnsStatus403WithJson()
    {
        // Arrange
        var payload = _fixture.Create<TestPayload>();
        var options = new EnvelopeOptions();

        // Act
        var result = AlbResult.Forbidden(payload);
        result.PackPayload(options);

        // Assert
        result.StatusCode.Should().Be(403);
        result.Headers.Should().ContainKey("Content-Type");
        result.Headers["Content-Type"].Should().Be("application/json; charset=utf-8");
        result.Body.Should().NotBeNull();
        result.Body.Should().Contain(payload.Name);
    }

    [Fact]
    public void NotFound_ReturnsStatus404()
    {
        // Act
        var result = AlbResult.NotFound();

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public void NotFound_WithBodyContent_ReturnsStatus404WithJson()
    {
        // Arrange
        var payload = _fixture.Create<TestPayload>();
        var options = new EnvelopeOptions();

        // Act
        var result = AlbResult.NotFound(payload);
        result.PackPayload(options);

        // Assert
        result.StatusCode.Should().Be(404);
        result.Headers.Should().ContainKey("Content-Type");
        result.Headers["Content-Type"].Should().Be("application/json; charset=utf-8");
        result.Body.Should().NotBeNull();
        result.Body.Should().Contain(payload.Name);
    }

    [Fact]
    public void Conflict_ReturnsStatus409()
    {
        // Act
        var result = AlbResult.Conflict();

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(409);
    }

    [Fact]
    public void Conflict_WithBodyContent_ReturnsStatus409WithJson()
    {
        // Arrange
        var payload = _fixture.Create<TestPayload>();
        var options = new EnvelopeOptions();

        // Act
        var result = AlbResult.Conflict(payload);
        result.PackPayload(options);

        // Assert
        result.StatusCode.Should().Be(409);
        result.Headers.Should().ContainKey("Content-Type");
        result.Headers["Content-Type"].Should().Be("application/json; charset=utf-8");
        result.Body.Should().NotBeNull();
        result.Body.Should().Contain(payload.Name);
    }

    [Fact]
    public void UnprocessableEntity_ReturnsStatus422()
    {
        // Act
        var result = AlbResult.UnprocessableEntity();

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(422);
    }

    [Fact]
    public void UnprocessableEntity_WithBodyContent_ReturnsStatus422WithJson()
    {
        // Arrange
        var payload = _fixture.Create<TestPayload>();
        var options = new EnvelopeOptions();

        // Act
        var result = AlbResult.UnprocessableEntity(payload);
        result.PackPayload(options);

        // Assert
        result.StatusCode.Should().Be(422);
        result.Headers.Should().ContainKey("Content-Type");
        result.Headers["Content-Type"].Should().Be("application/json; charset=utf-8");
        result.Body.Should().NotBeNull();
        result.Body.Should().Contain(payload.Name);
    }

    [Fact]
    public void InternalServerError_ReturnsStatus500()
    {
        // Act
        var result = AlbResult.InternalServerError();

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(500);
    }

    [Fact]
    public void InternalServerError_WithBodyContent_ReturnsStatus500WithJson()
    {
        // Arrange
        var payload = _fixture.Create<TestPayload>();
        var options = new EnvelopeOptions();

        // Act
        var result = AlbResult.InternalServerError(payload);
        result.PackPayload(options);

        // Assert
        result.StatusCode.Should().Be(500);
        result.Headers.Should().ContainKey("Content-Type");
        result.Headers["Content-Type"].Should().Be("application/json; charset=utf-8");
        result.Body.Should().NotBeNull();
        result.Body.Should().Contain(payload.Name);
    }

    // ReSharper disable once NotAccessedPositionalProperty.Local
    private record TestPayload(string Name, int Value);
}
