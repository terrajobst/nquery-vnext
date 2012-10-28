using System;

namespace NQuery.BoundNodes
{
    internal sealed class BoundSelectColumn : BoundNode
    {
        private readonly BoundExpression _expression;
        private readonly string _name;

        public BoundSelectColumn(BoundExpression expression, string name)
        {
            _expression = expression;
            _name = name;
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.SelectColumn; }
        }

        public BoundExpression Expression
        {
            get { return _expression; }
        }

        public string Name
        {
            get { return _name; }
        }
    }
}