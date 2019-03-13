#nullable enable

using System;
using System.Collections.Generic;

using NQuery.Symbols;

namespace NQuery.Hosting
{
    public interface IMethodProvider
    {
        IEnumerable<MethodSymbol> GetMethods(Type type);
    }
}