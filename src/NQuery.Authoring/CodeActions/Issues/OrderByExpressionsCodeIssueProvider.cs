﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;

using NQuery.Symbols;
using NQuery.Syntax;
using NQuery.Text;

namespace NQuery.Authoring.CodeActions.Issues
{
    internal sealed class OrderByExpressionsCodeIssueProvider : CodeIssueProvider<OrderedQuerySyntax>
    {
        protected override IEnumerable<CodeIssue> GetIssues(SemanticModel semanticModel, OrderedQuerySyntax node)
        {
            foreach (var orderByColumn in node.Columns)
            {
                var selector = orderByColumn.Selector;

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

                var boundSymbol = semanticModel.GetSymbol(selector);
                var expressionColumns = semanticModel.GetOutputColumns(node.Query).ToImmutableArray();
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

        private static bool IsTrivialColumnReference(OrderBySelectorSyntax selector, SemanticModel semanticModel)
        {
            if (selector is OrdinalOrderBySelectorSyntax)
                return true;

            var expressionSelector = (ExpressionOrderBySelectorSyntax)selector;
            return IsColumnReference(expressionSelector.Expression, semanticModel);
        }

        private static bool IsColumnReference(ExpressionSyntax selector, SemanticModel semanticModel)
        {
            // NOTE: We don't check the syntax because using the symbol allows
            //       to capture any syntax that would just resolve to column,
            //       such "FirstName" or "e.FirstName".

            return semanticModel.GetSymbol(selector) is ColumnInstanceSymbol;
        }

        private static string GetColumnReference(int index, QueryColumnInstanceSymbol column)
        {
            if (!string.IsNullOrEmpty(column.Name))
                return SyntaxFacts.GetValidIdentifier(column.Name);

            var ordinal = index + 1;
            return ordinal.ToString(CultureInfo.InvariantCulture);
        }

        private sealed class ReplaceSelectorCodeAction : CodeAction
        {
            private readonly OrderBySelectorSyntax _selector;
            private readonly string _columnReference;

            public ReplaceSelectorCodeAction(OrderBySelectorSyntax selector, string columnReference)
                : base(selector.SyntaxTree)
            {
                _selector = selector;
                _columnReference = columnReference;
            }

            public override string Description
            {
                get { return Resources.CodeActionReplaceByNamedColumn; }
            }

            protected override void GetChanges(TextChangeSet changeSet)
            {
                changeSet.ReplaceText(_selector.Span, _columnReference);
            }
        }
    }
}