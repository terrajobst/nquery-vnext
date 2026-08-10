using System.Collections.Immutable;
using System.Text;

using NQuery.Authoring.SignatureHelp.Providers;
using NQuery.CodeAnalysis;
using NQuery.CodeAnalysis.Symbols;
using NQuery.CodeAnalysis.Syntax;
using NQuery.CodeAnalysis.Text;

namespace NQuery.Authoring.SignatureHelp;

public static class SignatureHelpExtensions
{
    private static ImmutableArray<ISignatureHelpModelProvider> StandardSignatureHelpModelProviders { get; } =
    [
        new CastSignatureHelpModelProvider(),
        new CoalesceSignatureHelpModelProvider(),
        new CountAllSignatureHelpModelProvider(),
        new FunctionSignatureHelpModelProvider(),
        new MethodSignatureHelpModelProvider(),
        new NullIfSignatureHelpModelProvider()
    ];

    extension(AuthoringServicesBuilder builder)
    {
        public AuthoringServicesBuilder AddSignatureHelpService()
        {
            ThrowIfNull(builder);

            return builder.AddService(s => new SignatureHelpService(s.GetProviders<ISignatureHelpModelProvider>()));
        }

        public AuthoringServicesBuilder AddSignatureHelpModelProvider(ISignatureHelpModelProvider provider)
        {
            ThrowIfNull(builder);

            return builder.AddProvider<ISignatureHelpModelProvider>(provider);
        }

        public AuthoringServicesBuilder AddStandardSignatureHelpModelProviders()
        {
            ThrowIfNull(builder);

            return builder.AddProviders(StandardSignatureHelpModelProviders);
        }
    }

    extension(ArgumentListSyntax argumentList)
    {
        internal int GetParameterIndex(int position)
        {
            var separators = argumentList.Arguments.GetSeparators();
            return separators.TakeWhile(s => !s.IsMissing && s.Span.End <= position).Count();
        }
    }

    extension(SyntaxNode node)
    {
        internal bool IsBetweenParentheses(int position)
        {
            var argumentList = node.ChildNodes().SingleOrDefault(n => n.Kind == SyntaxKind.ArgumentList);
            if (argumentList is not null)
                return argumentList.IsBetweenParentheses(position);

            var leftParenthesisToken = node.GetSingleChildToken(SyntaxKind.LeftParenthesisToken);
            var rightParenthesisToken = node.GetSingleChildToken(SyntaxKind.RightParenthesisToken);
            var start = leftParenthesisToken.IsMissing ? leftParenthesisToken.Span.Start : leftParenthesisToken.Span.End;
            var end = rightParenthesisToken.IsMissing ? node.FullSpan.End : rightParenthesisToken.Span.Start;
            return start <= position && position <= end;
        }

        private SyntaxToken GetSingleChildToken(SyntaxKind tokenKind)
        {
            return node.ChildTokens().Single(nt => nt.Kind == tokenKind);
        }
    }

    private static bool IsCommaToken(SymbolMarkupToken token)
    {
        return token.Kind == SymbolMarkupKind.Punctuation && token.Text == @",";
    }

    private static bool IsAsKeyword(SymbolMarkupToken token)
    {
        return token.Kind == SymbolMarkupKind.Keyword && SyntaxFacts.GetKeywordKind(token.Text) == SyntaxKind.AsKeyword;
    }

    extension(SymbolMarkup markup)
    {
        private SignatureItem ToSignatureItem(Func<SymbolMarkupToken, bool> separatorPredicate)
        {
            var sb = new StringBuilder();
            var parameterStart = 0;
            var nextNonWhitespaceStartsParameter = false;
            var parameterSpans = new List<TextSpan>();
            var parameterNames = new List<string>();

            foreach (var node in markup.Tokens)
            {
                var isParameterName = node.Kind == SymbolMarkupKind.ParameterName;
                var isWhitespace = node.Kind == SymbolMarkupKind.Whitespace;
                var isLeftParenthesis = node.Kind == SymbolMarkupKind.Punctuation && node.Text == @"(";
                var isRightParenthesis = node.Kind == SymbolMarkupKind.Punctuation && node.Text == @")";
                var isSeparator = separatorPredicate(node);

                if (isParameterName)
                {
                    parameterNames.Add(node.Text);
                }

                if (isLeftParenthesis)
                {
                    nextNonWhitespaceStartsParameter = true;
                }
                else if (isSeparator || isRightParenthesis)
                {
                    var end = sb.Length;
                    var span = TextSpan.FromBounds(parameterStart, end);
                    parameterSpans.Add(span);
                    nextNonWhitespaceStartsParameter = true;
                }
                else if (!isWhitespace && nextNonWhitespaceStartsParameter)
                {
                    parameterStart = sb.Length;
                    nextNonWhitespaceStartsParameter = false;
                }

                sb.Append(node.Text);
            }

            var parameters = parameterSpans.Zip(parameterNames, (s, n) => new ParameterItem(n, s)).ToImmutableArray();
            var content = sb.ToString();
            return new SignatureItem(content, parameters);
        }
    }

    extension(IEnumerable<Symbol> symbols)
    {
        internal IEnumerable<SignatureItem> ToSignatureItems()
        {
            return symbols.Select(ToSignatureItem);
        }
    }

    extension(Symbol symbol)
    {
        internal SignatureItem ToSignatureItem()
        {
            return SymbolMarkup.ForSymbol(symbol).ToSignatureItem(IsCommaToken);
        }
    }

    internal static SignatureItem GetCastSignatureItem()
    {
        return SymbolMarkup.ForCastSymbol().ToSignatureItem(IsAsKeyword);
    }

    internal static SignatureItem GetCoalesceSignatureItem()
    {
        return SymbolMarkup.ForCoalesceSymbol().ToSignatureItem(IsCommaToken);
    }

    internal static SignatureItem GetNullIfSignatureItem()
    {
        return SymbolMarkup.ForNullIfSymbol().ToSignatureItem(IsCommaToken);
    }
}
