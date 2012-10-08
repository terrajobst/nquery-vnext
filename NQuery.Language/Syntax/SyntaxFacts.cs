using System;
using System.Collections.Generic;
using System.Linq;
using NQuery.Language.Binding;

namespace NQuery.Language
{
    public static class SyntaxFacts
    {
        public static bool IsLiteral(this SyntaxKind kind)
        {
            return kind == SyntaxKind.DateLiteralToken ||
                   kind == SyntaxKind.NumericLiteralToken ||
                   kind == SyntaxKind.StringLiteralToken;
        }

        public static bool IsComment(this SyntaxKind kind)
        {
            return kind == SyntaxKind.SingleLineCommentTrivia ||
                   kind == SyntaxKind.MultiLineCommentTrivia;
        }

        public static IEnumerable<SyntaxKind> GetKeywordKinds()
        {
            return GetReservedKeywordKinds().Concat(GetContextualKeywordKinds());
        }

        public static IEnumerable<SyntaxKind> GetReservedKeywordKinds()
        {
            yield return SyntaxKind.AndKeyword;
            yield return SyntaxKind.OrKeyword;
            yield return SyntaxKind.IsKeyword;
            yield return SyntaxKind.NullKeyword;
            yield return SyntaxKind.NotKeyword;
            yield return SyntaxKind.LikeKeyword;
            yield return SyntaxKind.SoundslikeKeyword;
            yield return SyntaxKind.SimilarKeyword;
            yield return SyntaxKind.BetweenKeyword;
            yield return SyntaxKind.InKeyword;
            yield return SyntaxKind.CastKeyword;
            yield return SyntaxKind.AsKeyword;
            yield return SyntaxKind.CoalesceKeyword;
            yield return SyntaxKind.NullIfKeyword;
            yield return SyntaxKind.CaseKeyword;
            yield return SyntaxKind.WhenKeyword;
            yield return SyntaxKind.ThenKeyword;
            yield return SyntaxKind.ElseKeyword;
            yield return SyntaxKind.EndKeyword;
            yield return SyntaxKind.TrueKeyword;
            yield return SyntaxKind.FalseKeyword;
            yield return SyntaxKind.ToKeyword;
        }

        public static IEnumerable<SyntaxKind> GetContextualKeywordKinds()
        {
            yield return SyntaxKind.SelectKeyword;
            yield return SyntaxKind.TopKeyword;
            yield return SyntaxKind.DistinctKeyword;
            yield return SyntaxKind.FromKeyword;
            yield return SyntaxKind.WhereKeyword;
            yield return SyntaxKind.GroupKeyword;
            yield return SyntaxKind.ByKeyword;
            yield return SyntaxKind.HavingKeyword;
            yield return SyntaxKind.OrderKeyword;
            yield return SyntaxKind.AscKeyword;
            yield return SyntaxKind.DescKeyword;
            yield return SyntaxKind.UnionKeyword;
            yield return SyntaxKind.AllKeyword;
            yield return SyntaxKind.IntersectKeyword;
            yield return SyntaxKind.ExceptKeyword;
            yield return SyntaxKind.ExistsKeyword;
            yield return SyntaxKind.AnyKeyword;
            yield return SyntaxKind.SomeKeyword;
            yield return SyntaxKind.JoinKeyword;
            yield return SyntaxKind.InnerKeyword;
            yield return SyntaxKind.CrossKeyword;
            yield return SyntaxKind.LeftKeyword;
            yield return SyntaxKind.RightKeyword;
            yield return SyntaxKind.OuterKeyword;
            yield return SyntaxKind.FullKeyword;
            yield return SyntaxKind.OnKeyword;
            yield return SyntaxKind.WithKeyword;
            yield return SyntaxKind.TiesKeyword;
        }

        public static string GetText(this SyntaxKind kind)
        {
            switch (kind)
            {
                case SyntaxKind.BitwiseNotToken:
                    return "~";
                case SyntaxKind.AmpersandToken:
                    return "&";
                case SyntaxKind.BarToken:
                    return "|";
                case SyntaxKind.CaretToken:
                    return "^";
                case SyntaxKind.LeftParenthesisToken:
                    return "(";
                case SyntaxKind.RightParenthesisToken:
                    return ")";
                case SyntaxKind.PlusToken:
                    return "+";
                case SyntaxKind.MinusToken:
                    return "-";
                case SyntaxKind.AsteriskToken:
                    return "*";
                case SyntaxKind.SlashToken:
                    return "/";
                case SyntaxKind.PercentToken:
                    return "%";
                case SyntaxKind.AsteriskAsteriskToken:
                    return "**";
                case SyntaxKind.CommaToken:
                    return ",";
                case SyntaxKind.DotToken:
                    return ".";
                case SyntaxKind.EqualsToken:
                    return "=";
                case SyntaxKind.ExclamationEqualsToken:
                    return "<>";
                case SyntaxKind.LessToken:
                    return "<";
                case SyntaxKind.LessEqualToken:
                    return "<=";
                case SyntaxKind.GreaterToken:
                    return ">";
                case SyntaxKind.GreaterEqualToken:
                    return ">=";
                case SyntaxKind.ExclamationLessToken:
                    return "!<";
                case SyntaxKind.ExclamationGreaterToken:
                    return "!>";
                case SyntaxKind.LessLessToken:
                    return "<<";
                case SyntaxKind.GreaterGreaterToken:
                    return ">>";
                case SyntaxKind.AndKeyword:
                    return "AND";
                case SyntaxKind.OrKeyword:
                    return "OR";
                case SyntaxKind.IsKeyword:
                    return "IS";
                case SyntaxKind.NullKeyword:
                    return "NULL";
                case SyntaxKind.NotKeyword:
                    return "NOT";
                case SyntaxKind.LikeKeyword:
                    return "LIKE";
                case SyntaxKind.SoundslikeKeyword:
                    return "SOUNDSLIKE";
                case SyntaxKind.SimilarKeyword:
                    return "SIMILAR";
                case SyntaxKind.ToKeyword:
                    return "TO";
                case SyntaxKind.BetweenKeyword:
                    return "BETWEEN";
                case SyntaxKind.InKeyword:
                    return "IN";
                case SyntaxKind.CastKeyword:
                    return "CAST";
                case SyntaxKind.AsKeyword:
                    return "AS";
                case SyntaxKind.CoalesceKeyword:
                    return "COALESCE";
                case SyntaxKind.NullIfKeyword:
                    return "NULLIF";
                case SyntaxKind.CaseKeyword:
                    return "CASE";
                case SyntaxKind.WhenKeyword:
                    return "WHEN";
                case SyntaxKind.ThenKeyword:
                    return "THEN";
                case SyntaxKind.ElseKeyword:
                    return "ELSE";
                case SyntaxKind.EndKeyword:
                    return "END";
                case SyntaxKind.TrueKeyword:
                    return "TRUE";
                case SyntaxKind.FalseKeyword:
                    return "FALSE";
                case SyntaxKind.SelectKeyword:
                    return "SELECT";
                case SyntaxKind.TopKeyword:
                    return "TOP";
                case SyntaxKind.DistinctKeyword:
                    return "DISTINCT";
                case SyntaxKind.FromKeyword:
                    return "FROM";
                case SyntaxKind.WhereKeyword:
                    return "WHERE";
                case SyntaxKind.GroupKeyword:
                    return "GROUP";
                case SyntaxKind.ByKeyword:
                    return "BY";
                case SyntaxKind.HavingKeyword:
                    return "HAVING";
                case SyntaxKind.OrderKeyword:
                    return "ORDER";
                case SyntaxKind.AscKeyword:
                    return "ASC";
                case SyntaxKind.DescKeyword:
                    return "DESC";
                case SyntaxKind.UnionKeyword:
                    return "UNION";
                case SyntaxKind.AllKeyword:
                    return "ALL";
                case SyntaxKind.IntersectKeyword:
                    return "INTERSECT";
                case SyntaxKind.ExceptKeyword:
                    return "EXCEPT";
                case SyntaxKind.ExistsKeyword:
                    return "EXISTS";
                case SyntaxKind.AnyKeyword:
                    return "ANY";
                case SyntaxKind.SomeKeyword:
                    return "SOME";
                case SyntaxKind.JoinKeyword:
                    return "JOIN";
                case SyntaxKind.InnerKeyword:
                    return "INNER";
                case SyntaxKind.CrossKeyword:
                    return "CROSS";
                case SyntaxKind.LeftKeyword:
                    return "LEFT";
                case SyntaxKind.RightKeyword:
                    return "RIGHT";
                case SyntaxKind.OuterKeyword:
                    return "OUTER";
                case SyntaxKind.FullKeyword:
                    return "FULL";
                case SyntaxKind.OnKeyword:
                    return "ON";
                case SyntaxKind.WithKeyword:
                    return "WITH";
                case SyntaxKind.TiesKeyword:
                    return "TIES";
                default:
                    return String.Empty;
            }
        }

        public static string GetDisplayText(this SyntaxKind kind)
        {
            switch (kind)
            {
                case SyntaxKind.EndOfFileToken:
                    return "<end-of-file>";

                case SyntaxKind.IdentifierToken:
                    return "<identifier>";

                case SyntaxKind.NumericLiteralToken:
                    return "<numeric-literal>";

                case SyntaxKind.StringLiteralToken:
                    return "<string-literal>";

                case SyntaxKind.DateLiteralToken:
                    return "<date-literal>";

                default:
                    return GetText(kind);
            }
        }

        public static string GetDisplayText(this SyntaxToken token)
        {
            var result = token.Text;
            return !string.IsNullOrEmpty(result) ? result : token.Kind.GetDisplayText();
        }

        public static bool IsIdentifierOrKeyword(this SyntaxKind kind)
        {
            return kind == SyntaxKind.IdentifierToken || kind.IsKeyword();
        }

        public static bool IsKeyword(this SyntaxKind kind)
        {
            switch (kind)
            {
                    // Keywords

                case SyntaxKind.AndKeyword:
                case SyntaxKind.OrKeyword:
                case SyntaxKind.IsKeyword:
                case SyntaxKind.NullKeyword:
                case SyntaxKind.NotKeyword:
                case SyntaxKind.LikeKeyword:
                case SyntaxKind.SoundslikeKeyword:
                case SyntaxKind.SimilarKeyword:
                case SyntaxKind.BetweenKeyword:
                case SyntaxKind.InKeyword:
                case SyntaxKind.CastKeyword:
                case SyntaxKind.AsKeyword:
                case SyntaxKind.CoalesceKeyword:
                case SyntaxKind.NullIfKeyword:
                case SyntaxKind.CaseKeyword:
                case SyntaxKind.WhenKeyword:
                case SyntaxKind.ThenKeyword:
                case SyntaxKind.ElseKeyword:
                case SyntaxKind.EndKeyword:
                case SyntaxKind.TrueKeyword:
                case SyntaxKind.FalseKeyword:
                case SyntaxKind.ToKeyword:

                    // Contextual keywords

                case SyntaxKind.SelectKeyword:
                case SyntaxKind.TopKeyword:
                case SyntaxKind.DistinctKeyword:
                case SyntaxKind.FromKeyword:
                case SyntaxKind.WhereKeyword:
                case SyntaxKind.GroupKeyword:
                case SyntaxKind.ByKeyword:
                case SyntaxKind.HavingKeyword:
                case SyntaxKind.OrderKeyword:
                case SyntaxKind.AscKeyword:
                case SyntaxKind.DescKeyword:
                case SyntaxKind.UnionKeyword:
                case SyntaxKind.AllKeyword:
                case SyntaxKind.IntersectKeyword:
                case SyntaxKind.ExceptKeyword:
                case SyntaxKind.ExistsKeyword:
                case SyntaxKind.AnyKeyword:
                case SyntaxKind.SomeKeyword:
                case SyntaxKind.JoinKeyword:
                case SyntaxKind.InnerKeyword:
                case SyntaxKind.CrossKeyword:
                case SyntaxKind.LeftKeyword:
                case SyntaxKind.RightKeyword:
                case SyntaxKind.OuterKeyword:
                case SyntaxKind.FullKeyword:
                case SyntaxKind.OnKeyword:
                case SyntaxKind.WithKeyword:
                case SyntaxKind.TiesKeyword:
                    return true;

                default:
                    return false;
            }
        }

        public static bool IsPunctuation(this SyntaxKind kind)
        {
            switch (kind)
            {
                case SyntaxKind.BitwiseNotToken:
                case SyntaxKind.AmpersandToken:
                case SyntaxKind.BarToken:
                case SyntaxKind.CaretToken:
                case SyntaxKind.LeftParenthesisToken:
                case SyntaxKind.RightParenthesisToken:
                case SyntaxKind.PlusToken:
                case SyntaxKind.MinusToken:
                case SyntaxKind.AsteriskToken:
                case SyntaxKind.SlashToken:
                case SyntaxKind.PercentToken:
                case SyntaxKind.AsteriskAsteriskToken:
                case SyntaxKind.CommaToken:
                case SyntaxKind.DotToken:
                case SyntaxKind.EqualsToken:
                case SyntaxKind.ExclamationEqualsToken:
                case SyntaxKind.LessToken:
                case SyntaxKind.LessEqualToken:
                case SyntaxKind.GreaterToken:
                case SyntaxKind.GreaterEqualToken:
                case SyntaxKind.ExclamationLessToken:
                case SyntaxKind.ExclamationGreaterToken:
                case SyntaxKind.GreaterGreaterToken:
                case SyntaxKind.LessLessToken:
                    return true;
                default:
                    return false;
            }
        }

        public static SyntaxKind GetKeywordKind(string text)
        {
            if (text == null)
                throw new ArgumentNullException("text");

            switch (text.ToUpper())
            {
                case "AND":
                    return SyntaxKind.AndKeyword;

                case "AS":
                    return SyntaxKind.AsKeyword;

                case "BETWEEN":
                    return SyntaxKind.BetweenKeyword;

                case "CASE":
                    return SyntaxKind.CaseKeyword;

                case "CAST":
                    return SyntaxKind.CastKeyword;

                case "COALESCE":
                    return SyntaxKind.CoalesceKeyword;

                case "ELSE":
                    return SyntaxKind.ElseKeyword;

                case "END":
                    return SyntaxKind.EndKeyword;

                case "FALSE":
                    return SyntaxKind.FalseKeyword;

                case "IN":
                    return SyntaxKind.InKeyword;

                case "IS":
                    return SyntaxKind.IsKeyword;

                case "LIKE":
                    return SyntaxKind.LikeKeyword;

                case "NOT":
                    return SyntaxKind.NotKeyword;

                case "NULL":
                    return SyntaxKind.NullKeyword;

                case "NULLIF":
                    return SyntaxKind.NullIfKeyword;

                case "OR":
                    return SyntaxKind.OrKeyword;

                case "ON":
                    return SyntaxKind.OnKeyword;

                case "SIMILAR":
                    return SyntaxKind.SimilarKeyword;

                case "SOUNDSLIKE":
                    return SyntaxKind.SoundslikeKeyword;

                case "THEN":
                    return SyntaxKind.ThenKeyword;

                case "TO":
                    return SyntaxKind.ToKeyword;

                case "TRUE":
                    return SyntaxKind.TrueKeyword;

                case "WHEN":
                    return SyntaxKind.WhenKeyword;

                case "ALL":
                    return SyntaxKind.AllKeyword;

                case "ANY":
                    return SyntaxKind.AnyKeyword;

                case "ASC":
                    return SyntaxKind.AscKeyword;

                case "BY":
                    return SyntaxKind.ByKeyword;

                case "CROSS":
                    return SyntaxKind.CrossKeyword;

                case "DESC":
                    return SyntaxKind.DescKeyword;

                case "DISTINCT":
                    return SyntaxKind.DistinctKeyword;

                case "EXCEPT":
                    return SyntaxKind.ExceptKeyword;

                case "EXISTS":
                    return SyntaxKind.ExistsKeyword;

                case "FROM":
                    return SyntaxKind.FromKeyword;

                case "GROUP":
                    return SyntaxKind.GroupKeyword;

                case "HAVING":
                    return SyntaxKind.HavingKeyword;

                case "INNER":
                    return SyntaxKind.InnerKeyword;

                case "INTERSECT":
                    return SyntaxKind.IntersectKeyword;

                case "JOIN":
                    return SyntaxKind.JoinKeyword;

                case "ORDER":
                    return SyntaxKind.OrderKeyword;

                case "SELECT":
                    return SyntaxKind.SelectKeyword;

                case "SOME":
                    return SyntaxKind.SomeKeyword;

                case "TOP":
                    return SyntaxKind.TopKeyword;

                case "UNION":
                    return SyntaxKind.UnionKeyword;

                case "WHERE":
                    return SyntaxKind.WhereKeyword;

                case "OUTER":
                    return SyntaxKind.OuterKeyword;

                case "FULL":
                    return SyntaxKind.FullKeyword;

                case "WITH":
                    return SyntaxKind.WithKeyword;

                case "TIES":
                    return SyntaxKind.TiesKeyword;

                default:
                    return SyntaxKind.IdentifierToken;
            }
        }

        public static SyntaxKind GetContextualKeywordKind(string text)
        {
            if (text == null)
                throw new ArgumentNullException("text");

            switch (text.ToUpper())
            {
                case "LEFT":
                    return SyntaxKind.LeftKeyword;

                case "RIGHT":
                    return SyntaxKind.RightKeyword;

                default:
                    return SyntaxKind.IdentifierToken;
            }
        }

        public static bool IsQuotedIdentifier(this SyntaxToken token)
        {
            return token.Kind == SyntaxKind.IdentifierToken &&
                   token.Text.Length > 0 &&
                   token.Text[0] == '"';
        }

        public static bool IsParenthesizedIdentifier(this SyntaxToken token)
        {
            return token.Kind == SyntaxKind.IdentifierToken &&
                   token.Text.Length > 0 &&
                   token.Text[0] == '[';
        }

        public static bool Matches(this SyntaxToken token, string text)
        {
            var comparison = token.IsQuotedIdentifier()
                                 ? StringComparison.Ordinal
                                 : StringComparison.OrdinalIgnoreCase;
            return String.Equals(token.ValueText, text, comparison);
        }

        public static bool IsTerminated(this SyntaxToken token)
        {
            switch (token.Kind)
            {
                case SyntaxKind.IdentifierToken:
                    if (token.IsQuotedIdentifier())
                        return EndsWithUnescapedChar(token.Text, '"');
                    if (token.IsParenthesizedIdentifier())
                        return EndsWithUnescapedChar(token.Text, ']');
                    return true;
                case SyntaxKind.StringLiteralToken:
                    return EndsWithUnescapedChar(token.Text, '\'');
                case SyntaxKind.DateLiteralToken:
                    return token.Text.Length >= 2 && token.Text.EndsWith("#");
                default:
                    return true;
            }
        }

        public static bool IsTerminated(this SyntaxTrivia trivia)
        {
            switch (trivia.Kind)
            {
                case SyntaxKind.MultiLineCommentTrivia:
                    return trivia.Text.Length >= 4 && trivia.Text.EndsWith("*/");
                default:
                    return true;
            }
        }

        private static bool EndsWithUnescapedChar(string text, char c)
        {
            var numberOfChars = 0;
            var i = text.Length - 1;
            while (i > 0 && text[i] == c)
            {
                numberOfChars++;
                i--;
            }
            return numberOfChars % 2 != 0;
        }

        internal static SyntaxKind GetUnaryOperatorExpression(SyntaxKind token)
        {
            switch (token)
            {
                case SyntaxKind.BitwiseNotToken:
                    return SyntaxKind.ComplementExpression;

                case SyntaxKind.PlusToken:
                    return SyntaxKind.IdentityExpression;

                case SyntaxKind.MinusToken:
                    return SyntaxKind.NegationExpression;

                case SyntaxKind.NotKeyword:
                    return SyntaxKind.LogicalNotExpression;

                default:
                    return SyntaxKind.BadToken;
            }
        }

        internal static int GetUnaryOperatorPrecedence(SyntaxKind unaryExpression)
        {
            switch (unaryExpression)
            {
                case SyntaxKind.ComplementExpression:
                    return 9;

                case SyntaxKind.IdentityExpression:
                    return 9;

                case SyntaxKind.NegationExpression:
                    return 9;

                case SyntaxKind.LogicalNotExpression:
                    return 4;

                default:
                    return 0;
            }
        }

        internal static SyntaxKind GetBinaryOperatorExpression(SyntaxKind token)
        {
            switch (token)
            {
                case SyntaxKind.AmpersandToken:
                    return SyntaxKind.BitwiseAndExpression;

                case SyntaxKind.BarToken:
                    return SyntaxKind.BitwiseOrExpression;

                case SyntaxKind.CaretToken:
                    return SyntaxKind.ExclusiveOrExpression;

                case SyntaxKind.PlusToken:
                    return SyntaxKind.AddExpression;

                case SyntaxKind.MinusToken:
                    return SyntaxKind.SubExpression;

                case SyntaxKind.AsteriskToken:
                    return SyntaxKind.MultiplyExpression;

                case SyntaxKind.SlashToken:
                    return SyntaxKind.DivideExpression;

                case SyntaxKind.PercentToken:
                    return SyntaxKind.ModuloExpression;

                case SyntaxKind.AsteriskAsteriskToken:
                    return SyntaxKind.PowerExpression;

                case SyntaxKind.EqualsToken:
                    return SyntaxKind.EqualExpression;

                case SyntaxKind.LessGreaterToken:
                case SyntaxKind.ExclamationEqualsToken:
                    return SyntaxKind.NotEqualExpression;

                case SyntaxKind.LessToken:
                    return SyntaxKind.LessExpression;

                case SyntaxKind.LessEqualToken:
                    return SyntaxKind.LessOrEqualExpression;

                case SyntaxKind.GreaterToken:
                    return SyntaxKind.GreaterExpression;

                case SyntaxKind.GreaterEqualToken:
                    return SyntaxKind.GreaterOrEqualExpression;

                case SyntaxKind.ExclamationLessToken:
                    return SyntaxKind.NotLessExpression;

                case SyntaxKind.ExclamationGreaterToken:
                    return SyntaxKind.NotGreaterExpression;

                case SyntaxKind.LessLessToken:
                    return SyntaxKind.LeftShiftExpression;

                case SyntaxKind.GreaterGreaterToken:
                    return SyntaxKind.RightShiftExpression;

                case SyntaxKind.AndKeyword:
                    return SyntaxKind.LogicalAndExpression;

                case SyntaxKind.OrKeyword:
                    return SyntaxKind.LogicalOrExpression;

                case SyntaxKind.LikeKeyword:
                    return SyntaxKind.LikeExpression;

                case SyntaxKind.SoundslikeKeyword:
                    return SyntaxKind.SoundslikeExpression;

                case SyntaxKind.SimilarKeyword:
                    return SyntaxKind.SimilarToExpression;

                case SyntaxKind.InKeyword:
                    return SyntaxKind.InExpression;

                default:
                    return SyntaxKind.BadToken;
            }
        }

        internal static int GetBinaryOperatorPrecedence(SyntaxKind binaryExpression)
        {
            switch (binaryExpression)
            {
                case SyntaxKind.BitwiseAndExpression:
                    return 5;

                case SyntaxKind.BitwiseOrExpression:
                    return 5;

                case SyntaxKind.ExclusiveOrExpression:
                    return 5;

                case SyntaxKind.AddExpression:
                    return 7;

                case SyntaxKind.SubExpression:
                    return 7;

                case SyntaxKind.MultiplyExpression:
                    return 8;

                case SyntaxKind.DivideExpression:
                    return 8;

                case SyntaxKind.ModuloExpression:
                    return 8;

                case SyntaxKind.PowerExpression:
                    return 10;

                case SyntaxKind.EqualExpression:
                    return 6;

                case SyntaxKind.NotEqualExpression:
                    return 6;

                case SyntaxKind.LessExpression:
                    return 6;

                case SyntaxKind.LessOrEqualExpression:
                    return 6;

                case SyntaxKind.GreaterExpression:
                    return 6;

                case SyntaxKind.GreaterOrEqualExpression:
                    return 6;

                case SyntaxKind.NotLessExpression:
                    return 6;

                case SyntaxKind.NotGreaterExpression:
                    return 6;

                case SyntaxKind.LeftShiftExpression:
                    return 5;

                case SyntaxKind.RightShiftExpression:
                    return 5;

                case SyntaxKind.LogicalAndExpression:
                    return 2;

                case SyntaxKind.LogicalOrExpression:
                    return 1;

                case SyntaxKind.LikeExpression:
                    return 3;

                case SyntaxKind.SoundslikeExpression:
                    return 3;

                case SyntaxKind.SimilarToExpression:
                    return 3;

                case SyntaxKind.InExpression:
                    return 3;

                default:
                    return 0;
            }
        }

        internal static int GetTernaryOperatorPrecedence(SyntaxKind ternaryExpression)
        {
            switch (ternaryExpression)
            {
                case SyntaxKind.BetweenExpression:
                    return 4;

                default:
                    return 0;
            }
        }

        internal static UnaryOperatorKind ToUnaryOperatorKind(this SyntaxKind expressionKind)
        {
            switch (expressionKind)
            {
                case SyntaxKind.ComplementExpression:
                    return UnaryOperatorKind.Complement;
                case SyntaxKind.IdentityExpression:
                    return UnaryOperatorKind.Identity;
                case SyntaxKind.NegationExpression:
                    return UnaryOperatorKind.Negation;
                case SyntaxKind.LogicalNotExpression:
                    return UnaryOperatorKind.LogicalNot;
                default:
                    throw new ArgumentOutOfRangeException("expressionKind");
            }
        }

        internal static BinaryOperatorKind ToBinaryOperatorKind(this SyntaxKind expressionKind)
        {
            switch (expressionKind)
            {
                case SyntaxKind.BitwiseAndExpression:
                    return BinaryOperatorKind.BitAnd;
                case SyntaxKind.BitwiseOrExpression:
                    return BinaryOperatorKind.BitOr;
                case SyntaxKind.ExclusiveOrExpression:
                    return BinaryOperatorKind.BitXor;
                case SyntaxKind.AddExpression:
                    return BinaryOperatorKind.Add;
                case SyntaxKind.SubExpression:
                    return BinaryOperatorKind.Sub;
                case SyntaxKind.MultiplyExpression:
                    return BinaryOperatorKind.Multiply;
                case SyntaxKind.DivideExpression:
                    return BinaryOperatorKind.Divide;
                case SyntaxKind.ModuloExpression:
                    return BinaryOperatorKind.Modulus;
                case SyntaxKind.PowerExpression:
                    return BinaryOperatorKind.Power;
                case SyntaxKind.EqualExpression:
                    return BinaryOperatorKind.Equal;
                case SyntaxKind.NotEqualExpression:
                    return BinaryOperatorKind.NotEqual;
                case SyntaxKind.LessExpression:
                    return BinaryOperatorKind.Less;
                case SyntaxKind.NotGreaterExpression:
                case SyntaxKind.LessOrEqualExpression:
                    return BinaryOperatorKind.LessOrEqual;
                case SyntaxKind.GreaterExpression:
                    return BinaryOperatorKind.Greater;
                case SyntaxKind.NotLessExpression:
                case SyntaxKind.GreaterOrEqualExpression:
                    return BinaryOperatorKind.GreaterOrEqual;
                case SyntaxKind.LeftShiftExpression:
                    return BinaryOperatorKind.LeftShift;
                case SyntaxKind.RightShiftExpression:
                    return BinaryOperatorKind.RightShift;
                case SyntaxKind.LogicalAndExpression:
                    return BinaryOperatorKind.LogicalAnd;
                case SyntaxKind.LogicalOrExpression:
                    return BinaryOperatorKind.LogicalOr;
                case SyntaxKind.LikeExpression:
                    return BinaryOperatorKind.Like;
                case SyntaxKind.SoundslikeExpression:
                    return BinaryOperatorKind.Soundslike;
                case SyntaxKind.SimilarToExpression:
                    return BinaryOperatorKind.SimilarTo;
                default:
                    throw new ArgumentOutOfRangeException("expressionKind");
            }
        }

        internal static string ToDisplayName(this UnaryOperatorKind operatorKind)
        {
            switch (operatorKind)
            {
                case UnaryOperatorKind.Identity:
                    return SyntaxKind.PlusToken.GetText();
                case UnaryOperatorKind.Negation:
                    return SyntaxKind.MinusToken.GetText();
                case UnaryOperatorKind.Complement:
                    return SyntaxKind.BitwiseNotToken.GetText();
                case UnaryOperatorKind.LogicalNot:
                    return SyntaxKind.NotKeyword.GetText();
                default:
                    throw new ArgumentOutOfRangeException("operatorKind");
            }
        }

        internal static string ToDisplayName(this BinaryOperatorKind operatorKind)
        {
            switch (operatorKind)
            {
                case BinaryOperatorKind.Power:
                    return SyntaxKind.AsteriskAsteriskToken.GetText();
                
                case BinaryOperatorKind.Multiply:
                    return SyntaxKind.AsteriskToken.GetText();
                
                case BinaryOperatorKind.Divide:
                    return SyntaxKind.SlashToken.GetText();
                
                case BinaryOperatorKind.Modulus:
                    return SyntaxKind.PercentToken.GetText();
                
                case BinaryOperatorKind.Add:
                    return SyntaxKind.PlusToken.GetText();
                
                case BinaryOperatorKind.Sub:
                    return SyntaxKind.MinusToken.GetText();
                
                case BinaryOperatorKind.Equal:
                    return SyntaxKind.EqualsToken.GetText();
                
                case BinaryOperatorKind.NotEqual:
                    return SyntaxKind.ExclamationEqualsToken.GetText();
                
                case BinaryOperatorKind.Less:
                    return SyntaxKind.LessToken.GetText();
                
                case BinaryOperatorKind.LessOrEqual:
                    return SyntaxKind.LessEqualToken.GetText();
                
                case BinaryOperatorKind.Greater:
                    return SyntaxKind.GreaterToken.GetText();
                
                case BinaryOperatorKind.GreaterOrEqual:
                    return SyntaxKind.GreaterEqualToken.GetText();
                
                case BinaryOperatorKind.BitXor:
                    return SyntaxKind.CaretToken.GetText();
                
                case BinaryOperatorKind.BitAnd:
                    return SyntaxKind.AmpersandToken.GetText();
                
                case BinaryOperatorKind.BitOr:
                    return SyntaxKind.BarToken.GetText();
                
                case BinaryOperatorKind.LeftShift:
                    return SyntaxKind.LessLessToken.GetText();
                
                case BinaryOperatorKind.RightShift:
                    return SyntaxKind.GreaterGreaterToken.GetText();
                
                case BinaryOperatorKind.Like:
                    return SyntaxKind.LikeKeyword.GetText();
                
                case BinaryOperatorKind.SimilarTo:
                    return SyntaxKind.SimilarKeyword.GetText() + " " + SyntaxKind.ToKeyword.GetText();
                
                case BinaryOperatorKind.Soundslike:
                    return SyntaxKind.SoundslikeKeyword.GetText();
                
                case BinaryOperatorKind.LogicalAnd:
                    return SyntaxKind.AndKeyword.GetText();
                
                case BinaryOperatorKind.LogicalOr:
                    return SyntaxKind.OrKeyword.GetText();
                
                default:
                    throw new ArgumentOutOfRangeException("operatorKind");
            }
        }

        public static bool IsValidAllAnyOperator(this SyntaxKind binaryExpression)
        {
            return binaryExpression == SyntaxKind.EqualExpression ||
                   binaryExpression == SyntaxKind.NotEqualExpression ||
                   binaryExpression == SyntaxKind.NotLessExpression ||
                   binaryExpression == SyntaxKind.NotGreaterExpression ||
                   binaryExpression == SyntaxKind.LessExpression ||
                   binaryExpression == SyntaxKind.LessOrEqualExpression ||
                   binaryExpression == SyntaxKind.GreaterExpression ||
                   binaryExpression == SyntaxKind.GreaterOrEqualExpression;
        }

        public static bool CanHaveLeadingNot(this SyntaxKind syntaxKind)
        {
            return syntaxKind == SyntaxKind.BetweenKeyword ||
                   syntaxKind == SyntaxKind.InKeyword ||
                   syntaxKind == SyntaxKind.LikeKeyword ||
                   syntaxKind == SyntaxKind.SimilarKeyword ||
                   syntaxKind == SyntaxKind.SoundslikeKeyword;
        }

        public static IEnumerable<string> GetTypeNames()
        {
            return new[]
                       {
                           "BOOL",
                           "BOOLEAN",
                           "BYTE",
                           "SBYTE",
                           "CHAR",
                           "SHORT",
                           "INT16",
                           "USHORT",
                           "UINT16",
                           "INT",
                           "INT32",
                           "UINT",
                           "UINT32",
                           "LONG",
                           "INT64",
                           "ULONG",
                           "UINT64",
                           "FLOAT",
                           "SINGLE",
                           "DOUBLE",
                           "DECIMAL",
                           "STRING",
                           "OBJECT",
                       };
        }
    }
}