using System;

namespace NQuery
{
    public sealed class Query
    {
        private readonly Compilation _compilation;

        public Query(DataContext dataContext, string text)
        {
            if (dataContext == null)
                throw new ArgumentNullException("dataContext");

            if (text == null)
                throw new ArgumentNullException("text");

            var syntaxTree = SyntaxTree.ParseQuery(text);
            _compilation = new Compilation(syntaxTree, dataContext);
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
            return _compilation.GetQueryReader(false);
        }

        public QueryReader ExecuteSchemaReader()
        {
            return _compilation.GetQueryReader(true);
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