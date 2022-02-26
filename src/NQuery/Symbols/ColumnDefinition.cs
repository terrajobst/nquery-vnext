using System.Linq.Expressions;

namespace NQuery.Symbols
{
    public abstract class ColumnDefinition
    {
        public abstract string Name { get; }
        public abstract Type DataType { get; }
        public abstract Expression CreateInvocation(Expression instance);
    }
}