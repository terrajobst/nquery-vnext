using System;

namespace NQuery
{
    public interface ITypeRegistry<out T>
    {
        T DefaultValue { get; }
        T LookupValue(Type type);
    }
}