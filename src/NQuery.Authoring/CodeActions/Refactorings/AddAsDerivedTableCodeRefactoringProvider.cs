using NQuery.CodeAnalysis;
using NQuery.CodeAnalysis.Syntax;
using NQuery.CodeAnalysis.Text;

namespace NQuery.Authoring.CodeActions.Refactorings;

internal sealed class AddAsDerivedTableCodeRefactoringProvider : CodeRefactoringProvider<DerivedTableReferenceSyntax>
{
    protected override IEnumerable<ICodeAction> GetRefactorings(SemanticModel semanticModel, int position, DerivedTableReferenceSyntax node)
    {
        return node.AsKeyword is null
            ? new[] { new AddAsToDerivedTableCodeAction(node) }
            : Enumerable.Empty<ICodeAction>();
    }

    private sealed class AddAsToDerivedTableCodeAction : CodeAction
    {
        private readonly DerivedTableReferenceSyntax _node;

        public AddAsToDerivedTableCodeAction(DerivedTableReferenceSyntax node)
            : base(node.SyntaxTree)
        {
            ThrowIfNull(node);

            _node = node;
        }

        public override string Description
        {
            get { return Resources.CodeActionAddAsKeyword; }
        }

        protected override void GetChanges(TextChangeSet changeSet)
        {
            changeSet.InsertText(_node.IdentifierToken.Span.Start, @"AS ");
        }
    }
}
