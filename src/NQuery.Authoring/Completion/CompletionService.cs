using System.Collections.Immutable;

using NQuery.CodeAnalysis;
using NQuery.CodeAnalysis.Text;

namespace NQuery.Authoring.Completion;

public sealed class CompletionService
{
    private readonly ImmutableArray<ICompletionProvider> _providers;

    internal CompletionService(ImmutableArray<ICompletionProvider> providers)
    {
        _providers = providers;
    }

    public CompletionModel GetModel(DocumentView view, CancellationToken cancellationToken = default)
    {
        ThrowIfNull(view);

        var semanticModel = view.Document.GetSemanticModel(cancellationToken);
        var position = view.Position;

        var syntaxTree = semanticModel.SyntaxTree;
        var token = GetIdentifierOrKeywordAtPosition(syntaxTree.Root, position);
        var applicableSpan = token?.Span ?? new TextSpan(position, 0);

        var items = _providers.SelectMany(p => p.GetItems(semanticModel, position));
        var sortedItems = items.OrderBy(c => c.DisplayText).ToImmutableArray();

        return new CompletionModel(semanticModel, applicableSpan, sortedItems);
    }

    private static SyntaxToken? GetIdentifierOrKeywordAtPosition(SyntaxNode root, int position)
    {
        var syntaxToken = root.FindTokenOnLeft(position);
        return syntaxToken.Kind.IsIdentifierOrKeyword() && syntaxToken.Span.ContainsOrTouches(position)
                   ? syntaxToken
                   : null;
    }
}
