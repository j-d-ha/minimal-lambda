// Portions of this file are derived from azure-functions-dotnet-worker
// Source:
// https://github.com/Azure/azure-functions-dotnet-worker/blob/2.51.0/src/DotNetWorker.Core/Context/Features/IInvocationFeatures.cs
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License
// See THIRD-PARTY-LICENSES.txt file in the project root or visit
// https://github.com/Azure/azure-functions-dotnet-worker/blob/2.51.0/LICENSE

namespace MinimalLambda;

/// <summary>A type-keyed collection of features available during a Lambda invocation.</summary>
/// <remarks>
///     <para>
///         <see cref="IFeatureCollection" /> provides a dictionary-like mechanism for storing and
///         retrieving features by type. Features allow handlers and middleware to access
///         invocation-specific functionality without tight coupling.
///     </para>
/// </remarks>
public interface IFeatureCollection : IEnumerable<KeyValuePair<Type, object>>
{
    /// <summary>Stores a feature instance in the collection.</summary>
    /// <typeparam name="T">The type of the feature to store.</typeparam>
    /// <param name="instance">The feature instance to store.</param>
    void Set<T>(T instance);

    /// <summary>Retrieves a feature instance from the collection by type.</summary>
    /// <typeparam name="T">The type of the feature to retrieve.</typeparam>
    /// <returns>The feature instance if found; otherwise <c>null</c>.</returns>
    T? Get<T>();
}
