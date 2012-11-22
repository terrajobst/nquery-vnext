namespace NQuery.Authoring.Selection
{
    public static class SelectionExtensions
    {
        public static TextSpan ExtendSelection(this SyntaxTree syntaxTree, TextSpan selectedSpan)
        {
            var token = syntaxTree.Root.FindToken(selectedSpan.Start).GetPreviousTokenIfEndOfFile();
            if (!selectedSpan.Contains(token.Span))
                return token.Span;

            var node = token.Parent;
            while (node != null)
            {
                if (selectedSpan.Contains(node.Span))
                    node = node.Parent;
                else
                    return node.Span;
            }

            return syntaxTree.Root.Span;
        }
    }
}