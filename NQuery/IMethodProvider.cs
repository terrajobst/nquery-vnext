using System;
using System.Collections.Generic;
using NQuery.Language.Symbols;

namespace NQuery.Language
{
    public interface IMethodProvider
    {
        IEnumerable<MethodSymbol> GetMethods(Type type);
    }
}