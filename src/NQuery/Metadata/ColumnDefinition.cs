using System.Linq.Expressions;

namespace NQuery.Metadata;

public abstract class ColumnDefinition
{
    private protected ColumnDefinition()
    {
    }

    public abstract string Name { get; }
    public abstract Type DataType { get; }
    internal abstract Expression CreateInvocation(Expression instance);
}
