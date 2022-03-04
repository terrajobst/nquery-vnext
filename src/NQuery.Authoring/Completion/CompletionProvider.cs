namespace NQuery.Authoring.Completion
{
    public abstract class CompletionProvider<T> : ICompletionProvider
        where T : SyntaxNode
    {
        public IEnumerable<CompletionItem> GetItems(SemanticModel semanticModel, int position)
        {
            var syntaxTree = semanticModel.SyntaxTree;
            var token = syntaxTree.Root.FindTokenOnLeft(position);
            var node = token.Parent.AncestorsAndSelf()
                                   .OfType<T>()
                                   .FirstOrDefault();

            return node is null
                    ? Enumerable.Empty<CompletionItem>()
                    : GetItems(semanticModel, position, node);
        }

        protected abstract IEnumerable<CompletionItem> GetItems(SemanticModel semanticModel, int position, T node);
    }
}