using System;

namespace NQuery.Algebra
{
    internal sealed class AlgebraConstantNode : AlgebraRelation
    {
        public override AlgebraKind Kind
        {
            get { return AlgebraKind.Constant;}
        }
    }
}