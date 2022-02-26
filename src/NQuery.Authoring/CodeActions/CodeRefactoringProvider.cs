namespace NQuery.Authoring.CodeActions
{
    public abstract class CodeRefactoringProvider<T> : ICodeRefactoringProvider
        where T : SyntaxNode
    {
        public IEnumerable<ICodeAction> GetRefactorings(SemanticModel semanticModel, int position)
        {
            var syntaxTree = semanticModel.SyntaxTree;
            return syntaxTree.Root.FindNodes<T>(position)
                                  .SelectMany(n => GetRefactorings(semanticModel, position, n));
        }

        protected abstract IEnumerable<ICodeAction> GetRefactorings(SemanticModel semanticModel, int position, T node);
    }
}