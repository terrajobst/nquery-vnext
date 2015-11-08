using System;
using System.Collections.Generic;
using System.Linq;

using NQuery.Text;

namespace NQuery.Authoring.CodeActions.Refactorings
{
    internal sealed class RemoveRedundantBracketsCodeRefactoringProvider : ICodeRefactoringProvider
    {
        public IEnumerable<ICodeAction> GetRefactorings(SemanticModel semanticModel, int position)
        {
            var syntaxTree = semanticModel.SyntaxTree;
            var token = syntaxTree.Root.FindTokenOnLeft(position);
            if (token.Kind != SyntaxKind.IdentifierToken || !token.IsParenthesizedIdentifier())
                return Enumerable.Empty<ICodeAction>();

            var identifierText = token.ValueText;
            var isRedundant = SyntaxFacts.IsValidIdentifier(identifierText);
            if (!isRedundant)
                return Enumerable.Empty<ICodeAction>();

            return new[] {new RemoveRedundantBracketsCodeAction(token)};
        }

        private sealed class RemoveRedundantBracketsCodeAction : CodeAction
        {
            private readonly SyntaxToken _token;

            public RemoveRedundantBracketsCodeAction(SyntaxToken token)
                : base(token.Parent.SyntaxTree)
            {
                _token = token;
            }

            public override string Description => "Remove redundant brackets";

            protected override void GetChanges(TextChangeSet changeSet)
            {
                changeSet.ReplaceText(_token.Span, _token.ValueText);
            }
        }
    }
}