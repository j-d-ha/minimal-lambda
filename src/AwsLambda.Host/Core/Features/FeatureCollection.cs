// Portions of this file are derived from azure-functions-dotnet-worker
// Source:
// https://github.com/Azure/azure-functions-dotnet-worker/blob/2.51.0/src/DotNetWorker.Core/Context/Features/InvocationFeatures.cs
// Copyright (c) .NET Foundation
// Licensed under the MIT License
// See THIRD-PARTY-LICENSES.txt file in the project root or visit
// https://github.com/Azure/azure-functions-dotnet-worker/blob/2.51.0/LICENSE

using System.Collections;

namespace AwsLambda.Host.Core.Features;

internal class FeatureCollection : IFeatureCollection
{
    private readonly Dictionary<Type, object> _features = new();

    public T? Get<T>()
    {
        var type = typeof(T);
        _features.TryGetValue(type, out var feature);

        return feature is null ? default : (T)feature;
    }

    public IEnumerator<KeyValuePair<Type, object>> GetEnumerator() => _features.GetEnumerator();

    public void Set(Type type, object instance)
    {
        ArgumentNullException.ThrowIfNull(type);
        ArgumentNullException.ThrowIfNull(instance);

        _features[type] = instance;
    }

    IEnumerator IEnumerable.GetEnumerator() => _features.GetEnumerator();
}
