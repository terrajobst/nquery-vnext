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
                if (type == null)
                    throw new ArgumentNullException(nameof(type));

                return Enumerable.Empty<PropertySymbol>();
            }
        }

        private sealed class NullMethodProvider : IMethodProvider
        {
            public IEnumerable<MethodSymbol> GetMethods(Type type)
            {
                if (type == null)
                    throw new ArgumentNullException(nameof(type));

                return Enumerable.Empty<MethodSymbol>();
            }
        }

        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly IPropertyProvider PropertyProvider = new NullPropertyProvider();

        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly IMethodProvider MethodProvider = new NullMethodProvider();
    }
}