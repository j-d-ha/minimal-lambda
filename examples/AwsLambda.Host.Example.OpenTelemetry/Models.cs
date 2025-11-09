namespace AwsLambda.Host.Example.OpenTelemetry;

internal record Request(string Name);

internal record Response(string Message, DateTime Timestamp);
