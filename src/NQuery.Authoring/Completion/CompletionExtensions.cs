using System.Collections.Immutable;

using NQuery.Authoring.Completion.Providers;
using NQuery.CodeAnalysis;
using NQuery.CodeAnalysis.Text;

namespace NQuery.Authoring.Completion;

public static class CompletionExtensions
{
    public static ImmutableArray<ICompletionProvider> StandardCompletionProviders { get; } =
    [
        new AliasCompletionProvider(),
        new JoinCompletionProvider(),
        new KeywordCompletionProvider(),
        new SymbolCompletionProvider(),
        new TypeCompletionProvider(),
        new CommonTableExpressionCompletionProvider()
    ];

    public static CompletionModel GetCompletionModel(this SemanticModel semanticModel, int position)
    {
        return semanticModel.GetCompletionModel(position, StandardCompletionProviders);
    }

    public static CompletionModel GetCompletionModel(this SemanticModel semanticModel, int position, IEnumerable<ICompletionProvider> providers)
    {
        var syntaxTree = semanticModel.SyntaxTree;
        var token = GetIdentifierOrKeywordAtPosition(syntaxTree.Root, position);
        var applicableSpan = token?.Span ?? new TextSpan(position, 0);

        var items = providers.SelectMany(p => p.GetItems(semanticModel, position));
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
