using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using NQuery.Hosting;

namespace NQuery.Symbols
{
    internal sealed class EumerableTableDefinition : TableDefinition
    {
        private readonly string _name;
        private readonly IEnumerable _source;
        private readonly Type _rowType;
        private readonly IPropertyProvider _propertyProvider;

        public EumerableTableDefinition(string name, IEnumerable source, Type rowType, IPropertyProvider propertyProvider)
        {
            _name = name;
            _source = source;
            _rowType = rowType;
            _propertyProvider = propertyProvider;
        }

        protected override IEnumerable<ColumnDefinition> GetColumns()
        {
            return _propertyProvider.GetProperties(_rowType)
                                    .Select(p => new PropertyColumnDefinition(_rowType, p));
        }

        public override IEnumerable GetRows()
        {
            return _source;
        }

        public override string Name
        {
            get { return _name; }
        }

        public override Type RowType
        {
            get { return _rowType; }
        }
    }
}