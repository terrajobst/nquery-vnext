using System.Linq.Expressions;
using System.Reflection;

namespace NQuery.Metadata;

public abstract class PropertyDefinition
{
    private protected PropertyDefinition(string name, Type type)
    {
        ThrowIfNull(name);
        ThrowIfNull(type);

        Name = name;
        Type = type;
    }

    public string Name { get; }

    public Type Type { get; }

    internal abstract Expression CreateInvocation(Expression instance);

    public static PropertyDefinition Create(PropertyInfo propertyInfo)
    {
        ThrowIfNull(propertyInfo);

        return Create(propertyInfo, propertyInfo.Name);
    }

    public static PropertyDefinition Create(PropertyInfo propertyInfo, string name)
    {
        ThrowIfNull(propertyInfo);
        ThrowIfNull(name);

        return new ReflectionPropertyDefinition(propertyInfo, name);
    }

    public static PropertyDefinition Create(FieldInfo fieldInfo)
    {
        ThrowIfNull(fieldInfo);

        return Create(fieldInfo, fieldInfo.Name);
    }

    public static PropertyDefinition Create(FieldInfo fieldInfo, string name)
    {
        ThrowIfNull(fieldInfo);
        ThrowIfNull(name);

        return new ReflectionFieldDefinition(fieldInfo, name);
    }
}
