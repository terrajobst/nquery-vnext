using System;
using System.Reflection;

using NQuery.Symbols;

namespace NQuery.Runtime
{
    public class ReflectionFieldSymbol : PropertySymbol
    {
        public ReflectionFieldSymbol(FieldInfo fieldInfo)
            : this(fieldInfo, fieldInfo == null ? null : fieldInfo.Name)
        {
        }

        public ReflectionFieldSymbol(FieldInfo fieldInfo, string name)
            : base(name, fieldInfo == null ? null : fieldInfo.FieldType)
        {
            if (fieldInfo == null)
                throw new ArgumentNullException("fieldInfo");

            FieldInfo = fieldInfo;
        }

        public FieldInfo FieldInfo { get; private set; }
    }
}