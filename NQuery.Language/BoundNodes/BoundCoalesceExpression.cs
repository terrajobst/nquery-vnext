using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NQuery.Language.Binding;

namespace NQuery.Language.BoundNodes
{
    internal sealed class BoundCoalesceExpression : BoundExpression
    {
        private readonly ReadOnlyCollection<BoundExpression> _arguments;

        public BoundCoalesceExpression(IList<BoundExpression> arguments)
        {
            _arguments = new ReadOnlyCollection<BoundExpression>(arguments);
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.CoalesceExpression; }
        }

        public override Type Type
        {
            get { return _arguments.Select(a => a.Type).DefaultIfEmpty(WellKnownTypes.Unknown).First(); }
        }

        public ReadOnlyCollection<BoundExpression> Arguments
        {
            get { return _arguments; }
        }
    }
}