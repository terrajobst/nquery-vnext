using System;
using System.Threading;

namespace NQuery
{
    public sealed class Query
    {
        private readonly DataContext _dataContext;
        private readonly string _text;

        private CompiledResult _result;

        public Query(DataContext dataContext, string text)
        {
            if (dataContext == null)
                throw new ArgumentNullException("dataContext");

            if (text == null)
                throw new ArgumentNullException("text");

            _dataContext = dataContext;
            _text = text;
        }

        private void EnsureCompiled()
        {
            if (_result != null)
                return;

            var syntaxTree = SyntaxTree.ParseQuery(_text);
            var compilation = new Compilation(syntaxTree, _dataContext);
            Interlocked.CompareExchange(ref _result, compilation.Compile(), null);
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
            get { return _dataContext; }
        }

        public string Text
        {
            get { return _text; }
        }
    }
}