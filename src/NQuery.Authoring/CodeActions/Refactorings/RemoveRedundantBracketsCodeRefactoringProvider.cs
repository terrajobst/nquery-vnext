using NQuery.CodeAnalysis;
using NQuery.CodeAnalysis.Text;

namespace NQuery.Authoring.CodeActions.Refactorings;

internal sealed class RemoveRedundantBracketsCodeRefactoringProvider : ICodeRefactoringProvider
{
    public IEnumerable<ICodeAction> GetRefactorings(DocumentView view, CancellationToken cancellationToken)
    {
        ThrowIfNull(view);

        var syntaxTree = view.Document.GetSyntaxTree(cancellationToken);
        var position = view.Position;
        var token = syntaxTree.Root.FindTokenOnLeft(position);
        if (token.Kind != SyntaxKind.IdentifierToken || !token.IsParenthesizedIdentifier())
            return [];

        var identifierText = token.ValueText;
        var isRedundant = SyntaxFacts.IsValidIdentifier(identifierText);
        if (!isRedundant)
            return [];

        return new[] { new RemoveRedundantBracketsCodeAction(token) };
    }

    private sealed class RemoveRedundantBracketsCodeAction : CodeAction
    {
        private readonly SyntaxToken _token;

        public RemoveRedundantBracketsCodeAction(SyntaxToken token)
            : base(token.Parent!.SyntaxTree)
        {
            _token = token;
        }

        public override string Description => Resources.CodeActionRemoveRedundantBrackets;

        protected override void GetChanges(TextChangeSet changeSet)
        {
            changeSet.ReplaceText(_token.Span, _token.ValueText);
        }
    }
}
