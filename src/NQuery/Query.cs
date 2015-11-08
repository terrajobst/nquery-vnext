using System;
using System.Threading;

namespace NQuery
{
    public sealed class Query
    {
        private CompiledQuery _query;

        private Query(DataContext dataContext, string text)
        {
            DataContext = dataContext;
            Text = text;
        }

        public static Query Create(DataContext dataContext, string text)
        {
            if (dataContext == null)
                throw new ArgumentNullException(nameof(dataContext));

            if (text == null)
                throw new ArgumentNullException(nameof(text));

            return new Query(dataContext, text);
        }

        private void EnsureCompiled()
        {
            if (_query != null)
                return;

            var syntaxTree = SyntaxTree.ParseQuery(Text);
            var compilation = Compilation.Create(DataContext, syntaxTree);
            Interlocked.CompareExchange(ref _query, compilation.Compile(), null);
        }

        public object ExecuteScalar()
        {
            using (var reader = ExecuteReader())
            {
                return !reader.Read() || reader.ColumnCount == 0
                           ? null
                           : reader[0];
            }
        }

        public T ExecuteScalar<T>()
        {
            return (T) ExecuteScalar();
        }

        public QueryReader ExecuteReader()
        {
            EnsureCompiled();
            return _query.CreateReader();
        }

        public QueryReader ExecuteSchemaReader()
        {
            EnsureCompiled();
            return _query.CreateSchemaReader();
        }

        public DataContext DataContext { get; }

        public string Text { get; }
    }
}