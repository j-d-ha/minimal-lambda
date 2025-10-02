namespace Lambda.Host.SourceGenerators.Models;

internal class MapHandlerInvocationInfo
{
    internal required DelegateInfo DelegateInfo { get; set; }
    internal required LocationInfo? LocationInfo { get; set; }
}
