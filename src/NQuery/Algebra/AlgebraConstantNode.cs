using System;

namespace NQuery.Algebra
{
    internal sealed class AlgebraConstantNode : AlgebraNode
    {
        public override AlgebraKind Kind
        {
            get { return AlgebraKind.Constant;}
        }
    }
}