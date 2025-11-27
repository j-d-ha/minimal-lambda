using System.Text.Json.Serialization;
using Amazon.Lambda.KinesisFirehoseEvents;
using AwsLambda.Host.Options;

namespace AwsLambda.Host.Envelopes.KinesisFirehose;

/// <inheritdoc cref="KinesisFirehoseResponse" />
/// <remarks>
///     This abstract class extends <see cref="KinesisFirehoseResponse" /> and provides a
///     foundation for strongly typed response handling. Derived classes implement
///     <see cref="PackPayload" /> to serialize the strongly typed
///     <see cref="FirehoseRecordEnvelope.DataContent" /> property into the response records using
///     their chosen serialization strategy.
/// </remarks>
public abstract class KinesisFirehoseResponseEnvelopeBase<T>
    : KinesisFirehoseResponse,
        IResponseEnvelope
{
    /// <inheritdoc cref="KinesisFirehoseResponse.Records" />
    public new required IList<FirehoseRecordEnvelope> Records { get; set; }

    /// <inheritdoc />
    public abstract void PackPayload(EnvelopeOptions options);

    /// <inheritdoc cref="FirehoseRecord" />
    public class FirehoseRecordEnvelope : FirehoseRecord
    {
        /// <summary>
        ///     Gets and sets the data content to be serialized into
        ///     <see cref="FirehoseRecord.Base64EncodedData" />
        /// </summary>
        [JsonIgnore]
        public T? DataContent { get; set; }
    }
}
