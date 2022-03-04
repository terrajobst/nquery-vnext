using NQuery.Symbols;
using NQuery.Syntax;
using NQuery.Text;

namespace NQuery.Authoring.CodeActions.Refactorings
{
    internal sealed class QualifyColumnCodeRefactoringProvider : CodeRefactoringProvider<NameExpressionSyntax>
    {
        protected override IEnumerable<ICodeAction> GetRefactorings(SemanticModel semanticModel, int position, NameExpressionSyntax node)
        {
            if (node.Parent is PropertyAccessExpressionSyntax)
                return Enumerable.Empty<ICodeAction>();

            var column = semanticModel.GetSymbol(node) as TableColumnInstanceSymbol;
            if (column is null)
                return Enumerable.Empty<ICodeAction>();

            return new ICodeAction[] { new QualifyColumnCodeAction(node, column) };
        }

        private sealed class QualifyColumnCodeAction : CodeAction
        {
            private readonly NameExpressionSyntax _node;
            private readonly TableColumnInstanceSymbol _symbol;

            public QualifyColumnCodeAction(NameExpressionSyntax node, TableColumnInstanceSymbol symbol)
                : base(node.SyntaxTree)
            {
                _node = node;
                _symbol = symbol;
            }

            public override string Description
            {
                get { return Resources.CodeActionQualifyColumn; }
            }

            protected override void GetChanges(TextChangeSet changeSet)
            {
                var name = SyntaxFacts.GetValidIdentifier(_symbol.TableInstance.Name);
                changeSet.InsertText(_node.Span.Start, name + @".");
            }
        }
    }
}