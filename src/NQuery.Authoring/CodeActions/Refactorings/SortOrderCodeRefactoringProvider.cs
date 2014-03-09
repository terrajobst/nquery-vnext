using System;
using System.Collections.Generic;

using NQuery.Syntax;
using NQuery.Text;

namespace NQuery.Authoring.CodeActions.Refactorings
{
    internal sealed class SortOrderCodeRefactoringProvider : CodeRefactoringProvider<OrderByColumnSyntax>
    {
        protected override IEnumerable<ICodeAction> GetRefactorings(SemanticModel semanticModel, int position, OrderByColumnSyntax node)
        {
            if (node.Modifier == null)
                yield return new ToExplicitSortOrderCodeAction(node);
            else
                yield return new ToImplicitSortOrderCodeAction(node);

            yield return new ToSortOrderCodeAction(node);
        }

        private sealed class ToExplicitSortOrderCodeAction : ICodeAction
        {
            private readonly OrderByColumnSyntax _node;

            public ToExplicitSortOrderCodeAction(OrderByColumnSyntax node)
            {
                _node = node;
            }

            public string Description
            {
                get { return "To explicit sort order"; }
            }

            public SyntaxTree GetEdit()
            {
                return _node.SyntaxTree.InsertText(_node.Span.End, " ASC");
            }
        }

        private sealed class ToImplicitSortOrderCodeAction : ICodeAction
        {
            private readonly OrderByColumnSyntax _node;

            public ToImplicitSortOrderCodeAction(OrderByColumnSyntax node)
            {
                _node = node;
            }

            public string Description
            {
                get { return "To implicit sort order"; }
            }

            public SyntaxTree GetEdit()
            {
                var span = TextSpan.FromBounds(_node.ColumnSelector.Span.End, _node.Modifier.Span.End);
                return _node.SyntaxTree.RemoveText(span);
            }
        }

        private sealed class ToSortOrderCodeAction : ICodeAction
        {
            private readonly OrderByColumnSyntax _node;

            public ToSortOrderCodeAction(OrderByColumnSyntax node)
            {
                _node = node;
            }

            public string Description
            {
                get
                {
                    return _node.Modifier == null || _node.Modifier.Kind == SyntaxKind.AscKeyword
                                ? "To descending"
                                : "To ascending";
                }
            }

            public SyntaxTree GetEdit()
            {
                var syntaxTree = _node.SyntaxTree;
                var modifier = _node.Modifier;

                if (modifier == null)
                    return syntaxTree.InsertText(_node.Span.End, " DESC");

                var newKeyword = modifier.Kind == SyntaxKind.AscKeyword ? "DESC" : "ASC";
                return syntaxTree.ReplaceText(modifier.Span, newKeyword);
            }
        }
    }
}