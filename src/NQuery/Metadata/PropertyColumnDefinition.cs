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

    // Returns the property value in its own CLR type -- not boxed to object. The row
    // writer lifts it to the column's nullable shape and stores it typed, so a value-typed
    // column never boxes on the way into the row buffer.
    internal override Expression CreateInvocation(Expression instance)
    {
        return _property.CreateInvocation(Expression.Convert(instance, _rowType));
    }
}
