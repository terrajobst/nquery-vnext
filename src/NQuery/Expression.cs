using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using NQuery.Symbols;

namespace NQuery
{
    public sealed class Expression<T>
    {
        private readonly Compilation _compilation;
        private readonly T _nullValue;
        private readonly Type _targetType;

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

            _nullValue = nullValue;
            _targetType = targetType;
            var syntaxTree = SyntaxTree.ParseExpression(text);
            _compilation = new Compilation(syntaxTree, dataContext);
        }

        private sealed class FakeTable : TableDefinition
        {
            public override string Name
            {
                get { return "$FakeTable"; }
            }

            public override Type RowType
            {
                get { return TypeFacts.Missing; }
            }

            protected override IEnumerable<ColumnDefinition> GetColumns()
            {
                return Enumerable.Empty<ColumnDefinition>();
            }

            public override IEnumerable GetRows()
            {
                return new object[] { 0 };
            }
        }

        private Query CreateFakeQuery()
        {
            var fakeTable = new SchemaTableSymbol(new FakeTable());
            var dataContext = _compilation.DataContext.AddTables(fakeTable);
            var text = string.Format("SELECT {0} FROM [$FakeTable]", Text);
            return new Query(dataContext, text);
        }

        public Type Resolve()
        {
            // TODO: That's a hack -- expressions should be first class citizens.
            // TODO: We need to validate the TargetType.
            var query = CreateFakeQuery();
            using (var reader = query.ExecuteSchemaReader())
                return reader.GetColumnType(0);
        }

        public T Evaluate()
        {
            // TODO: That's a hack -- expressions should be first class citizens.
            // TODO: We need to validate the TargetType.
            var query = CreateFakeQuery();
            var result = query.ExecuteScalar();
            return result == null ? NullValue : (T) result;
        }

        public DataContext DataContext
        {
            get { return _compilation.DataContext; }
        }

        public string Text
        {
            get { return _compilation.SyntaxTree.TextBuffer.Text; }
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