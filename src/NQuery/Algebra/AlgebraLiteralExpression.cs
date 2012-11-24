using System;

namespace NQuery.Algebra
{
    internal sealed class AlgebraLiteralExpression : AlgebraExpression
    {
        private readonly object _value;

        public AlgebraLiteralExpression(object value)
        {
            _value = value;
        }

        public override AlgebraKind Kind
        {
            get { return AlgebraKind.LiteralExpression; }
        }

        public object Value
        {
            get { return _value; }
        }
    }
}