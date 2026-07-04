using System.Linq.Expressions;
using System.Reflection;

namespace NQuery.Metadata;

internal sealed class ReflectionFieldDefinition : PropertyDefinition
{
    public ReflectionFieldDefinition(FieldInfo fieldInfo, string name)
        : base(fieldInfo.DeclaringType!, name, fieldInfo.FieldType)
    {
        ThrowIfNull(fieldInfo);
        ThrowIfNull(name);

        FieldInfo = fieldInfo;
    }

    internal override Expression CreateInvocation(Expression instance)
    {
        return Expression.MakeMemberAccess(instance, FieldInfo);
    }

    public FieldInfo FieldInfo { get; }
}
