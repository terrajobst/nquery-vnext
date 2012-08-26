using System;
using System.Collections;
using System.Collections.Generic;

namespace NQuery.Language
{
    public sealed class ReadOnlyTypeRegistry<T> : ITypeRegistry<T>, IEnumerable<KeyValuePair<Type,T>>
        where T : class
    {
        private readonly IDictionary<Type, T> _mapping;

        public ReadOnlyTypeRegistry(TypeRegistry<T> typeRegistry)
        {
            if (typeRegistry == null)
                throw new ArgumentNullException("typeRegistry");

            _mapping = typeRegistry;
            DefaultValue = typeRegistry.DefaultValue;
        }

        public ReadOnlyTypeRegistry(IDictionary<Type, T> mapping, T defaultValue)
        {
            if (mapping == null)
                throw new ArgumentNullException("mapping");

            _mapping = mapping;
            DefaultValue = defaultValue;
        }

        public T DefaultValue { get; set; }

        public T LookupValue(Type key)
        {
            if (key == null)
                throw new ArgumentNullException("key");

            while (key != null)
            {
                T value;
                if (_mapping.TryGetValue(key, out value))
                    return value;

                key = key.BaseType;
            }

            return DefaultValue;
        }

        public IEnumerator<KeyValuePair<Type, T>> GetEnumerator()
        {
            return _mapping.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}