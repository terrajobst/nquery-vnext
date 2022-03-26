using System.Linq.Expressions;

namespace NQuery.Symbols
{
    internal sealed class PropertyColumnSymbol : SchemaColumnSymbol
    {
        private readonly Type _rowType;
        private readonly PropertySymbol _propertySymbol;

        public PropertyColumnSymbol(Type rowType, PropertySymbol propertySymbol)
            : base(propertySymbol.Name, propertySymbol.Type)
        {
            _rowType = rowType;
            _propertySymbol = propertySymbol;
        }

        public override Expression CreateInvocation(Expression instance)
        {
            return
                Expression.Convert(
                    _propertySymbol.CreateInvocation(
                        Expression.Convert(
                            instance,
                            _rowType
                        )
                    ),
                    typeof(object)
                );
        }
    }
}