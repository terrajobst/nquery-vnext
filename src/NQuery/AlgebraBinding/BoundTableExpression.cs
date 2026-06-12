using NQuery.Symbols;

using NQuery.Binding;

namespace NQuery.AlgebraBinding
{
    internal sealed class BoundTableExpression : BoundExpression
    {
        public BoundTableExpression(TableInstanceSymbol symbol)
        {
            Symbol = symbol;
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.TableExpression; }
        }

        public override Type Type
        {
            get { return Symbol.Type; }
        }

        public TableInstanceSymbol Symbol { get; }

        public override string ToString()
        {
            return Symbol.ToString();
        }
    }
}