using System;
using System.Collections.Generic;

namespace NQuery.Binding
{
    internal sealed class BoundAssertRelation : BoundRelation
    {
        public BoundAssertRelation(BoundRelation input, BoundExpression condition, string message)
        {
            Input = input;
            Condition = condition;
            Message = message;
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.AssertRelation; }
        }

        public BoundRelation Input { get; }

        public BoundExpression Condition { get; }

        public string Message { get; }

        public BoundAssertRelation Update(BoundRelation input, BoundExpression condition, string message)
        {
            if (input == Input && condition == Condition && message == Message)
                return this;

            return new BoundAssertRelation(input, condition, message);
        }

        public override IEnumerable<ValueSlot> GetDefinedValues()
        {
            return Input.GetDefinedValues();
        }

        public override IEnumerable<ValueSlot> GetOutputValues()
        {
            return Input.GetOutputValues();
        }
    }
}