# Getting Started

**aws-lambda-host** brings ASP.NET Core–style hosting, dependency injection, and middleware to AWS Lambda. Instead of wiring up serialization and context handling manually, you configure a Lambda-specific host that manages scopes, middleware, and strongly typed handlers at compile time.

### Why aws-lambda-host?

- **Familiar patterns** – Builder APIs, DI, and middleware mirror ASP.NET Core.
- **Source-generated handlers** – Avoid reflection while staying AOT ready.
- **Lambda-focused lifecycle** – Startup, invocation, and shutdown hooks map to the Lambda runtime model.
- **Type-safe envelopes** – Request/response contracts are validated at compile time.

## Prerequisites

Before you begin, ensure you have:

- **.NET 8 SDK or later** – [Download here](https://dotnet.microsoft.com/download)
- **C# 11 or later** – Required for source generators and language features
- **Basic AWS Lambda knowledge** – Understanding of Lambda concepts (functions, events, execution model)
- **AWS Account** – For deploying and testing (optional for local development)

!!! tip "IDE Recommendations"
    - Visual Studio 2022 (17.8+)
    - JetBrains Rider 2023.3+
    - Visual Studio Code with C# Dev Kit

## Start Here

- **[Installation](installation.md)** – Add the NuGet packages and configure your csproj.
- **[Your First Lambda](first-lambda.md)** – Walk through a handler, DI setup, and local testing.
- **[Core Concepts](core-concepts.md)** – Learn about the host lifecycle, middleware, and source generation.

Prefer to explore? Head directly to **[Guides](/guides/)**, **[Examples](/examples/)**, or the **[API Reference](/api-reference/)** for deeper dives.

## Framework Highlights

- **Async-first runtime** – Cancellation tokens, timeout awareness, and scoped services work as expected.
- **Middleware pipeline** – Implement interception logic such as logging, validation, or OpenTelemetry spans in one place.
- **Envelope integrations** – Map SQS, SNS, API Gateway, and custom payloads using the envelope abstractions.
- **Observability-ready** – First-class OpenTelemetry integration for traces and metrics.

## Explore Features

- **[Envelopes](/features/envelopes/)** – Type-safe event source integration (SQS, SNS, API Gateway, etc.)
- **[OpenTelemetry](/features/open_telemetry.md)** – Distributed tracing and observability
- **[AOT Compilation](/advanced/aot-compilation.md)** – Optimize for fastest cold starts
- **[Source Generators](/advanced/source-generators.md)** – Understand compile-time optimizations

## Need Help

## Getting Help

If you run into issues or have questions:

- **[FAQ](/resources/faq.md)** – Common questions and answers
- **[Troubleshooting](/resources/troubleshooting.md)** – Solutions to common problems
- **[GitHub Issues](https://github.com/j-d-ha/aws-lambda-host/issues)** – Report bugs or request features
- **[GitHub Discussions](https://github.com/j-d-ha/aws-lambda-host/discussions)** – Ask questions and share ideas

Continue with **[Installation](installation.md)** to configure your project.
