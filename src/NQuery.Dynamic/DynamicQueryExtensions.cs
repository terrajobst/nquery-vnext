using System.Collections.Frozen;

namespace NQuery.Dynamic;

public static class DynamicQueryExtensions
{
    extension(Query query)
    {
        public IEnumerable<dynamic> ExecuteDynamicSequence()
        {
            using var reader = query.ExecuteReader();
            while (reader.Read())
            {
                var values = reader.GetValues();
                yield return new DynamicRow(values);
            }
        }
    }

    extension(QueryReader reader)
    {
        private FrozenDictionary<string, object> GetValues()
        {
            var result = new Dictionary<string, object>(reader.ColumnCount, StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < reader.ColumnCount; i++)
            {
                var key = reader.GetColumnName(i);
                var value = reader[i];
                result[key] = value;
            }

            return result.ToFrozenDictionary();
        }
    }
}
