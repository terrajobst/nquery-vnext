namespace System.Collections.Generic;

internal static class DictionaryExtensions
{
    extension<TKey, TValue>(Dictionary<TKey, TValue> dictionary)
        where TKey : notnull
    {
        public TValue AddOrUpdate(TKey key, TValue addValue, Func<TKey, TValue, TValue> updateValueFactory)
        {
            var value = dictionary.TryGetValue(key, out var existing)
                ? updateValueFactory(key, existing)
                : addValue;
            dictionary[key] = value;
            return value;
        }
    }
}
