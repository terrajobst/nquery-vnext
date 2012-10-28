using System;
using System.Collections.Generic;

using NQuery.Symbols;

namespace NQuery
{
    public interface IMethodProvider
    {
        IEnumerable<MethodSymbol> GetMethods(Type type);
    }
}