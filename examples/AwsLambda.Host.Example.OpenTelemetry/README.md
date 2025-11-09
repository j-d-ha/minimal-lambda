# AwsLambda.Host.Example.OpenTelemetry

## Getting Started

You may need to CD into this directory before running the following commands.

```bash
cd ./examples/AwsLambda.Host.Example.OpenTelemetry
```

### Dependencies

These are the requirements to run this example:

- [Docker](https://docs.docker.com/get-docker/)
- [Docker Compose](https://docs.docker.com/compose/install/)
- [AWS Lambda Test Tool](https://github.com/aws/aws-lambda-dotnet/tree/master/Tools/LambdaTestTool-v2)

### Jaeger

**Start Jaeger**

```bash
docker compose up
```

This should start Jaeger on [`http://localhost:16686`](http://localhost:16686).

### Lambda Local Emulator

The Lambda Local Emulator is used to run the Lambda function locally.
It can be started with the following command:

```bash
dotnet lambda-test-tool start --lambda-emulator-port 5050
```