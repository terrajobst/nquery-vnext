using System;

namespace NQuery.Language
{
    public interface ITypeRegistry<out T>
    {
        T DefaultValue { get; }
        T LookupValue(Type type);
    }
}