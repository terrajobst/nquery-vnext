using System.Collections.Generic;
using System.Linq;
using NQuery.Language.BoundNodes;
using NQuery.Language.Symbols;

namespace NQuery.Language.Binding
{
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
}