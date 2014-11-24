using System;
using System.Collections.Generic;
using System.Linq;

using NQuery.Syntax;

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
            var root = semanticModel.Compilation.SyntaxTree.Root;
            var column = root.DescendantNodes().OfType<OrderByColumnSyntax>().FirstOrDefault(n => n.Span.ContainsOrTouches(position));
            if (column == null)
                return Enumerable.Empty<ICodeAction>();

            var orderedQuery = column.Ancestors().OfType<OrderedQuerySyntax>().FirstOrDefault();
            if (orderedQuery == null)
                return Enumerable.Empty<ICodeAction>();

            var selectQuery = orderedQuery.GetAppliedSelectQuery();
            if (selectQuery == null)
                return Enumerable.Empty<ICodeAction>();

            return new[] {new CodeAction(selectQuery, column.ColumnSelector)};
        }

        private sealed class CodeAction : ICodeAction
        {
            private readonly SelectQuerySyntax _selectQuery;
            private readonly ExpressionSyntax _expression;

            public CodeAction(SelectQuerySyntax selectQuery, ExpressionSyntax expression)
            {
                _selectQuery = selectQuery;
                _expression = expression;
            }

            public string Description
            {
                get { return string.Format("Add {0} to SELECT list", GetExpressionText()); }
            }

            private string GetExpressionText()
            {
                return _expression.ToString().Trim();
            }

            public SyntaxTree GetEdit()
            {
                var insertPosition = _selectQuery.SelectClause.Span.End;
                var text = ", " + GetExpressionText();
                var syntaxTree = _selectQuery.SyntaxTree;
                return syntaxTree.InsertText(insertPosition, text);
            }
        }
    }
}