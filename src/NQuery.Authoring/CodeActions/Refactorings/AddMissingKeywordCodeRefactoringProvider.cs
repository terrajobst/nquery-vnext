using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

namespace NQuery.Authoring.CodeActions
{
    [Export(typeof(ICodeRefactoringProvider))]
    internal sealed class AddMissingKeywordCodeRefactoringProvider : CodeRefactoringProvider<SyntaxNode>
    {
        protected override IEnumerable<ICodeAction> GetRefactorings(SemanticModel semanticModel, int position, SyntaxNode node)
        {
            var missingKeywords = node.ChildNodesAndTokens().Where(t => t.IsMissing && t.Kind.IsKeyword()).Select(nt => nt.AsToken());
            return missingKeywords.Select(k => new AddMissingKeywordCodeAction(k));
        }

        private sealed class AddMissingKeywordCodeAction : ICodeAction
        {
            private readonly SyntaxToken _keyword;

            public AddMissingKeywordCodeAction(SyntaxToken keyword)
            {
                _keyword = keyword;
            }

            public string Description
            {
                get { return string.Format("Add missing '{0}' keyword", GetKeywordText()); }
            }

            private string GetKeywordText()
            {
                return _keyword.Kind.GetText();
            }

            public SyntaxTree GetEdit()
            {
                var syntaxTree = _keyword.Parent.SyntaxTree;
                return syntaxTree.InsertText(_keyword.Span.Start, GetKeywordText() + " ");
            }
        }
    }
}