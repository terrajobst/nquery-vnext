using System.Collections;

using NQuery.Hosting;

namespace NQuery.Symbols
{
    public abstract class SchemaTableSymbol : TableSymbol
    {
        protected SchemaTableSymbol(string name)
            : base(name)
        {
        }

        public override SymbolKind Kind
        {
            get { return SymbolKind.SchemaTable; }
        }

        public abstract IEnumerable GetRows();

        public static SchemaTableSymbol Create<T>(string name, IEnumerable<T> source)
        {
            ArgumentNullException.ThrowIfNull(name);
            ArgumentNullException.ThrowIfNull(source);

            return Create(name, source, new ReflectionProvider());
        }

        public static SchemaTableSymbol Create<T>(string name, IEnumerable<T> source, IPropertyProvider propertyProvider)
        {
            ArgumentNullException.ThrowIfNull(name);
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(propertyProvider);

            return Create(name, source, typeof(T), propertyProvider);
        }

        public static SchemaTableSymbol Create(string name, IEnumerable source, Type rowType, IPropertyProvider propertyProvider)
        {
            ArgumentNullException.ThrowIfNull(name);
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(rowType);
            ArgumentNullException.ThrowIfNull(propertyProvider);

            return new EnumerableTableSymbol(name, source, rowType, propertyProvider);
        }
    }
}