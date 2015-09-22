using System;
using System.Collections.Generic;
using System.Linq;

using NQuery.Syntax;
using NQuery.Text;

namespace NQuery.Authoring.CodeActions.Fixes
{
    internal sealed class AddToGroupByCodeFixProvider : CodeFixProvider
    {
        public override IEnumerable<DiagnosticId> GetFixableDiagnosticIds()
        {
            return new[]
            {
                DiagnosticId.SelectExpressionNotAggregatedOrGrouped,
                DiagnosticId.SelectExpressionNotAggregatedAndNoGroupBy,
                DiagnosticId.HavingExpressionNotAggregatedOrGrouped,
                DiagnosticId.OrderByExpressionNotAggregatedAndNoGroupBy,
                DiagnosticId.OrderByExpressionNotAggregatedOrGrouped
            };
        }

        protected override IEnumerable<ICodeAction> GetFixes(SemanticModel semanticModel, int position, Diagnostic diagnostic)
        {
            var syntaxTree = semanticModel.SyntaxTree;
            var expression = syntaxTree.Root.DescendantNodes().OfType<ExpressionSyntax>().FirstOrDefault(e => e.Span == diagnostic.Span);
            if (expression == null)
                return Enumerable.Empty<ICodeAction>();

            var selectQuery = GetSelectQuery(expression);
            if (selectQuery == null)
                return Enumerable.Empty<ICodeAction>();

            switch (diagnostic.DiagnosticId)
            {
                case DiagnosticId.SelectExpressionNotAggregatedOrGrouped:
                case DiagnosticId.SelectExpressionNotAggregatedAndNoGroupBy:
                    return GetSelectFixes(selectQuery, expression);

                case DiagnosticId.HavingExpressionNotAggregatedOrGrouped:
                    return GetHavingFixes(selectQuery, expression);

                case DiagnosticId.OrderByExpressionNotAggregatedAndNoGroupBy:
                case DiagnosticId.OrderByExpressionNotAggregatedOrGrouped:
                    return GetOrderByFixes(selectQuery, expression);

                default:
                    throw ExceptionBuilder.UnexpectedValue(diagnostic.DiagnosticId);
            }
        }

        private static SelectQuerySyntax GetSelectQuery(ExpressionSyntax expression)
        {
            var selectQuery = expression.Ancestors().OfType<SelectQuerySyntax>().FirstOrDefault();
            if (selectQuery != null)
                return selectQuery;

            var orderedQuery = expression.Ancestors().OfType<OrderedQuerySyntax>().FirstOrDefault();
            if (orderedQuery != null)
                return orderedQuery.GetAppliedSelectQuery();

            return null;
        }

        private static IEnumerable<ICodeAction> GetSelectFixes(SelectQuerySyntax selectQuery, ExpressionSyntax expression)
        {
            var selectColumn = expression.Ancestors().OfType<ExpressionSelectColumnSyntax>().FirstOrDefault();
            if (selectColumn == null)
                return Enumerable.Empty<ICodeAction>();

            return GetExpressionFixes(selectQuery, selectColumn.Expression, expression);
        }

        private static IEnumerable<ICodeAction> GetHavingFixes(SelectQuerySyntax selectQuery, ExpressionSyntax expression)
        {
            yield return new AddToGroupByCodeAction(selectQuery, expression);
        }

        private static IEnumerable<ICodeAction> GetOrderByFixes(SelectQuerySyntax selectQuery, ExpressionSyntax expression)
        {
            var expressionSelector = expression.Ancestors().OfType<ExpressionOrderBySelectorSyntax>().FirstOrDefault();
            if (expressionSelector == null)
                return Enumerable.Empty<ICodeAction>();

            return GetExpressionFixes(selectQuery, expressionSelector.Expression, expression);
        }

        private static IEnumerable<ICodeAction> GetExpressionFixes(SelectQuerySyntax selectQuery, ExpressionSyntax containingExpression, ExpressionSyntax expression)
        {
            yield return new AddToGroupByCodeAction(selectQuery, expression);

            if (containingExpression != expression)
                yield return new AddToGroupByCodeAction(selectQuery, containingExpression);
        }

        private sealed class AddToGroupByCodeAction : CodeAction
        {
            private readonly SelectQuerySyntax _selectQuery;
            private readonly ExpressionSyntax _expression;

            public AddToGroupByCodeAction(SelectQuerySyntax selectQuery, ExpressionSyntax expression)
                : base(selectQuery.SyntaxTree)
            {
                _selectQuery = selectQuery;
                _expression = expression;
            }

            public override string Description
            {
                get { return string.Format(Resources.CodeActionAddToGroupBy, GetExpressionText()); }
            }

            protected override void GetChanges(TextChangeSet changeSet)
            {
                var position = GetInsertPosition();
                var text = GetTextToInsert();
                changeSet.InsertText(position, text);
            }

            private int GetInsertPosition()
            {
                if (_selectQuery.GroupByClause != null)
                    return _selectQuery.GroupByClause.LastToken().Span.End;

                if (_selectQuery.WhereClause != null)
                    return _selectQuery.WhereClause.LastToken().Span.End;

                if (_selectQuery.FromClause != null)
                    return _selectQuery.FromClause.LastToken().Span.End;

                return _selectQuery.SelectClause.LastToken().Span.End;
            }

            private string GetTextToInsert()
            {
                if (_selectQuery.GroupByClause != null)
                {
                    if (_selectQuery.GroupByClause.Columns.Last().IsMissing)
                        return @" " + GetExpressionText();

                    return @", " + GetExpressionText();
                }

                return Environment.NewLine + @"GROUP   BY " + GetExpressionText();
            }

            private string GetExpressionText()
            {
                return _expression.ToString().Trim();
            }
        }
    }
}