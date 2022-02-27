using NQuery.Syntax;
using NQuery.Text;

namespace NQuery.Authoring.CodeActions.Refactorings
{
    internal sealed class SortOrderCodeRefactoringProvider : CodeRefactoringProvider<OrderByColumnSyntax>
    {
        protected override IEnumerable<ICodeAction> GetRefactorings(SemanticModel semanticModel, int position, OrderByColumnSyntax node)
        {
            if (node.Modifier is null)
                yield return new ToExplicitSortOrderCodeAction(node);
            else
                yield return new ToImplicitSortOrderCodeAction(node);

            yield return new ToSortOrderCodeAction(node);
        }

        private sealed class ToExplicitSortOrderCodeAction : CodeAction
        {
            private readonly OrderByColumnSyntax _node;

            public ToExplicitSortOrderCodeAction(OrderByColumnSyntax node)
                : base(node.SyntaxTree)
            {
                _node = node;
            }

            public override string Description
            {
                get { return Resources.CodeActionToExplicitSortOrder; }
            }

            protected override void GetChanges(TextChangeSet changeSet)
            {
                changeSet.InsertText(_node.Span.End, @" ASC");
            }
        }

        private sealed class ToImplicitSortOrderCodeAction : CodeAction
        {
            private readonly OrderByColumnSyntax _node;

            public ToImplicitSortOrderCodeAction(OrderByColumnSyntax node)
                : base(node.SyntaxTree)
            {
                _node = node;
            }

            public override string Description
            {
                get { return Resources.CodeActionToImplicitSortOrder; }
            }

            protected override void GetChanges(TextChangeSet changeSet)
            {
                var span = TextSpan.FromBounds(_node.ColumnSelector.Span.End, _node.Modifier.Span.End);
                changeSet.DeleteText(span);
            }
        }

        private sealed class ToSortOrderCodeAction : CodeAction
        {
            private readonly OrderByColumnSyntax _node;

            public ToSortOrderCodeAction(OrderByColumnSyntax node)
                : base(node.SyntaxTree)
            {
                _node = node;
            }

            public override string Description
            {
                get
                {
                    return _node.Modifier is null || _node.Modifier.Kind == SyntaxKind.AscKeyword
                                ? Resources.CodeActionToDescending
                                : Resources.CodeActionToAscending;
                }
            }

            protected override void GetChanges(TextChangeSet changeSet)
            {
                var modifier = _node.Modifier;

                if (modifier is null)
                {
                    changeSet.InsertText(_node.Span.End, @" DESC");
                }
                else
                {
                    var newKeyword = modifier.Kind == SyntaxKind.AscKeyword ? @"DESC" : @"ASC";
                    changeSet.ReplaceText(modifier.Span, newKeyword);
                }
            }
        }
    }
}