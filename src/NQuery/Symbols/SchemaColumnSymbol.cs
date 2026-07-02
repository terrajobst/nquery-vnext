using System.Linq.Expressions;

namespace NQuery.Symbols
{
    public abstract class SchemaColumnSymbol : ColumnSymbol
    {
        protected SchemaColumnSymbol(string name, Type type)
            : base(name, type)
        {
        }

        public abstract Expression CreateInvocation(Expression instance);
    }
}