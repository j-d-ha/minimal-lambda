# Sample CloudWatch Logs Events

## String Log Event

### Event

```json
{
  "awslogs": {
    "data": "H4sIAB+5KWkAA7VUy27iMBRdw1dYXhNiOzgPdlSEikXaimRmMRhVJjEoUh5MHm0ziH+vHUI7bQcxlVorimyf+zr33GTfBwCmoiz5VgTNTsAxgNNJMLn3XN+fXLtwoAzyx0wUCsLEGFHTsh2EyRFK8u11kdc7hTKdP5ZMT3i6jjjT00bb1FlYxXmmZTwVLw5+VQieKg+CyIjpGDOdWExfrl7j83UYic378zFEWa/LsIh3KvIsTipRlDLYUkIS9Br/AwwltDpldx9EVr067Nu3hOJIVWRY1HZM7NARMjF1KMLYRlQtx6EmMhHBJkE2MgghFjJH1KaGYdt2W1kbp4plPyueqpZgC2HDRMf1YtH1W6XbM5iIB5EwOGZwfjO7ZXDATgbt5V2Rh/IYZ1tQiN+1DN2adPv7OGqtZHtkrxg8wDbJYfCF3JwL3PAZbn4wWQRgcSx0HkmPTk1Nbd69TlMFfko1pXDS/8uZOOgCE3KGyVIpswJqWjWMNWIFGI2RfMhQ1v7r/4jNum8BiCcR1pWIQFmHStpNnSTNN7DFF9gaZ9i6N9PPqvYN1ZNL1eN/V79w724/P3a9aV3wqp07dT2kJkjL3lWcJFKnNxhVgCfSvGiAH/8RY0CluN5Vz+NPoLv/UYo2r63u51lc/RWCIjRESAbpeqZ+Tf1D/xkC6wGBiQUAAA=="
  }
}
```

### Event Content

```json
{
  "messageType": "DATA_MESSAGE",
  "owner": "123456789012",
  "logGroup": "/aws/lambda/my-function-name",
  "logStream": "2024/11/27/[$LATEST]1234567890abcdef1234567890abcdef",
  "subscriptionFilters": [
    "MySubscriptionFilter"
  ],
  "logEvents": [
    {
      "id": "37589619540615950118055555995606021620803222706458533888",
      "timestamp": 1701360000000,
      "message": "{\"level\":\"INFO\",\"message\":\"Processing request\",\"request_id\":\"abc123\"}"
    },
    {
      "id": "37589619540615950118055555995606021620803222706458533889",
      "timestamp": 1701360001000,
      "message": "START RequestId: 12345678-1234-1234-1234-123456789012 Version: $LATEST"
    },
    {
      "id": "37589619540615950118055555995606021620803222706458533890",
      "timestamp": 1701360002000,
      "message": "[INFO] 2024-11-27T10:00:02.123Z 12345678-1234-1234-1234-123456789012 Function executed successfully"
    },
    {
      "id": "37589619540615950118055555995606021620803222706458533891",
      "timestamp": 1701360003000,
      "message": "END RequestId: 12345678-1234-1234-1234-123456789012"
    },
    {
      "id": "37589619540615950118055555995606021620803222706458533892",
      "timestamp": 1701360003100,
      "message": "REPORT RequestId: 12345678-1234-1234-1234-123456789012Duration: 1234.56 ms\tBilled Duration: 1235 ms\tMemory Size: 512 MB\tMax Memory Used: 128 MB\tInit Duration: 500.00 ms"
    }
  ]
}
```

## JSON Log Event

### Event

```json
{
  "awslogs": {
    "data": "H4sIAB+5KWkAA7VUy27iMBRdw1dYXhNiOzgPdlSEikXaimRmMRhVJjEoUh5MHm0ziH+vHUI7bQcxlVorimyf+zr33GTfBwCmoiz5VgTNTsAxgNNJMLn3XN+fXLtwoAzyx0wUCsLEGFHTsh2EyRFK8u11kdc7hTKdP5ZMT3i6jjjT00bb1FlYxXmmZTwVLw5+VQieKg+CyIjpGDOdWExfrl7j83UYic378zFEWa/LsIh3KvIsTipRlDLYUkIS9Br/AwwltDpldx9EVr067Nu3hOJIVWRY1HZM7NARMjF1KMLYRlQtx6EmMhHBJkE2MgghFjJH1KaGYdt2W1kbp4plPyueqpZgC2HDRMf1YtH1W6XbM5iIB5EwOGZwfjO7ZXDATgbt5V2Rh/IYZ1tQiN+1DN2adPv7OGqtZHtkrxg8wDbJYfCF3JwL3PAZbn4wWQRgcSx0HkmPTk1Nbd69TlMFfko1pXDS/8uZOOgCE3KGyVIpswJqWjWMNWIFGI2RfMhQ1v7r/4jNum8BiCcR1pWIQFmHStpNnSTNN7DFF9gaZ9i6N9PPqvYN1ZNL1eN/V79w724/P3a9aV3wqp07dT2kJkjL3lWcJFKnNxhVgCfSvGiAH/8RY0CluN5Vz+NPoLv/UYo2r63u51lc/RWCIjRESAbpeqZ+Tf1D/xkC6wGBiQUAAA=="
  }
}
```

### Event Content

```json
{
  "messageType": "DATA_MESSAGE",
  "owner": "123456789012",
  "logGroup": "/aws/lambda/my-function-name",
  "logStream": "2024/11/27/[$LATEST]1234567890abcdef1234567890abcdef",
  "subscriptionFilters": [
    "MySubscriptionFilter"
  ],
  "logEvents": [
    {
      "id": "37589619540615950118055555995606021620803222706458533888",
      "timestamp": 1701360000000,
      "message": "{\"level\":\"INFO\",\"message\":\"Processing request\",\"request_id\":\"abc123\"}"
    }
  ]
}
```

### C# Record

```csharp
public record Log(
    [property: JsonPropertyName("level")] string Level,
    [property: JsonPropertyName("message")] string Message,
    [property: JsonPropertyName("request_id")] string RequestId
);
```