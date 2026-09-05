using NQuery.CodeAnalysis;
using NQuery.CodeAnalysis.Syntax;

namespace NQuery.Authoring.Formatting;

// The spacing every gap gets unless the layout walker overrode it. A function of the two tokens
// plus, in the few cases where the kind alone lies, the node the second token belongs to: a left
// parenthesis is glued to what precedes it when it opens an argument list and separated when it
// opens a subquery, and a minus is glued to its operand when it's unary.
internal static class SpacingRules
{
    public static Gap GetGap(SyntaxToken previous, SyntaxToken current)
    {
        var kind = GetGapKind(previous, current);

        // Two tokens the rules want adjacent can still lex as something else together -- "- -1"
        // collapsing into a comment being the memorable one. The spacing rules aren't the right
        // place to enumerate those, so this asks the question directly.
        if (kind == GapKind.None && WouldMerge(previous.Text, current.Text))
            return new Gap(GapKind.Space);

        return new Gap(kind);
    }

    private static GapKind GetGapKind(SyntaxToken previous, SyntaxToken current)
    {
        if (previous.Kind == SyntaxKind.DotToken || current.Kind == SyntaxKind.DotToken)
            return GapKind.None;

        if (current.Kind == SyntaxKind.CommaToken)
            return GapKind.None;

        // The '@' of a variable reference.
        if (previous.Kind == SyntaxKind.AtToken)
            return GapKind.None;

        if (previous.Kind == SyntaxKind.LeftParenthesisToken)
            return GapKind.None;

        if (current.Kind == SyntaxKind.RightParenthesisToken)
            return GapKind.None;

        if (current.Kind == SyntaxKind.LeftParenthesisToken)
            return OpensAnArgumentList(current) ? GapKind.None : GapKind.Space;

        if (IsPunctuationUnaryOperator(previous))
            return GapKind.None;

        return GapKind.Space;
    }

    // True for the parentheses of something invoked by name, where SQL is written without a space.
    // A parenthesis introducing a subquery, a grouping, or a CTE's column list keeps its space.
    private static bool OpensAnArgumentList(SyntaxToken leftParenthesis)
    {
        switch (leftParenthesis.Parent)
        {
            case ArgumentListSyntax argumentList:
                // IN's operand list reads as a list of values rather than as an invocation.
                return argumentList.Parent is not InExpressionSyntax;
            case CountAllExpressionSyntax:
            case NullIfExpressionSyntax:
            case CastExpressionSyntax:
                return true;
            default:
                return false;
        }
    }

    // NOT is a unary operator too, but it's a word and keeps its space.
    private static bool IsPunctuationUnaryOperator(SyntaxToken token)
    {
        return token.Parent is UnaryExpressionSyntax unary &&
               ReferenceEquals(unary.UnaryOperatorToken, token) &&
               token.Kind.IsPunctuation();
    }

    private static bool WouldMerge(string left, string right)
    {
        if (left.Length == 0 || right.Length == 0)
            return false;

        var l = left[left.Length - 1];
        var r = right[0];

        return (l, r) switch
        {
            ('-', '-') => true,
            ('/', '*') => true,
            ('*', '/') => true,
            ('/', '/') => true,
            _ => char.IsDigit(l) && r == '.' || l == '.' && char.IsDigit(r)
        };
    }
}
