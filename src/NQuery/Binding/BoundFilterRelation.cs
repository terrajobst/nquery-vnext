using System;
using System.Runtime.InteropServices;

namespace NQuery.Binding
{
    internal sealed class BoundFilterRelation : BoundRelation
    {
        private readonly BoundRelation _input;
        private readonly BoundExpression _condition;

        public BoundFilterRelation(BoundRelation input, BoundExpression condition)
        {
            _input = input;
            _condition = condition;
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.FilterRelation; }
        }

        public BoundRelation Input
        {
            get { return _input; }
        }

        public BoundExpression Condition
        {
            get { return _condition; }
        }

        public BoundFilterRelation Update(BoundRelation input, BoundExpression condition)
        {
            if (input == _input && condition == _condition)
                return this;

            return new BoundFilterRelation(input, condition);
        }
    }
}