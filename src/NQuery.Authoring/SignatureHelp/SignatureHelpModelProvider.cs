namespace NQuery.Authoring.SignatureHelp
{
    public abstract class SignatureHelpModelProvider<T> : ISignatureHelpModelProvider
        where T : SyntaxNode
    {
        public SignatureHelpModel GetModel(SemanticModel semanticModel, int position)
        {
            var syntaxTree = semanticModel.SyntaxTree;
            var token = syntaxTree.Root.FindTokenOnLeft(position);
            var node = token.Parent
                            .AncestorsAndSelf()
                            .OfType<T>()
                            .FirstOrDefault(c => c.IsBetweenParentheses(position));

            return node is null
                ? null
                : GetModel(semanticModel, node, position);
        }

        protected abstract SignatureHelpModel GetModel(SemanticModel semanticModel, T node, int position);
    }
}