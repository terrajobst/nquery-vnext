using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using NQuery.Symbols;
using NQuery.Syntax;

namespace NQuery.Authoring.CodeActions.Issues
{
    internal sealed class OrderByExpressionsCodeIssueProvider : CodeIssueProvider<OrderedQuerySyntax>
    {
        protected override IEnumerable<CodeIssue> GetIssues(SemanticModel semanticModel, OrderedQuerySyntax node)
        {
            foreach (var orderByColumn in node.Columns)
            {
                var selector = orderByColumn.ColumnSelector;

                // If the selector refers to a column, we don't want to report
                // an issue because it's too trivial to be replaced.

                if (IsTrivialColumnReference(selector, semanticModel))
                    continue;

                // Selector is an expression.
                // 
                // Now let's see whether we can find the expression in the
                // SELECT list of the underlying query.
                //
                // We aren't actually comparing the expressions ourselves.
                // Instead, we just use the query's output columns to
                // and correlate this with the symbol the ORDER BY selector
                // is bound to.

                var boundSymbol = semanticModel.GetSymbol(orderByColumn);
                var expressionColumns = semanticModel.GetOutputColumns(node.Query).ToList();
                var index = expressionColumns.IndexOf(boundSymbol);
                if (index < 0)
                    continue;

                // The expression is found. Now we only need to compute a
                // reference to the underlying SELECT list.

                var columnReference = GetColumnReference(index, boundSymbol);

                var codeAction = new[] {new ReplaceSelectorCodeAction(selector, columnReference)};
                yield return new CodeIssue(CodeIssueKind.Warning, selector.Span, codeAction);
            }
        }

        private static bool IsTrivialColumnReference(ExpressionSyntax selector, SemanticModel semanticModel)
        {
            return IsColumnReference(selector, semanticModel) ||
                   IsOrdinalColumnReference(selector);
        }

        private static bool IsColumnReference(ExpressionSyntax selector, SemanticModel semanticModel)
        {
            // NOTE: We don't check the syntax because using the symbol allows
            //       to capture any syntax that would just resolve to column,
            //       such "FirstName" or "e.FirstName".

            return semanticModel.GetSymbol(selector) is ColumnInstanceSymbol;
        }

        private static bool IsOrdinalColumnReference(ExpressionSyntax selector)
        {
            var literal = selector as LiteralExpressionSyntax;
            return literal != null && literal.Value is int;
        }

        private static string GetColumnReference(int index, QueryColumnInstanceSymbol column)
        {
            if (!string.IsNullOrEmpty(column.Name))
                return SyntaxFacts.GetValidIdentifier(column.Name);

            var ordinal = index + 1;
            return ordinal.ToString(CultureInfo.InvariantCulture);
        }

        private sealed class ReplaceSelectorCodeAction : ICodeAction
        {
            private readonly ExpressionSyntax _selector;
            private readonly string _columnReference;

            public ReplaceSelectorCodeAction(ExpressionSyntax selector, string columnReference)
            {
                _selector = selector;
                _columnReference = columnReference;
            }

            public string Description
            {
                get { return "Replace expression by SELECT column reference"; }
            }

            public SyntaxTree GetEdit()
            {
                var syntaxTree = _selector.SyntaxTree;
                return syntaxTree.ReplaceText(_selector.Span, _columnReference);
            }
        }
    }
}