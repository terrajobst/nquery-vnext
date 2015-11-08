using System;
using System.Linq.Expressions;
using System.Reflection;

namespace NQuery.Symbols
{
    public class ReflectionFieldSymbol : PropertySymbol
    {
        public ReflectionFieldSymbol(FieldInfo fieldInfo)
            : this(fieldInfo, fieldInfo?.Name)
        {
        }

        public ReflectionFieldSymbol(FieldInfo fieldInfo, string name)
            : base(name, fieldInfo?.FieldType)
        {
            if (fieldInfo == null)
                throw new ArgumentNullException(nameof(fieldInfo));

            FieldInfo = fieldInfo;
        }

        public override Expression CreateInvocation(Expression instance)
        {
            return Expression.MakeMemberAccess(instance, FieldInfo);
        }

        public FieldInfo FieldInfo { get; }
    }
}