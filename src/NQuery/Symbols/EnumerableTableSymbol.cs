using System.Collections;
using System.Collections.Immutable;

using NQuery.Hosting;

namespace NQuery.Symbols
{
    internal sealed class EnumerableTableSymbol : SchemaTableSymbol
    {
        private readonly IEnumerable _source;
        private readonly Type _rowType;

        public EnumerableTableSymbol(string name, IEnumerable source, Type rowType, IPropertyProvider propertyProvider)
            : base(name)
        {
            _source = source;
            _rowType = rowType;
            Columns = propertyProvider.GetProperties(_rowType)
                                      .Select(p => (ColumnSymbol)new PropertyColumnSymbol(_rowType, p))
                                      .ToImmutableArray();
        }

        public override Type Type
        {
            get { return _rowType; }
        }

        public override ImmutableArray<ColumnSymbol> Columns { get; }

        public override IEnumerable GetRows()
        {
            return _source;
        }
    }
}