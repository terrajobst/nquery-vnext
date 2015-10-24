using System;
using System.Collections.Generic;

namespace NQuery.Binding
{
    internal sealed class BoundAssertRelation : BoundRelation
    {
        private readonly BoundRelation _input;
        private readonly BoundExpression _condition;
        private readonly string _message;

        public BoundAssertRelation(BoundRelation input, BoundExpression condition, string message)
        {
            _input = input;
            _condition = condition;
            _message = message;
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.AssertRelation; }
        }

        public BoundRelation Input
        {
            get { return _input; }
        }

        public BoundExpression Condition
        {
            get { return _condition; }
        }

        public string Message
        {
            get { return _message; }
        }

        public BoundAssertRelation Update(BoundRelation input, BoundExpression condition, string message)
        {
            if (input == _input && condition == _condition && message == _message)
                return this;

            return new BoundAssertRelation(input, condition, message);
        }

        public override IEnumerable<ValueSlot> GetDefinedValues()
        {
            return _input.GetDefinedValues();
        }

        public override IEnumerable<ValueSlot> GetOutputValues()
        {
            return _input.GetOutputValues();
        }
    }
}