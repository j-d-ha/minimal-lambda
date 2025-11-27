# Envelopes

Envelopes extend AWS Lambda event types with strongly-typed payload handling, making it easier to
work with JSON (or other formats) in Lambda function handlers.

## Overview

Envelope packages wrap official AWS Lambda event classes (like `SQSEvent`, `APIGatewayProxyRequest`)
and add a `BodyContent<T>` property that provides type-safe access to deserialized message payloads.
Instead of manually parsing JSON strings from event bodies, you get strongly-typed objects with full
IDE support and compile-time type checking.

**Key benefits:**

- **Type Safety**  Generic type parameter `<T>` ensures compile-time type checking for payloads
- **Extensibility**  Abstract base classes allow custom serialization formats (JSON, XML, etc.)
- **Zero Overhead**  Envelopes extend official AWS event types, adding no runtime cost
- **AOT Ready**  Support for Native AOT compilation via JsonSerializerContext registration
- **Familiar API**  Works seamlessly with existing AWS Lambda event patterns

## Packages

| Package                                                                                    | NuGet                                                                                                                                                  | Downloads                                                                                                                                                    |
|--------------------------------------------------------------------------------------------|--------------------------------------------------------------------------------------------------------------------------------------------------------|--------------------------------------------------------------------------------------------------------------------------------------------------------------|
| [**AwsLambda.Host.Envelopes.Sqs**](./AwsLambda.Host.Envelopes.Sqs/README.md)               | [![NuGet](https://img.shields.io/nuget/v/AwsLambda.Host.Envelopes.Sqs.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.Sqs)               | [![Downloads](https://img.shields.io/nuget/dt/AwsLambda.Host.Envelopes.Sqs.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.Sqs/)               |
| [**AwsLambda.Host.Envelopes.ApiGateway**](./AwsLambda.Host.Envelopes.ApiGateway/README.md) | [![NuGet](https://img.shields.io/nuget/v/AwsLambda.Host.Envelopes.ApiGateway.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.ApiGateway) | [![Downloads](https://img.shields.io/nuget/dt/AwsLambda.Host.Envelopes.ApiGateway.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.ApiGateway/) |
| [**AwsLambda.Host.Envelopes.Sns**](./AwsLambda.Host.Envelopes.Sns/README.md)               | [![NuGet](https://img.shields.io/nuget/v/AwsLambda.Host.Envelopes.Sns.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.Sns)               | [![Downloads](https://img.shields.io/nuget/dt/AwsLambda.Host.Envelopes.Sns.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.Sns/)               |
| [**AwsLambda.Host.Envelopes.Alb**](./AwsLambda.Host.Envelopes.Alb/README.md)               | [![NuGet](https://img.shields.io/nuget/v/AwsLambda.Host.Envelopes.Alb.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.Alb)               | [![Downloads](https://img.shields.io/nuget/dt/AwsLambda.Host.Envelopes.Alb.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.Alb/)               |

Each package has detailed documentation in its own README file.

## Quick Reference

Use the following table to find the right envelope package for your Lambda event type:

| Lambda Event Type         | Package                             |
|---------------------------|-------------------------------------|
| API Gateway REST API      | AwsLambda.Host.Envelopes.ApiGateway |
| API Gateway HTTP API v1   | AwsLambda.Host.Envelopes.ApiGateway |
| API Gateway HTTP API v2   | AwsLambda.Host.Envelopes.ApiGateway |
| API Gateway WebSocket     | AwsLambda.Host.Envelopes.ApiGateway |
| Application Load Balancer | AwsLambda.Host.Envelopes.Alb        |
| SNS                       | AwsLambda.Host.Envelopes.Sns        |
| SQS                       | AwsLambda.Host.Envelopes.Sqs        |
