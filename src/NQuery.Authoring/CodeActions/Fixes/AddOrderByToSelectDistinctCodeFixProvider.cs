using NQuery.Syntax;
using NQuery.Text;

namespace NQuery.Authoring.CodeActions.Fixes
{
    internal sealed class AddOrderByToSelectDistinctCodeFixProvider : CodeFixProvider
    {
        public override IEnumerable<DiagnosticId> GetFixableDiagnosticIds()
        {
            return new[]
            {
                DiagnosticId.OrderByItemsMustBeInSelectListIfDistinctSpecified
            };
        }

        protected override IEnumerable<ICodeAction> GetFixes(SemanticModel semanticModel, int position, Diagnostic diagnostic)
        {
            var root = semanticModel.SyntaxTree.Root;
            var column = root.DescendantNodes().OfType<OrderByColumnSyntax>().FirstOrDefault(n => n.Span.ContainsOrTouches(position));
            if (column == null)
                return Enumerable.Empty<ICodeAction>();

            var orderedQuery = column.Ancestors().OfType<OrderedQuerySyntax>().FirstOrDefault();
            if (orderedQuery == null)
                return Enumerable.Empty<ICodeAction>();

            var selectQuery = orderedQuery.GetAppliedSelectQuery();
            if (selectQuery == null)
                return Enumerable.Empty<ICodeAction>();

            return new[] {new AddOrderByToSelectDistinctCodeAction(selectQuery, column.ColumnSelector)};
        }

        private sealed class AddOrderByToSelectDistinctCodeAction : CodeAction
        {
            private readonly SelectQuerySyntax _selectQuery;
            private readonly ExpressionSyntax _expression;

            public AddOrderByToSelectDistinctCodeAction(SelectQuerySyntax selectQuery, ExpressionSyntax expression)
                : base(selectQuery.SyntaxTree)
            {
                _selectQuery = selectQuery;
                _expression = expression;
            }

            public override string Description
            {
                get { return string.Format(Resources.CodeActionAddToSelectList, GetExpressionText()); }
            }

            private string GetExpressionText()
            {
                return _expression.ToString().Trim();
            }

            private string GetInsertionText()
            {
                if (_selectQuery.SelectClause.Columns.Last().IsMissing)
                    return @" " + GetExpressionText();

                return @", " + GetExpressionText();
            }

            protected override void GetChanges(TextChangeSet changeSet)
            {
                var insertPosition = _selectQuery.SelectClause.LastToken().Span.End;
                var text = GetInsertionText();
                changeSet.InsertText(insertPosition, text);
            }
        }
    }
}