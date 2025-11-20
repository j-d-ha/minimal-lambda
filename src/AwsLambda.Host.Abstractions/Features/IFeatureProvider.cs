// Portions of this file are derived from azure-functions-dotnet-worker
// Source:
// https://github.com/Azure/azure-functions-dotnet-worker/blob/main/src/DotNetWorker.Core/Context/Features/IInvocationFeatureProvider.cs
// Copyright (c) .NET Foundation
// Licensed under the MIT License
// See THIRD-PARTY-LICENSES.txt file in the project root or visit
// https://github.com/Azure/azure-functions-dotnet-worker/blob/2.51.0/LICENSE

namespace AwsLambda.Host;

/// <summary>Provides a mechanism to create feature instances on demand.</summary>
/// <remarks>
///     <para>
///         Feature providers are used by <see cref="IFeatureCollection" /> to create features of
///         specific types when requested. This allows features to be instantiated lazily without
///         requiring upfront registration.
///     </para>
///     <para>
///         Registered <see cref="IFeatureProvider" /> instances are provided to the
///         <see cref="IFeatureCollection" /> to enable feature discovery and creation.
///     </para>
/// </remarks>
public interface IFeatureProvider
{
    /// <summary>Attempts to create a feature instance of the specified type.</summary>
    /// <param name="type">The type of feature to create.</param>
    /// <param name="feature">
    ///     When this method returns <c>true</c>, contains the created feature instance;
    ///     otherwise <c>null</c>.
    /// </param>
    /// <returns>
    ///     <c>true</c> if a feature of the specified type was successfully created; otherwise
    ///     <c>false</c>.
    /// </returns>
    bool TryCreate(Type type, out object? feature);
}
