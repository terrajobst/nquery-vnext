using System;
using System.Collections.Generic;
using System.Linq;

using NQuery.Text;

namespace NQuery.Authoring.CodeActions.Refactorings
{
    internal sealed class AddMissingKeywordCodeRefactoringProvider : CodeRefactoringProvider<SyntaxNode>
    {
        protected override IEnumerable<ICodeAction> GetRefactorings(SemanticModel semanticModel, int position, SyntaxNode node)
        {
            var missingKeywords = node.ChildTokens().Where(t => t.IsMissing && t.Kind.IsKeyword());
            return missingKeywords.Select(k => new AddMissingKeywordCodeAction(k));
        }

        private sealed class AddMissingKeywordCodeAction : CodeAction
        {
            private readonly SyntaxToken _keyword;

            public AddMissingKeywordCodeAction(SyntaxToken keyword)
                : base(keyword.Parent.SyntaxTree)
            {
                _keyword = keyword;
            }

            public override string Description
            {
                get { return $"Add missing '{GetKeywordText()}' keyword"; }
            }

            private string GetKeywordText()
            {
                return _keyword.Kind.GetText();
            }

            protected override void GetChanges(TextChangeSet changeSet)
            {
                changeSet.InsertText(_keyword.Span.Start, GetKeywordText() + " ");
            }
        }
    }
}