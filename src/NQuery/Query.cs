using System;
using System.Threading;

namespace NQuery
{
    public sealed class Query
    {
        private readonly Compilation _compilation;

        private CompiledResult _result;

        public Query(DataContext dataContext, string text)
        {
            if (dataContext == null)
                throw new ArgumentNullException("dataContext");

            if (text == null)
                throw new ArgumentNullException("text");

            var syntaxTree = SyntaxTree.ParseQuery(text);
            _compilation = new Compilation(syntaxTree, dataContext);
        }

        private void EnsureCompiled()
        {
            if (_result == null)
                Interlocked.CompareExchange(ref _result, _compilation.Compile(), null);
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
            return _result.CreateQueryReader(false);
        }

        public QueryReader ExecuteSchemaReader()
        {
            EnsureCompiled();
            return _result.CreateQueryReader(true);
        }

        public DataContext DataContext
        {
            get { return _compilation.DataContext; }
        }

        public string Text
        {
            get { return _compilation.SyntaxTree.TextBuffer.Text; }
        }
    }
}