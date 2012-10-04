using System.Collections.Generic;
using System.Linq;
using System.Text;
using NQuery.Language.Symbols;

namespace NQuery.Language.VSEditor.SignatureHelp
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

        public static bool IsBetweenParentheses(this NullIfExpressionSyntax expression, int position)
        {
            return IsBetweenParentheses(expression.FullSpan, expression.LeftParenthesisToken, expression.RightParenthesisToken, position);
        }

        private static bool IsBetweenParentheses(TextSpan parentFullSpan, SyntaxToken leftParenthesis, SyntaxToken rightParenthesis, int position)
        {
            var start = leftParenthesis.IsMissing ? leftParenthesis.Span.Start : leftParenthesis.Span.End;
            var end = rightParenthesis.IsMissing ? parentFullSpan.End : rightParenthesis.Span.Start;
            return start <= position && position <= end;
        }

        public static IEnumerable<SignatureItem> ToSignatureItems(this IEnumerable<MethodSymbol> symbols)
        {
            return symbols.Select(ToSignatureItem);
        }

        public static IEnumerable<SignatureItem> ToSignatureItems(this IEnumerable<FunctionSymbol> symbols)
        {
            return symbols.Select(ToSignatureItem);
        }

        public static IEnumerable<SignatureItem> ToSignatureItems(this IEnumerable<AggregateSymbol> symbols)
        {
            return symbols.Select(ToSignatureItem);
        }

        public static SignatureItem ToSignatureItem(this MethodSymbol symbol)
        {
            var parameters = new List<ParameterItem>();
            var sb = new StringBuilder();

            sb.Append(symbol.Name);
            sb.Append("(");

            var isFirst = true;
            foreach (var parameter in symbol.Parameters)
            {
                if (isFirst)
                    isFirst = false;
                else
                    sb.Append(", ");

                var start = sb.Length;

                sb.Append(parameter.Name);
                sb.Append(" AS ");
                sb.Append(parameter.Type.Name.ToUpper());

                var end = sb.Length;
                var span = TextSpan.FromBounds(start, end);
                parameters.Add(new ParameterItem(parameter.Name, "Docs for " + parameter.Name, span));
            }

            sb.Append(")");
            sb.Append(" AS ");
            sb.Append(symbol.Type.Name.ToUpper());

            var content = sb.ToString();

            return new SignatureItem(content, "Docs for " + symbol.Name, parameters);
        }

        public static SignatureItem ToSignatureItem(this FunctionSymbol symbol)
        {
            var parameters = new List<ParameterItem>();
            var sb = new StringBuilder();

            sb.Append(symbol.Name);
            sb.Append("(");

            var isFirst = true;
            foreach (var parameter in symbol.Parameters)
            {
                if (isFirst)
                    isFirst = false;
                else
                    sb.Append(", ");

                var start = sb.Length;

                sb.Append(parameter.Name);
                sb.Append(" AS ");
                sb.Append(parameter.Type.Name.ToUpper());

                var end = sb.Length;
                var span = TextSpan.FromBounds(start, end);
                parameters.Add(new ParameterItem(parameter.Name, "Docs for " + parameter.Name, span));
            }

            sb.Append(")");
            sb.Append(" AS ");
            sb.Append(symbol.Type.Name.ToUpper());

            var content = sb.ToString();

            return new SignatureItem(content, "Docs for " + symbol.Name, parameters);
        }

        public static SignatureItem ToSignatureItem(this AggregateSymbol symbol)
        {
            var parameters = new List<ParameterItem>();
            var sb = new StringBuilder();

            sb.Append("AGGREGATE ");
            sb.Append(symbol.Name);
            sb.Append("(");

            var p1Start = sb.Length;
            sb.Append("expression");
            var p1End = sb.Length;

            sb.Append(")");

            parameters.Add(new ParameterItem("expression", null, TextSpan.FromBounds(p1Start, p1End)));

            var content = sb.ToString();

            return new SignatureItem(content, "Docs for " + symbol.Name, parameters);
        }

        public static SignatureItem GetCastSignatureItem()
        {
            var parameters = new List<ParameterItem>();
            var sb = new StringBuilder();

            sb.Append("CAST(");

            var p1Start = sb.Length;
            sb.Append("expression");
            var p1End = sb.Length;

            sb.Append(" AS ");

            var p2Start = sb.Length;
            sb.Append("dataType");
            var p2End = sb.Length;

            sb.Append(")");

            parameters.Add(new ParameterItem("expression", "expression of any type", TextSpan.FromBounds(p1Start, p1End)));
            parameters.Add(new ParameterItem("dataType", "the type the epression is converted to", TextSpan.FromBounds(p2Start, p2End)));

            var content = sb.ToString();

            return new SignatureItem(content, "Converts an expression of one data type to another", parameters);
        }

        public static SignatureItem GetCoalesceSignatureItem()
        {
            var parameters = new List<ParameterItem>();
            var sb = new StringBuilder();

            sb.Append("COALESCE(");

            var p1Start = sb.Length;
            sb.Append("expression1");
            var p1End = sb.Length;

            sb.Append(", ");

            var p2Start = sb.Length;
            sb.Append("expression2");
            var p2End = sb.Length;

            sb.Append(" ");

            var pNStart = sb.Length;
            sb.Append("[, ...]");
            var pNEnd = sb.Length;

            sb.Append(")");

            parameters.Add(new ParameterItem("expression1", "expression of any type", TextSpan.FromBounds(p1Start, p1End)));
            parameters.Add(new ParameterItem("expression2", "expression of any type", TextSpan.FromBounds(p2Start, p2End)));
            parameters.Add(new ParameterItem("expressionN", "expression of any type", TextSpan.FromBounds(pNStart, pNEnd)));

            var content = sb.ToString();

            return new SignatureItem(content, "Returns the first nonnull expression among its arguments.", parameters);
        }

        public static SignatureItem GetNullIfSignatureItem()
        {
            var parameters = new List<ParameterItem>();
            var sb = new StringBuilder();

            sb.Append("NULLIF(");

            var p1Start = sb.Length;
            sb.Append("expression1");
            var p1End = sb.Length;

            sb.Append(", ");

            var p2Start = sb.Length;
            sb.Append("expression2");
            var p2End = sb.Length;

            sb.Append(")");

            parameters.Add(new ParameterItem("expression1", "expression of any type", TextSpan.FromBounds(p1Start, p1End)));
            parameters.Add(new ParameterItem("expression2", "expression of any type", TextSpan.FromBounds(p2Start, p2End)));

            var content = sb.ToString();

            return new SignatureItem(content, "Returns a null value if the two specified expressions are equal.", parameters);
        }
    }
}