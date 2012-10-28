using System;
using System.Collections.Generic;
using NQuery.Language.Symbols;

namespace NQuery.Language
{
    public interface IPropertyProvider
    {
        IEnumerable<PropertySymbol> GetProperties(Type type);
    }
}