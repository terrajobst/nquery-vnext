using System.Collections.Immutable;

using NQuery.Authoring.Selection.Providers;
using NQuery.CodeAnalysis;
using NQuery.CodeAnalysis.Text;

namespace NQuery.Authoring.Selection;

public static class SelectionExtensions
{
    public static ImmutableArray<ISelectionSpanProvider> StandardSelectionSpanProviders { get; } =
    [
        new ArgumentListSelectionSpanProvider(),
        new CommonTableExpressionColumnNameListSelectionSpanProvider(),
        new CommonTableExpressionQuerySelectionSpanProvider(),
        new FromClauseSelectionSpanProvider(),
        new GroupByClauseSelectionSpanProvider(),
        new OrderedQuerySelectionSpanProvider(),
        new SelectClauseSelectionSpanProvider()
    ];

    extension(SyntaxTree syntaxTree)
    {
        public TextSpan ExtendSelection(TextSpan selectedSpan)
        {
            return syntaxTree.ExtendSelection(selectedSpan, StandardSelectionSpanProviders);
        }

        public TextSpan ExtendSelection(TextSpan selectedSpan, IEnumerable<ISelectionSpanProvider> providers)
        {
            var token = syntaxTree.Root.FindToken(selectedSpan.Start).GetPreviousTokenIfEndOfFile();
            foreach (var span in GetNextSpans(token, providers))
            {
                if (!selectedSpan.Contains(span))
                    return span;
            }

            var node = token.Parent;
            while (node is not null)
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
    }

    private static IEnumerable<TextSpan> GetNextSpans(SyntaxNodeOrToken nodeOrToken, IEnumerable<ISelectionSpanProvider> providers)
    {
        yield return nodeOrToken.Span;

        var spans = providers.SelectMany(p => p.Provide(nodeOrToken));
        foreach (var span in spans)
            yield return span;
    }
}
