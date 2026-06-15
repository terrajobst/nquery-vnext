using System.Linq.Expressions;

namespace NQuery.Metadata;

internal sealed class PropertyColumnDefinition : ColumnDefinition
{
    private readonly Type _rowType;
    private readonly PropertyDefinition _property;

    public PropertyColumnDefinition(Type rowType, PropertyDefinition property)
        : base(property.Name, property.Type)
    {
        _rowType = rowType;
        _property = property;
    }

    internal override Expression CreateInvocation(Expression instance)
    {
        return
            Expression.Convert(
                _property.CreateInvocation(
                    Expression.Convert(
                        instance,
                        _rowType
                    )
                ),
                typeof(object)
            );
    }
}
