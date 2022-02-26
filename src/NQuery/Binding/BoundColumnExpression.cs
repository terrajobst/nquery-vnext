using NQuery.Symbols;

namespace NQuery.Binding
{
    internal sealed class BoundColumnExpression : BoundExpression
    {
        public BoundColumnExpression(ColumnInstanceSymbol symbol)
        {
            Symbol = symbol;
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.ColumnExpression;  }
        }

        public override Type Type
        {
            get { return Symbol.Type; }
        }

        public ColumnInstanceSymbol Symbol { get; }

        public override string ToString()
        {
            return Symbol.ToString();
        }
    }
}