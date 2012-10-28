using System;
using System.Collections.Generic;
using System.Linq;

namespace NQuery
{
    /// <summary>
    /// This class is used to store references to types. The only thing that is special about this class
    /// is that it also allows to return derived types. For exmaple if there is the class <c>D</c> derived
    /// from class <c>B</c> and <see cref="TypeRegistry{T}"/> contains <c>D</c> but not <c>B</c> a request for
    /// <c>B</c> will return <c>D</c>.
    /// </summary>
    public sealed class TypeRegistry<T> : Dictionary<Type, T>, ITypeRegistry<T>
        where T: class
    {
        public TypeRegistry()
        {
        }

        public TypeRegistry(IEnumerable<KeyValuePair<Type,T>> mapping)
            : base(mapping.ToDictionary(kv => kv.Key, kv => kv.Value))
        {
        }

        public TypeRegistry(IEnumerable<KeyValuePair<Type, T>> mapping, T defaultValue)
            : this(mapping)
        {
            DefaultValue = defaultValue;
        }

        public TypeRegistry(TypeRegistry<T> typeRegistry)
            : this(typeRegistry, typeRegistry.DefaultValue)
        {
        }

        public TypeRegistry(ReadOnlyTypeRegistry<T> typeRegistry)
            : this(typeRegistry, typeRegistry.DefaultValue)
        {
        }

        public T DefaultValue { get; set; }

        public T LookupValue(Type key)
        {
            if (key == null)
                throw new ArgumentNullException("key");

            while (key != null)
            {
                T value;
                if (TryGetValue(key, out value))
                    return value;

                key = key.BaseType;
            }

            return DefaultValue;
        }
    }
}