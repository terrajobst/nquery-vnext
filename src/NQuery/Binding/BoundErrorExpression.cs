using System;

namespace NQuery.Binding
{
    internal sealed class BoundErrorExpression : BoundExpression
    {
        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.ErrorExpression; }
        }

        public override Type Type
        {
            get { return TypeFacts.Unknown; }
        }
    }
}