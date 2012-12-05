using System;

using NQuery.Binding;

namespace NQuery.BoundNodes
{
    internal sealed class BoundSelectColumn : BoundNode
    {
        private readonly string _name;
        private readonly ExpressionSyntax _syntax;
        private readonly ValueSlot _valueSlot;

        public BoundSelectColumn(string name, ExpressionSyntax syntax, ValueSlot valueSlot)
        {
            _name = name;
            _syntax = syntax;
            _valueSlot = valueSlot;
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.SelectColumn; }
        }

        public string Name
        {
            get { return _name; }
        }

        public ExpressionSyntax Syntax
        {
            get { return _syntax; }
        }

        public ValueSlot ValueSlot
        {
            get { return _valueSlot; }
        }
    }
}