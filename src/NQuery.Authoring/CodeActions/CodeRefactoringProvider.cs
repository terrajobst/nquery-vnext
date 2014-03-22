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
            return from t in syntaxTree.Root.FindStartTokens(position)
                   from n in t.Parent.AncestorsAndSelf().OfType<T>()
                   from r in GetRefactorings(semanticModel, position, n)
                   select r;
        }

        protected abstract IEnumerable<ICodeAction> GetRefactorings(SemanticModel semanticModel, int position, T node);
    }
}