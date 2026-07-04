using System.Linq.Expressions;
using System.Reflection;

namespace NQuery.Metadata;

internal sealed class ReflectionPropertyDefinition : PropertyDefinition
{
    public ReflectionPropertyDefinition(PropertyInfo propertyInfo, string name)
        : base(propertyInfo.DeclaringType!, name, propertyInfo.PropertyType)
    {
        ThrowIfNull(propertyInfo);
        ThrowIfNull(name);

        PropertyInfo = propertyInfo;
    }

    internal override Expression CreateInvocation(Expression instance)
    {
        return Expression.MakeMemberAccess(instance, PropertyInfo);
    }

    public PropertyInfo PropertyInfo { get; }
}
