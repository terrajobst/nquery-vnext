using System;

namespace NQuery.Algebra
{
    internal sealed class AlgebraCaseExpression : AlgebraExpression
    {
        public override AlgebraKind Kind
        {
            get { return AlgebraKind.CaseExpression; }
        }

        public override Type Type
        {
            get { return TypeFacts.Unknown; }
        }
    }
}