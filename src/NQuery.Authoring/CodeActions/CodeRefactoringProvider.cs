using System;
using System.Collections.Generic;
using System.Linq;

namespace NQuery.Authoring.CodeActions
{
    public abstract class CodeRefactoringProvider<T> : ICodeRefactoringProvider
        where T : SyntaxNode
    {
        public IEnumerable<ICodeAction> GetRefactorings(SemanticModel semanticModel, int position)
        {
            var syntaxTree = semanticModel.Compilation.SyntaxTree;
            var syntaxToken = syntaxTree.Root.FindToken(position);
            var synaxNodes = syntaxToken.Parent.AncestorsAndSelf().OfType<T>();
            return synaxNodes.SelectMany(n => GetRefactorings(semanticModel, position, n));
        }

        protected abstract IEnumerable<ICodeAction> GetRefactorings(SemanticModel semanticModel, int position, T node);
    }
}