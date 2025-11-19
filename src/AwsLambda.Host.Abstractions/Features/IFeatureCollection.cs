// Portions of this file are derived from azure-functions-dotnet-worker
// Source:
// https://github.com/Azure/azure-functions-dotnet-worker/blob/main/src/DotNetWorker.Core/Context/Features/IInvocationFeatures.cs
// Copyright (c) .NET Foundation
// Licensed under the MIT License
// See THIRD-PARTY-LICENSES.txt file in the project root or visit
// https://github.com/Azure/azure-functions-dotnet-worker/blob/2.51.0/LICENSE

namespace AwsLambda.Host;

public interface IFeatureCollection : IEnumerable<KeyValuePair<Type, object>>
{
    void Set<T>(T instance);

    T? Get<T>();
}
