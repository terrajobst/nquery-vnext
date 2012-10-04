using System.Linq;

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
    }
}