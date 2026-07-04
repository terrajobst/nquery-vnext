using NQuery.CodeAnalysis;
using NQuery.CodeAnalysis.Syntax;

namespace NQuery.Authoring.Completion.Providers;

internal sealed class AliasCompletionProvider : CompletionProvider<AliasSyntax>
{
    protected override IEnumerable<CompletionItem> GetItems(SemanticModel semanticModel, int position, AliasSyntax node)
    {
        if (node.AsKeyword is not null ||
            node.IdentifierToken.IsMissing ||
            !node.Span.ContainsOrTouches(position))
        {
            yield break;
        }

        var identifier = node.IdentifierToken.Text;
        yield return new CompletionItem(identifier, identifier, null, true);
    }
}
