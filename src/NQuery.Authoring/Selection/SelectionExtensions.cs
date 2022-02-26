using NQuery.Authoring.Selection.Providers;
using NQuery.Text;

namespace NQuery.Authoring.Selection
{
    public static class SelectionExtensions
    {
        public static IEnumerable<ISelectionSpanProvider> GetStandardSelectionSpanProviders()
        {
            return new ISelectionSpanProvider[]
                   {
                       new ArgumentListSelectionSpanProvider(),
                       new CommonTableExpressionColumnNameListSelectionSpanProvider(),
                       new CommonTableExpressionQuerySelectionSpanProvider(),
                       new FromClauseSelectionSpanProvider(),
                       new GroupByClauseSelectionSpanProvider(),
                       new OrderedQuerySelectionSpanProvider(),
                       new SelectClauseSelectionSpanProvider()
                   };
        }

        public static TextSpan ExtendSelection(this SyntaxTree syntaxTree, TextSpan selectedSpan)
        {
            var providers = GetStandardSelectionSpanProviders();
            return syntaxTree.ExtendSelection(selectedSpan, providers);
        }

        public static TextSpan ExtendSelection(this SyntaxTree syntaxTree, TextSpan selectedSpan, IEnumerable<ISelectionSpanProvider> providers)
        {
            var token = syntaxTree.Root.FindToken(selectedSpan.Start).GetPreviousTokenIfEndOfFile();
            foreach (var span in GetNextSpans(token, providers))
            {
                if (!selectedSpan.Contains(span))
                    return span;
            }

            var node = token.Parent;
            while (node != null)
            {
                foreach (var span in GetNextSpans(node, providers))
                {
                    if (!selectedSpan.Contains(span))
                        return span;
                }

                node = node.Parent;
            }

            return syntaxTree.Root.Span;
        }

        private static IEnumerable<TextSpan> GetNextSpans(SyntaxNodeOrToken nodeOrToken, IEnumerable<ISelectionSpanProvider> providers)
        {
            yield return nodeOrToken.Span;

            var spans = providers.SelectMany(p => p.Provide(nodeOrToken));
            foreach (var span in spans)
                yield return span;
        }
    }
}