using System.Linq.Expressions;

namespace NQuery.Symbols
{
    internal sealed class PropertyColumnDefinition : ColumnDefinition
    {
        private readonly Type _rowType;
        private readonly PropertySymbol _propertySymbol;

        public PropertyColumnDefinition(Type rowType, PropertySymbol propertySymbol)
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

        public override string Name
        {
            get { return _propertySymbol.Name; }
        }

        public override Type DataType
        {
            get { return _propertySymbol.Type; }
        }
    }
}