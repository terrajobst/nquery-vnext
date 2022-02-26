﻿using System.Collections.Immutable;

using NQuery.Authoring.Completion.Providers;
using NQuery.Text;

namespace NQuery.Authoring.Completion
{
    public static class CompletionExtensions
    {
        public static IEnumerable<ICompletionProvider> GetStandardCompletionProviders()
        {
            return new ICompletionProvider[]
                   {
                       new AliasCompletionProvider(),
                       new JoinCompletionProvider(),
                       new KeywordCompletionProvider(),
                       new SymbolCompletionProvider(),
                       new TypeCompletionProvider(),
                       new CommonTableExpressionCompletionProvider()
                   };
        }

        public static CompletionModel GetCompletionModel(this SemanticModel semanticModel, int position)
        {
            var providers = GetStandardCompletionProviders();
            return semanticModel.GetCompletionModel(position, providers);
        }

        public static CompletionModel GetCompletionModel(this SemanticModel semanticModel, int position, IEnumerable<ICompletionProvider> providers)
        {
            var syntaxTree = semanticModel.SyntaxTree;
            var token = GetIdentifierOrKeywordAtPosition(syntaxTree.Root, position);
            var applicableSpan = token == null ? new TextSpan(position, 0) : token.Span;

            var items = providers.SelectMany(p => p.GetItems(semanticModel, position));
            var sortedItems = items.OrderBy(c => c.DisplayText).ToImmutableArray();

            return new CompletionModel(semanticModel, applicableSpan, sortedItems);
        }

        private static SyntaxToken GetIdentifierOrKeywordAtPosition(SyntaxNode root, int position)
        {
            var syntaxToken = root.FindTokenOnLeft(position);
            return syntaxToken.Kind.IsIdentifierOrKeyword() && syntaxToken.Span.ContainsOrTouches(position)
                       ? syntaxToken
                       : null;
        }
    }
}