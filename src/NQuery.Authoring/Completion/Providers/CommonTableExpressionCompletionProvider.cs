using NQuery.CodeAnalysis;
using NQuery.CodeAnalysis.Syntax;

namespace NQuery.Authoring.Completion.Providers;

internal sealed class CommonTableExpressionCompletionProvider : CompletionProvider<CommonTableExpressionSyntax>
{
    protected override IEnumerable<CompletionItem> GetItems(SemanticModel semanticModel, int position, CommonTableExpressionSyntax node)
    {
        if (node.RecursiveKeyword is not null ||
            node.IdentifierToken.IsMissing ||
            !node.IdentifierToken.FullSpan.ContainsOrTouches(position))
        {
            yield break;
        }

        var identifier = node.IdentifierToken.Text;
        yield return new CompletionItem(identifier, identifier, null, true);
    }
}
