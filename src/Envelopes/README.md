# Envelopes

Envelopes extend AWS Lambda event types with strongly-typed payload handling, making it easier to
work with JSON (or other formats) in Lambda function handlers.

## Overview

Envelope packages wrap official AWS Lambda event classes (like `SQSEvent`, `APIGatewayProxyRequest`)
and add a `BodyContent<T>` property that provides type-safe access to deserialized message payloads.
Instead of manually parsing JSON strings from event bodies, you get strongly-typed objects with full
IDE support and compile-time type checking.

**Key benefits:**

- **Type Safety** Generic type parameter `<T>` ensures compile-time type checking for payloads
- **Extensibility** Abstract base classes allow custom serialization formats (JSON, XML, etc.)
- **Zero Overhead** Envelopes extend official AWS event types, adding no runtime cost
- **AOT Ready** Support for Native AOT compilation via JsonSerializerContext registration
- **Familiar API** Works seamlessly with existing AWS Lambda event patterns

## Packages

| Lambda Event Type                    | Package                                                                                                                       | NuGet                                                                                                                                                            | Downloads                                                                                                                                                              |
|--------------------------------------|-------------------------------------------------------------------------------------------------------------------------------|------------------------------------------------------------------------------------------------------------------------------------------------------------------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| SQS                                  | [**AwsLambda.Host.Envelopes.Sqs**](./AwsLambda.Host.Envelopes.Sqs/README.md)                                                  | [![NuGet](https://img.shields.io/nuget/v/AwsLambda.Host.Envelopes.Sqs.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.Sqs)                         | [![Downloads](https://img.shields.io/nuget/dt/AwsLambda.Host.Envelopes.Sqs.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.Sqs/)                         |
| SQS (SNS-to-SQS Subscription)        | [**AwsLambda.Host.Envelopes.Sqs**](./AwsLambda.Host.Envelopes.Sqs/README.md#sqssnsenvelope---sns-to-sqs-subscription-pattern) | [![NuGet](https://img.shields.io/nuget/v/AwsLambda.Host.Envelopes.Sqs.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.Sqs)                         | [![Downloads](https://img.shields.io/nuget/dt/AwsLambda.Host.Envelopes.Sqs.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.Sqs/)                         |
| API Gateway (REST/HTTP/WebSocket)    | [**AwsLambda.Host.Envelopes.ApiGateway**](./AwsLambda.Host.Envelopes.ApiGateway/README.md)                                    | [![NuGet](https://img.shields.io/nuget/v/AwsLambda.Host.Envelopes.ApiGateway.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.ApiGateway)           | [![Downloads](https://img.shields.io/nuget/dt/AwsLambda.Host.Envelopes.ApiGateway.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.ApiGateway/)           |
| SNS                                  | [**AwsLambda.Host.Envelopes.Sns**](./AwsLambda.Host.Envelopes.Sns/README.md)                                                  | [![NuGet](https://img.shields.io/nuget/v/AwsLambda.Host.Envelopes.Sns.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.Sns)                         | [![Downloads](https://img.shields.io/nuget/dt/AwsLambda.Host.Envelopes.Sns.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.Sns/)                         |
| Kinesis Data Streams                 | [**AwsLambda.Host.Envelopes.Kinesis**](./AwsLambda.Host.Envelopes.Kinesis/README.md)                                          | [![NuGet](https://img.shields.io/nuget/v/AwsLambda.Host.Envelopes.Kinesis.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.Kinesis)                 | [![Downloads](https://img.shields.io/nuget/dt/AwsLambda.Host.Envelopes.Kinesis.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.Kinesis/)                 |
| Kinesis Data Firehose Transformation | [**AwsLambda.Host.Envelopes.KinesisFirehose**](./AwsLambda.Host.Envelopes.KinesisFirehose/README.md)                          | [![NuGet](https://img.shields.io/nuget/v/AwsLambda.Host.Envelopes.KinesisFirehose.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.KinesisFirehose) | [![Downloads](https://img.shields.io/nuget/dt/AwsLambda.Host.Envelopes.KinesisFirehose.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.KinesisFirehose/) |
| Kafka (MSK or self-managed)          | [**AwsLambda.Host.Envelopes.Kafka**](./AwsLambda.Host.Envelopes.Kafka/README.md)                                              | [![NuGet](https://img.shields.io/nuget/v/AwsLambda.Host.Envelopes.Kafka.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.Kafka)                     | [![Downloads](https://img.shields.io/nuget/dt/AwsLambda.Host.Envelopes.Kafka.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.Kafka/)                     |
| CloudWatch Logs                      | [**AwsLambda.Host.Envelopes.CloudWatchLogs**](./AwsLambda.Host.Envelopes.CloudWatchLogs/README.md)                            | [![NuGet](https://img.shields.io/nuget/v/AwsLambda.Host.Envelopes.CloudWatchLogs.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.CloudWatchLogs)   | [![Downloads](https://img.shields.io/nuget/dt/AwsLambda.Host.Envelopes.CloudWatchLogs.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.CloudWatchLogs/)   |
| Application Load Balancer            | [**AwsLambda.Host.Envelopes.Alb**](./AwsLambda.Host.Envelopes.Alb/README.md)                                                  | [![NuGet](https://img.shields.io/nuget/v/AwsLambda.Host.Envelopes.Alb.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.Alb)                         | [![Downloads](https://img.shields.io/nuget/dt/AwsLambda.Host.Envelopes.Alb.svg)](https://www.nuget.org/packages/AwsLambda.Host.Envelopes.Alb/)                         |

Each package has detailed documentation in its own README file.
