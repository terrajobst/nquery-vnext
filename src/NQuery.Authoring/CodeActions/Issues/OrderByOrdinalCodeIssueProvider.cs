using System;
using System.Collections.Generic;
using System.Linq;

using NQuery.Syntax;

namespace NQuery.Authoring.CodeActions.Issues
{
    internal sealed class OrderByOrdinalCodeIssueProvider : CodeIssueProvider<OrderedQuerySyntax>
    {
        protected override IEnumerable<CodeIssue> GetIssues(SemanticModel semanticModel, OrderedQuerySyntax node)
        {
            return from orderByColumn in node.Columns
                   let selector = orderByColumn.ColumnSelector as LiteralExpressionSyntax
                   where selector != null && selector.Value is int
                   let column = semanticModel.GetSymbol(orderByColumn)
                   where column != null && !string.IsNullOrEmpty(column.Name)
                   let namedReference = SyntaxFacts.GetValidIdentifier(column.Name)
                   let action = new[] {new ReplaceOrdingalByNamedReferenceCodeAction(selector, namedReference)}
                   select new CodeIssue(CodeIssueKind.Warning, selector.Span, action);
        }

        private sealed class ReplaceOrdingalByNamedReferenceCodeAction : ICodeAction
        {
            private readonly ExpressionSyntax _selector;
            private readonly string _columnReference;

            public ReplaceOrdingalByNamedReferenceCodeAction(ExpressionSyntax selector, string columnReference)
            {
                _selector = selector;
                _columnReference = columnReference;
            }

            public string Description
            {
                get { return "Replace ordinal by named column reference"; }
            }

            public SyntaxTree GetEdit()
            {
                var syntaxTree = _selector.SyntaxTree;
                return syntaxTree.ReplaceText(_selector.Span, _columnReference);
            }
        }
    }
}