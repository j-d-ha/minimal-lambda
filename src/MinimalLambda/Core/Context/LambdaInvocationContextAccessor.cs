// Portions of this file are derived from aspnetcore
// Source:
// https://github.com/dotnet/aspnetcore/blob/v10.0.0/src/Http/Http/src/HttpContextAccessor.cs
// Copyright (c) .NET Foundation and Contributors
// Licensed under the MIT License
// See THIRD-PARTY-LICENSES.txt file in the project root or visit
// https://github.com/dotnet/aspnetcore/blob/v10.0.0/LICENSE.txt

namespace MinimalLambda;

internal class LambdaInvocationContextAccessor : ILambdaInvocationContextAccessor
{
    private static readonly AsyncLocal<LambdaInvocationContextFactoryHolder> ContextHolder = new();

    public ILambdaInvocationContext? LambdaInvocationContext
    {
        get => ContextHolder.Value?.Context;
        set
        {
            ContextHolder.Value?.Context = null;
            if (value is not null)
                ContextHolder.Value = new LambdaInvocationContextFactoryHolder { Context = value };
        }
    }

    private sealed class LambdaInvocationContextFactoryHolder
    {
        internal ILambdaInvocationContext? Context;
    }
}
