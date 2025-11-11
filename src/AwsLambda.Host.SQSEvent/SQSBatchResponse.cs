using System.Runtime.Serialization;

namespace AwsLambda.Host.SQSEvent;

/// <inheritdoc />
[DataContract]
public class SQSBatchResponse : Amazon.Lambda.SQSEvents.SQSBatchResponse;
