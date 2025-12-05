// Portions of this file are derived from aspnetcore
// Source:
// https://github.com/dotnet/aspnetcore/blob/v10.0.0/src/Mvc/Mvc.Testing/src/WebApplicationFactory.cs
// Copyright (c) .NET Foundation and Contributors
// Licensed under the MIT License
// See THIRD-PARTY-LICENSES.txt file in the project root or visit
// https://github.com/dotnet/aspnetcore/blob/v10.0.0/LICENSE.txt

using Microsoft.Extensions.Hosting;

namespace AwsLambda.Host.Testing;

public class LambdaApplicationFactory<TStartup> : IDisposable, IAsyncDisposable
    where TStartup : class
{
    private bool _disposed;
    private bool _disposedAsync;

    private IHost? _host;

    public async ValueTask DisposeAsync()
    {
        // HostFactoryResolver.

        if (_disposed)
            return;

        if (_disposedAsync)
            return;

        if (_host != null)
        {
            await _host.StopAsync().ConfigureAwait(false);
            _host?.Dispose();
        }

        _disposedAsync = true;

        Dispose(true);

        GC.SuppressFinalize(this);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing && !_disposedAsync)
            DisposeAsync().AsTask().ConfigureAwait(false).GetAwaiter().GetResult();

        _disposed = true;
    }
}
