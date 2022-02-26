using System.Linq.Expressions;

namespace NQuery.Symbols
{
    public abstract class MethodSymbol : InvocableSymbol
    {
        protected MethodSymbol(string name, Type type, IEnumerable<ParameterSymbol> parameters)
            : base(name, type, parameters)
        {
        }

        public abstract Expression CreateInvocation(Expression instance, IEnumerable<Expression> arguments);

        public override SymbolKind Kind
        {
            get { return SymbolKind.Method; }
        }
    }
}