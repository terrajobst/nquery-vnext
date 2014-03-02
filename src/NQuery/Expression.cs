using System;
using System.Threading;

namespace NQuery
{
    public sealed class Expression<T>
    {
        private readonly DataContext _dataContext;
        private readonly string _text;
        private readonly T _nullValue;
        private readonly Type _targetType;

        private CompiledQuery _query;

        public Expression(DataContext dataContext, string text)
            : this(dataContext, text, default(T))
        {
        }

        public Expression(DataContext dataContext, string text, T nullValue)
            : this(dataContext, text, nullValue, typeof(T))
        {
        }

        public Expression(DataContext dataContext, string text, T nullValue, Type targetType)
        {
            if (dataContext == null)
                throw new ArgumentNullException("dataContext");

            if (text == null)
                throw new ArgumentNullException("text");

            if (targetType == null)
                throw new ArgumentNullException("targetType");

            if (!typeof(T).IsAssignableFrom(targetType))
                throw new ArgumentException(string.Format("The target type must be a sub type of {0}", typeof(T).FullName), "targetType");

            _dataContext = dataContext;
            _text = text;
            _nullValue = nullValue;
            _targetType = targetType;
        }

        private void EnsureCompiled()
        {
            if (_query != null)
                return;

            var syntaxTree = SyntaxTree.ParseExpression(_text);
            var compilation = new Compilation(_dataContext, syntaxTree);
            Interlocked.CompareExchange(ref _query, compilation.Compile(), null);
        }

        public Type Resolve()
        {
            EnsureCompiled();
            using (var reader = _query.CreateReader(true))
                return reader.GetColumnType(0);
        }

        public T Evaluate()
        {
            EnsureCompiled();
            using (var reader = _query.CreateReader(false))
            {
                var result = !reader.Read() || reader.ColumnCount == 0
                    ? null
                    : reader[0];

                return result == null
                    ? _nullValue
                    : (T) result;
            }
        }

        public DataContext DataContext
        {
            get { return _dataContext; }
        }

        public string Text
        {
            get { return _text; }
        }

        public T NullValue
        {
            get { return _nullValue; }
        }

        public Type TargetType
        {
            get { return _targetType; }
        }
    }
}