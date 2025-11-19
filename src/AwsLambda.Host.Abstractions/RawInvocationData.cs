namespace AwsLambda.Host;

public class RawInvocationData
{
    public Stream Event { get; init; }

    public Stream Response { get; set; }
};
