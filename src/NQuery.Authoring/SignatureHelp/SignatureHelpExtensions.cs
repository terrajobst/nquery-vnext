using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NQuery.Symbols;
using NQuery.Syntax;
using NQuery.Text;

namespace NQuery.Authoring.SignatureHelp
{
    public static class SignatureHelpExtensions
    {
        public static IEnumerable<ISignatureHelpModelProvider> GetStandardSignatureHelpModelProviders()
        {
            return new ISignatureHelpModelProvider[]
                   {
                       new CastSignatureHelpModelProvider(),
                       new CoalesceSignatureHelpModelProvider(),
                       new CountAllSignatureHelpModelProvider(),
                       new FunctionSignatureHelpModelProvider(),
                       new MethodSignatureHelpModelProvider(),
                       new NullIfSignatureHelpModelProvider()
                   };
        }

        public static SignatureHelpModel GetSignatureHelpModel(this SemanticModel semanticModel, int position)
        {
            var providers = GetStandardSignatureHelpModelProviders();
            return semanticModel.GetSignatureHelpModel(position, providers);
        }

        public static SignatureHelpModel GetSignatureHelpModel(this SemanticModel semanticModel, int position, IEnumerable<ISignatureHelpModelProvider> providers)
        {
            return (from p in providers
                    let m = p.GetModel(semanticModel, position)
                    where m != null
                    orderby m.ApplicableSpan.Start descending
                    select m).FirstOrDefault();
        }

        internal static int GetParameterIndex(this ArgumentListSyntax argumentList, int position)
        {
            var separators = argumentList.Arguments.GetSeparators();
            return separators.TakeWhile(s => !s.IsMissing && s.Span.End <= position).Count();
        }

        internal static bool IsBetweenParentheses(this SyntaxNode node, int position)
        {
            var argumentList = node.ChildNodes().SingleOrDefault(n => n.Kind == SyntaxKind.ArgumentList);
            if (argumentList != null)
                return argumentList.IsBetweenParentheses(position);

            var leftParenthesisToken = node.GetSingleChildToken(SyntaxKind.LeftParenthesisToken);
            var rightParenthesisToken = node.GetSingleChildToken(SyntaxKind.RightParenthesisToken);
            var start = leftParenthesisToken.IsMissing ? leftParenthesisToken.Span.Start : leftParenthesisToken.Span.End;
            var end = rightParenthesisToken.IsMissing ? node.FullSpan.End : rightParenthesisToken.Span.Start;
            return start <= position && position <= end;
        }

        private static SyntaxToken GetSingleChildToken(this SyntaxNode node, SyntaxKind tokenKind)
        {
            return node.ChildTokens().Single(nt => nt.Kind == tokenKind);
        }

        private static bool IsCommaToken(SymbolMarkupToken token)
        {
            return token.Kind == SymbolMarkupKind.Punctuation && token.Text == ",";
        }

        private static bool IsAsKeyword(SymbolMarkupToken token)
        {
            return token.Kind == SymbolMarkupKind.Keyword && SyntaxFacts.GetKeywordKind(token.Text) == SyntaxKind.AsKeyword;
        }

        private static SignatureItem ToSignatureItem(this SymbolMarkup markup, Func<SymbolMarkupToken, bool> separatorPredicate)
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
                var isLeftParenthesis = node.Kind == SymbolMarkupKind.Punctuation && node.Text == "(";
                var isRightParenthesis = node.Kind == SymbolMarkupKind.Punctuation && node.Text == ")";
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

            var parameters = parameterSpans.Zip(parameterNames, (s, n) => new ParameterItem(n, s)).ToArray();
            var content = sb.ToString();
            return new SignatureItem(content, parameters);
        }

        internal static IEnumerable<SignatureItem> ToSignatureItems(this IEnumerable<Symbol> symbols)
        {
            return symbols.Select(ToSignatureItem);
        }

        internal static SignatureItem ToSignatureItem(this Symbol symbol)
        {
            return SymbolMarkup.ForSymbol(symbol).ToSignatureItem(IsCommaToken);
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
}