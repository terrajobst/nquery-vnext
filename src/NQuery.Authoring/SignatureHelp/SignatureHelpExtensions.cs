using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NQuery.Symbols;
using NQuery.Syntax;

namespace NQuery.Authoring.SignatureHelp
{
    internal static class SignatureHelpExtensions
    {
        public static int GetParameterIndex(this ArgumentListSyntax argumentList, int position)
        {
            var separators = argumentList.Arguments.GetSeparators();
            return separators.TakeWhile(s => !s.IsMissing && s.Span.End <= position).Count();
        }

        public static bool IsBetweenParentheses(this ArgumentListSyntax argumentList, int position)
        {
            return IsBetweenParentheses(argumentList.FullSpan, argumentList.LeftParenthesis, argumentList.RightParenthesis, position);
        }

        public static bool IsBetweenParentheses(this CastExpressionSyntax expression, int position)
        {
            return IsBetweenParentheses(expression.FullSpan, expression.LeftParenthesisToken, expression.RightParenthesisToken, position);
        }

        public static bool IsBetweenParentheses(this CountAllExpressionSyntax expression, int position)
        {
            return IsBetweenParentheses(expression.FullSpan, expression.LeftParenthesis, expression.RightParenthesis, position);
        }

        public static bool IsBetweenParentheses(this NullIfExpressionSyntax expression, int position)
        {
            return IsBetweenParentheses(expression.FullSpan, expression.LeftParenthesisToken, expression.RightParenthesisToken, position);
        }

        public static bool IsBetweenParentheses(this CoalesceExpressionSyntax expression, int position)
        {
            return expression.ArgumentList.IsBetweenParentheses(position);
        }

        public static bool IsBetweenParentheses(this MethodInvocationExpressionSyntax expression, int position)
        {
            return expression.ArgumentList.IsBetweenParentheses(position);
        }

        public static bool IsBetweenParentheses(this FunctionInvocationExpressionSyntax expression, int position)
        {
            return expression.ArgumentList.IsBetweenParentheses(position);
        }

        private static bool IsBetweenParentheses(TextSpan parentFullSpan, SyntaxToken leftParenthesis, SyntaxToken rightParenthesis, int position)
        {
            var start = leftParenthesis.IsMissing ? leftParenthesis.Span.Start : leftParenthesis.Span.End;
            var end = rightParenthesis.IsMissing ? parentFullSpan.End : rightParenthesis.Span.Start;
            return start <= position && position <= end;
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

            var parameters = parameterSpans.Zip(parameterNames, (s, n) => new ParameterItem(n, string.Empty, s)).ToArray();
            var content = sb.ToString();
            return new SignatureItem(content, string.Empty, parameters);
        }

        public static IEnumerable<SignatureItem> ToSignatureItems(this IEnumerable<Symbol> symbols)
        {
            return symbols.Select(ToSignatureItem);
        }

        public static SignatureItem ToSignatureItem(this Symbol symbol)
        {
            return SymbolMarkup.ForSymbol(symbol).ToSignatureItem(IsCommaToken);
        }

        public static SignatureItem GetCastSignatureItem()
        {
            return SymbolMarkup.ForCastSymbol().ToSignatureItem(IsAsKeyword);
        }

        public static SignatureItem GetCoalesceSignatureItem()
        {
            return SymbolMarkup.ForCoalesceSymbol().ToSignatureItem(IsCommaToken);
        }

        public static SignatureItem GetNullIfSignatureItem()
        {
            return SymbolMarkup.ForNullIfSymbol().ToSignatureItem(IsCommaToken);
        }
    }
}