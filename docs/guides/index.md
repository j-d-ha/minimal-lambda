# Guides

Comprehensive guides for building production Lambda functions with aws-lambda-host. Each guide provides in-depth coverage of a specific framework feature with complete examples, best practices, and troubleshooting.

## Core Framework Guides

Master the essential framework features that power your Lambda functions.

### [Dependency Injection](dependency-injection.md)
Learn service registration patterns, understand Singleton vs Scoped lifetimes, and master dependency injection in handlers and lifecycle methods.

**Topics covered:**

- Service lifetime management (Singleton, Scoped)
- Service registration patterns
- Injectable parameter types
- Options pattern and configuration
- Best practices and anti-patterns

### [Middleware](middleware.md)
Build middleware pipelines for cross-cutting concerns like logging, metrics, validation, and error handling.

**Topics covered:**
- Middleware pipeline composition
- Common middleware patterns
- Context and state management
- Execution order and control flow
- Reusable middleware components

### [Lifecycle Management](lifecycle-management.md)
Understand and control the Lambda lifecycle phases: OnInit, Invocation, and OnShutdown.

**Topics covered:**

- OnInit phase for cold start setup
- OnShutdown phase for cleanup
- Multiple lifecycle handlers
- Timeout configuration
- Error handling in lifecycle

### [Handler Registration](handler-registration.md)
Register type-safe Lambda handlers with automatic dependency injection and source generation.

**Topics covered:**

- MapHandler method usage
- `[Event]` attribute requirements
- Injectable parameter types
- Return type handling
- Source generation benefits
- Handler patterns

### [Configuration](configuration.md)
Configure framework behavior with LambdaHostOptions and application settings.

**Topics covered:**

- LambdaHostOptions reference
- Timeout and cancellation configuration
- Application configuration patterns
- JSON serialization options
- Environment variables
- Best practices

## Development Guides

Build robust, testable, and deployable Lambda functions.

### [Error Handling](error-handling.md)
Implement resilient error handling with retries, graceful degradation, and proper exception management.

**Topics covered:**

- Exception handling in handlers
- Middleware error handling
- Cancellation token usage
- Retry patterns and strategies
- Dead Letter Queue integration
- Best practices

### [Testing](testing.md)
Write comprehensive tests for your Lambda functions using xUnit, NSubstitute, and AutoFixture.

**Topics covered:**

- Testing framework setup
- Unit testing services
- AutoNSubstituteData pattern
- Testing handlers and middleware
- Integration testing
- Test naming conventions

## Learning Path

### New to aws-lambda-host?

Start with [Getting Started](/getting-started/) to build your first Lambda function, then return here for deeper coverage of specific features.

### Building Production Lambda Functions?

Use these guides as reference documentation when implementing specific features. Each guide is self-contained and can be read independently.

### Optimizing Performance?

After mastering the guides, explore [Advanced Topics](/advanced/) for AOT compilation, source generators, and performance optimization.

## Additional Resources

- **[Examples](/examples/)** – Repository sample projects (more coming soon)
- **[Features](/features/)** – Envelope packages and OpenTelemetry add-ons
- **[Advanced Topics](/advanced/)** – Placeholder for Native AOT, generators, and performance deep dives

## Getting Help

If you encounter issues not covered in these guides:

- Search or ask in [GitHub Discussions](https://github.com/j-d-ha/aws-lambda-host/discussions)
- Report bugs in [GitHub Issues](https://github.com/j-d-ha/aws-lambda-host/issues)

---

Ready to dive in? Choose a guide above or start with [Dependency Injection](dependency-injection.md).
