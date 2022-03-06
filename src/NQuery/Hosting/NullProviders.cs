using System.Diagnostics.CodeAnalysis;

using NQuery.Symbols;

namespace NQuery.Hosting
{
    public static class NullProviders
    {
        private sealed class NullPropertyProvider : IPropertyProvider
        {
            public IEnumerable<PropertySymbol> GetProperties(Type type)
            {
                ArgumentNullException.ThrowIfNull(type);

                return Enumerable.Empty<PropertySymbol>();
            }
        }

        private sealed class NullMethodProvider : IMethodProvider
        {
            public IEnumerable<MethodSymbol> GetMethods(Type type)
            {
                ArgumentNullException.ThrowIfNull(type);

                return Enumerable.Empty<MethodSymbol>();
            }
        }

        public static readonly IPropertyProvider PropertyProvider = new NullPropertyProvider();
        public static readonly IMethodProvider MethodProvider = new NullMethodProvider();
    }
}