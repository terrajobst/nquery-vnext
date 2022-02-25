using System;
using System.Collections.Generic;

namespace NQuery.Dynamic
{
    public static class DynamicQueryExtensions
    {
        public static IEnumerable<dynamic> ExecuteDynamicSequence(this Query query)
        {
            using (var reader = query.ExecuteReader())
            {
                while (reader.Read())
                {
                    var values = reader.GetValues();
                    yield return new DynamicRow(values);
                }
            }
        }

        private static IDictionary<string, object> GetValues(this QueryReader reader)
        {
            var result = new Dictionary<string, object>(reader.ColumnCount, StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < reader.ColumnCount; i++)
            {
                var key = reader.GetColumnName(i);
                var value = reader[i];
                result[key] = value;
            }

            return result;
        }
    }
}