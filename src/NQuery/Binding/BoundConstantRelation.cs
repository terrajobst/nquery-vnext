using System;

namespace NQuery.Binding
{
    internal sealed class BoundConstantRelation : BoundRelation
    {
        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.ConstantRelation; }
        }
    }
}