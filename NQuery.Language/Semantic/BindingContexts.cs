using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace NQuery.Language.Semantic
{
    internal abstract class BindingContext
    {
        public abstract IEnumerable<Symbol> LookupSymbols();

        public IEnumerable<Symbol> LookupSymbols(string name, bool matchCase)
        {
            var ordinalIgnoreCase = matchCase
                                            ? StringComparison.Ordinal
                                            : StringComparison.OrdinalIgnoreCase;

            return LookupSymbols().Where(t => t.Name.Equals(name, ordinalIgnoreCase));
        }
    }

    internal sealed class SchemaBindingConext : BindingContext
    {
        private readonly ReadOnlyCollection<SchemaTableSymbol> _schemaTables;

        public SchemaBindingConext(ReadOnlyCollection<SchemaTableSymbol> schemaTables)
        {
            _schemaTables = schemaTables;
        }

        public override IEnumerable<Symbol> LookupSymbols()
        {
            return _schemaTables;
        }
    }

    internal sealed class TableBindingContext : BindingContext
    {
        private readonly BindingContext _parent;
        private readonly BoundTableReference _tableReference;

        public TableBindingContext(BindingContext parent, BoundTableReference tableReference)
        {
            _parent = parent;
            _tableReference = tableReference;
        }

        public override IEnumerable<Symbol> LookupSymbols()
        {
            var parentSymbols = _parent.LookupSymbols();
            var tableSymbols = _tableReference.GetDeclaredTableInstances();
            return parentSymbols.Concat(tableSymbols);
        }
    }

    internal sealed class JoinConditionBindingContext : BindingContext
    {
        private readonly BindingContext _parent;
        private readonly BoundTableReference _left;
        private readonly BoundTableReference _right;

        public JoinConditionBindingContext(BindingContext parent, BoundTableReference left, BoundTableReference right)
        {
            _parent = parent;
            _left = left;
            _right = right;
        }

        public override IEnumerable<Symbol> LookupSymbols()
        {
            var parentSymbols = _parent.LookupSymbols();
            var leftSymbols = _left.GetDeclaredTableInstances();
            var rightSymbols = _right.GetDeclaredTableInstances();
            return parentSymbols.Concat(leftSymbols).Concat(rightSymbols);
        }
    }

    internal sealed class ExpressionBindingContext : BindingContext
    {
        private readonly BindingContext _parent;

        public ExpressionBindingContext(BindingContext parent)
        {
            _parent = parent;
        }

        public override IEnumerable<Symbol> LookupSymbols()
        {
            foreach (var lookupSymbol in _parent.LookupSymbols())
            {
                yield return lookupSymbol;

                var tableInstance = lookupSymbol as TableInstanceSymbol;
                if (tableInstance != null)
                {
                    // TODO: I think a TableInstanceSymbol should have all the ColumnInstanceSymbols
                    foreach (var columnSymbol in tableInstance.Table.Columns)
                        yield return columnSymbol;
                }
            }
        }
    }
}