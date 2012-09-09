using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace NQuery.Language.BoundNodes
{
    internal sealed class BoundInExpression : BoundExpression
    {
        private readonly BoundExpression _expression;
        private readonly ReadOnlyCollection<BoundExpression> _arguments;

        public BoundInExpression(BoundExpression expression, IList<BoundExpression> arguments)
        {
            _expression = expression;
            _arguments = new ReadOnlyCollection<BoundExpression>(arguments);
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.InExpression; }
        }

        public override Type Type
        {
            get { return typeof(bool); }
        }

        public BoundExpression Expression
        {
            get { return _expression; }
        }

        public ReadOnlyCollection<BoundExpression> Arguments
        {
            get { return _arguments; }
        }
    }
}