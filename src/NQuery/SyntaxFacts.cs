using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NQuery.Binding;
using NQuery.Syntax;
using NQuery.Text;

namespace NQuery
{
    public static class SyntaxFacts
    {
        public static SyntaxToken ParseToken(string text)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            var sourceText = SourceText.From(text);
            var lexer = new Lexer(null, sourceText);
            return lexer.Lex();
        }

        public static ExpressionSyntax ParseExpression(string text)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            var tree = SyntaxTree.ParseExpression(text);
            return (ExpressionSyntax)tree.Root.Root;
        }

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
            yield return SyntaxKind.AllKeyword;
            yield return SyntaxKind.AndKeyword;
            yield return SyntaxKind.AnyKeyword;
            yield return SyntaxKind.AscKeyword;
            yield return SyntaxKind.AsKeyword;
            yield return SyntaxKind.BetweenKeyword;
            yield return SyntaxKind.ByKeyword;
            yield return SyntaxKind.CaseKeyword;
            yield return SyntaxKind.CastKeyword;
            yield return SyntaxKind.CoalesceKeyword;
            yield return SyntaxKind.CrossKeyword;
            yield return SyntaxKind.DescKeyword;
            yield return SyntaxKind.DistinctKeyword;
            yield return SyntaxKind.ElseKeyword;
            yield return SyntaxKind.EndKeyword;
            yield return SyntaxKind.ExceptKeyword;
            yield return SyntaxKind.ExistsKeyword;
            yield return SyntaxKind.FalseKeyword;
            yield return SyntaxKind.FromKeyword;
            yield return SyntaxKind.FullKeyword;
            yield return SyntaxKind.GroupKeyword;
            yield return SyntaxKind.HavingKeyword;
            yield return SyntaxKind.InKeyword;
            yield return SyntaxKind.InnerKeyword;
            yield return SyntaxKind.IntersectKeyword;
            yield return SyntaxKind.IsKeyword;
            yield return SyntaxKind.JoinKeyword;
            yield return SyntaxKind.LikeKeyword;
            yield return SyntaxKind.NotKeyword;
            yield return SyntaxKind.NullIfKeyword;
            yield return SyntaxKind.NullKeyword;
            yield return SyntaxKind.OnKeyword;
            yield return SyntaxKind.OrderKeyword;
            yield return SyntaxKind.OrKeyword;
            yield return SyntaxKind.OuterKeyword;
            yield return SyntaxKind.RecursiveKeyword;
            yield return SyntaxKind.SelectKeyword;
            yield return SyntaxKind.SimilarKeyword;
            yield return SyntaxKind.SomeKeyword;
            yield return SyntaxKind.SoundsKeyword;
            yield return SyntaxKind.ThenKeyword;
            yield return SyntaxKind.TiesKeyword;
            yield return SyntaxKind.ToKeyword;
            yield return SyntaxKind.TopKeyword;
            yield return SyntaxKind.TrueKeyword;
            yield return SyntaxKind.UnionKeyword;
            yield return SyntaxKind.WhenKeyword;
            yield return SyntaxKind.WhereKeyword;
            yield return SyntaxKind.WithKeyword;
        }

        public static IEnumerable<SyntaxKind> GetContextualKeywordKinds()
        {
            yield return SyntaxKind.LeftKeyword;
            yield return SyntaxKind.RightKeyword;
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
                case SyntaxKind.AtToken:
                    return "@";
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
                    return "!=";
                case SyntaxKind.LessGreaterToken:
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
                case SyntaxKind.SoundsKeyword:
                    return "SOUNDS";
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
                case SyntaxKind.RecursiveKeyword:
                    return "RECURSIVE";
                default:
                    return string.Empty;
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
            if (token == null)
                throw new ArgumentNullException(nameof(token));

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
                case SyntaxKind.SoundsKeyword:
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
                case SyntaxKind.RecursiveKeyword:
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
                case SyntaxKind.LessGreaterToken:
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

        public static string GetValidIdentifier(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            return IsValidIdentifier(name)
                       ? name
                       : GetParenthesizedIdentifier(name);
        }

        public static bool IsValidIdentifier(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            if (name.Length == 0)
                return false;

            var firstChar = name.First();
            if (!char.IsLetter(firstChar) && firstChar != '_')
                return false;

            if (!name.Skip(1).All(c => char.IsLetterOrDigit(c) || c == '_' || c == '$'))
                return false;

            return GetKeywordKind(name) == SyntaxKind.IdentifierToken;
        }

        public static string GetParenthesizedIdentifier(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            var sb = new StringBuilder();

            sb.Append('[');

            foreach (var c in name)
            {
                if (c == ']')
                    sb.Append(']');
                sb.Append(c);
            }

            sb.Append(']');

            return sb.ToString();
        }

        public static string GetQuotedIdentifier(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            var sb = new StringBuilder();

            sb.Append('"');

            foreach (var c in name)
            {
                if (c == '"')
                    sb.Append('"');
                sb.Append(c);
            }

            sb.Append('"');

            return sb.ToString();
        }

        public static SyntaxKind GetKeywordKind(string text)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));

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

                case "SOUNDS":
                    return SyntaxKind.SoundsKeyword;

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

                case "RECURSIVE":
                    return SyntaxKind.RecursiveKeyword;

                default:
                    return SyntaxKind.IdentifierToken;
            }
        }

        public static SyntaxKind GetContextualKeywordKind(string text)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));

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
            if (token == null)
                throw new ArgumentNullException(nameof(token));

            return token.Kind == SyntaxKind.IdentifierToken &&
                   token.Text.Length > 0 &&
                   token.Text[0] == '"';
        }

        public static bool IsParenthesizedIdentifier(this SyntaxToken token)
        {
            if (token == null)
                throw new ArgumentNullException(nameof(token));

            return token.Kind == SyntaxKind.IdentifierToken &&
                   token.Text.Length > 0 &&
                   token.Text[0] == '[';
        }

        public static bool Matches(this SyntaxToken token, string text)
        {
            if (token == null)
                throw new ArgumentNullException(nameof(token));

            if (text == null)
                throw new ArgumentNullException(nameof(text));

            var comparison = token.IsQuotedIdentifier()
                                 ? StringComparison.Ordinal
                                 : StringComparison.OrdinalIgnoreCase;
            return string.Equals(token.ValueText, text, comparison);
        }

        public static bool IsTerminated(this SyntaxToken token)
        {
            if (token == null)
                throw new ArgumentNullException(nameof(token));

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
            if (trivia == null)
                throw new ArgumentNullException(nameof(trivia));

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

        public static IEnumerable<SyntaxKind> GetUnaryExpressionTokenKinds()
        {
            yield return SyntaxKind.BitwiseNotToken;
            yield return SyntaxKind.PlusToken;
            yield return SyntaxKind.MinusToken;
            yield return SyntaxKind.NotKeyword;
        }

        internal static SyntaxKind GetUnaryOperatorExpression(SyntaxKind tokenKind)
        {
            switch (tokenKind)
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

        public static IEnumerable<SyntaxKind> GetBinaryExpressionTokenKinds()
        {
            yield return SyntaxKind.AmpersandToken;
            yield return SyntaxKind.BarToken;
            yield return SyntaxKind.CaretToken;
            yield return SyntaxKind.PlusToken;
            yield return SyntaxKind.MinusToken;
            yield return SyntaxKind.AsteriskToken;
            yield return SyntaxKind.SlashToken;
            yield return SyntaxKind.PercentToken;
            yield return SyntaxKind.AsteriskAsteriskToken;
            yield return SyntaxKind.EqualsToken;
            yield return SyntaxKind.LessGreaterToken;
            yield return SyntaxKind.ExclamationEqualsToken;
            yield return SyntaxKind.LessToken;
            yield return SyntaxKind.LessEqualToken;
            yield return SyntaxKind.GreaterToken;
            yield return SyntaxKind.GreaterEqualToken;
            yield return SyntaxKind.ExclamationLessToken;
            yield return SyntaxKind.ExclamationGreaterToken;
            yield return SyntaxKind.LessLessToken;
            yield return SyntaxKind.GreaterGreaterToken;
            yield return SyntaxKind.AndKeyword;
            yield return SyntaxKind.OrKeyword;
        }

        public static bool CanSwapBinaryExpressionTokenKind(SyntaxKind tokenKind)
        {
            switch (tokenKind)
            {
                case SyntaxKind.AmpersandToken:
                case SyntaxKind.BarToken:
                case SyntaxKind.CaretToken:
                case SyntaxKind.PlusToken:
                case SyntaxKind.MinusToken:
                case SyntaxKind.AsteriskToken:
                case SyntaxKind.EqualsToken:
                case SyntaxKind.LessGreaterToken:
                case SyntaxKind.ExclamationEqualsToken:
                case SyntaxKind.AndKeyword:
                case SyntaxKind.OrKeyword:
                case SyntaxKind.LessToken:
                case SyntaxKind.LessEqualToken:
                case SyntaxKind.GreaterToken:
                case SyntaxKind.GreaterEqualToken:
                case SyntaxKind.ExclamationLessToken:
                case SyntaxKind.ExclamationGreaterToken:
                    return true;
                default:
                    return false;
            }
        }

        public static SyntaxKind SwapBinaryExpressionTokenKind(SyntaxKind tokenKind)
        {
            switch (tokenKind)
            {
                case SyntaxKind.AmpersandToken:
                case SyntaxKind.BarToken:
                case SyntaxKind.CaretToken:
                case SyntaxKind.PlusToken:
                case SyntaxKind.MinusToken:
                case SyntaxKind.AsteriskToken:
                case SyntaxKind.EqualsToken:
                case SyntaxKind.LessGreaterToken:
                case SyntaxKind.ExclamationEqualsToken:
                case SyntaxKind.AndKeyword:
                case SyntaxKind.OrKeyword:
                    return tokenKind;
                case SyntaxKind.LessToken:
                    return SyntaxKind.GreaterToken;
                case SyntaxKind.LessEqualToken:
                    return SyntaxKind.GreaterEqualToken;
                case SyntaxKind.GreaterToken:
                    return SyntaxKind.LessToken;
                case SyntaxKind.GreaterEqualToken:
                    return SyntaxKind.LessEqualToken;
                case SyntaxKind.ExclamationLessToken:
                    return SyntaxKind.ExclamationGreaterToken;
                case SyntaxKind.ExclamationGreaterToken:
                    return SyntaxKind.ExclamationLessToken;
                default:
                    throw ExceptionBuilder.UnexpectedValue(tokenKind);
            }
        }

        internal static SyntaxKind GetBinaryOperatorExpression(SyntaxKind tokenKind)
        {
            switch (tokenKind)
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

                case SyntaxKind.SoundsKeyword:
                    return SyntaxKind.SoundsLikeExpression;

                case SyntaxKind.SimilarKeyword:
                    return SyntaxKind.SimilarToExpression;

                case SyntaxKind.InKeyword:
                    return SyntaxKind.InExpression;

                default:
                    return SyntaxKind.BadToken;
            }
        }

        internal static int GetBinaryOperatorPrecedence(SyntaxKind binaryExpressionKind)
        {
            switch (binaryExpressionKind)
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

                case SyntaxKind.SoundsLikeExpression:
                    return 3;

                case SyntaxKind.SimilarToExpression:
                    return 3;

                case SyntaxKind.InExpression:
                    return 3;

                default:
                    return 0;
            }
        }

        internal static int GetTernaryOperatorPrecedence(SyntaxKind ternaryExpressionKind)
        {
            switch (ternaryExpressionKind)
            {
                case SyntaxKind.BetweenExpression:
                    return 7;

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
                    throw ExceptionBuilder.UnexpectedValue(expressionKind);
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
                case SyntaxKind.SoundsLikeExpression:
                    return BinaryOperatorKind.SoundsLike;
                case SyntaxKind.SimilarToExpression:
                    return BinaryOperatorKind.SimilarTo;
                default:
                    throw ExceptionBuilder.UnexpectedValue(expressionKind);
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
                    throw ExceptionBuilder.UnexpectedValue(operatorKind);
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

                case BinaryOperatorKind.SoundsLike:
                    return SyntaxKind.SoundsKeyword.GetText() + " " + SyntaxKind.LikeKeyword.GetText();

                case BinaryOperatorKind.LogicalAnd:
                    return SyntaxKind.AndKeyword.GetText();

                case BinaryOperatorKind.LogicalOr:
                    return SyntaxKind.OrKeyword.GetText();

                default:
                    throw ExceptionBuilder.UnexpectedValue(operatorKind);
            }
        }

        public static bool IsValidAllAnyOperator(this SyntaxKind binaryExpressionKind)
        {
            return binaryExpressionKind == SyntaxKind.EqualExpression ||
                   binaryExpressionKind == SyntaxKind.NotEqualExpression ||
                   binaryExpressionKind == SyntaxKind.NotLessExpression ||
                   binaryExpressionKind == SyntaxKind.NotGreaterExpression ||
                   binaryExpressionKind == SyntaxKind.LessExpression ||
                   binaryExpressionKind == SyntaxKind.LessOrEqualExpression ||
                   binaryExpressionKind == SyntaxKind.GreaterExpression ||
                   binaryExpressionKind == SyntaxKind.GreaterOrEqualExpression;
        }

        public static bool CanHaveLeadingNot(this SyntaxKind syntaxKind)
        {
            return syntaxKind == SyntaxKind.BetweenKeyword ||
                   syntaxKind == SyntaxKind.InKeyword ||
                   syntaxKind == SyntaxKind.LikeKeyword ||
                   syntaxKind == SyntaxKind.SimilarKeyword ||
                   syntaxKind == SyntaxKind.SoundsKeyword;
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
                           "OBJECT"
                       };
        }

        public static bool CanStartExpression(SyntaxKind kind)
        {
            var unaryOperator = GetUnaryOperatorExpression(kind);
            if (unaryOperator != SyntaxKind.BadToken)
                return true;

            switch (kind)
            {
                case SyntaxKind.NullKeyword:
                case SyntaxKind.TrueKeyword:
                case SyntaxKind.FalseKeyword:
                case SyntaxKind.DateLiteralToken:
                case SyntaxKind.NumericLiteralToken:
                case SyntaxKind.StringLiteralToken:
                case SyntaxKind.ExistsKeyword:
                case SyntaxKind.AtToken:
                case SyntaxKind.CastKeyword:
                case SyntaxKind.CaseKeyword:
                case SyntaxKind.CoalesceKeyword:
                case SyntaxKind.NullIfKeyword:
                case SyntaxKind.IdentifierToken:
                case SyntaxKind.LeftParenthesisToken:
                    return true;
                default:
                    return false;
            }
        }

        public static bool CanFollowSelectColumn(SyntaxKind kind)
        {
            return CanStartExpression(kind) ||
                   kind == SyntaxKind.CommaToken ||
                   CanFollowSelectColumnList(kind);
        }

        public static bool CanFollowSelectColumnList(SyntaxKind kind)
        {
            return CanStartQueryClause(kind) ||
                   kind == SyntaxKind.UnionKeyword ||
                   kind == SyntaxKind.ExceptKeyword ||
                   kind == SyntaxKind.RightParenthesisToken ||
                   kind == SyntaxKind.EndOfFileToken;
        }

        private static bool CanStartQueryClause(SyntaxKind kind)
        {
            return kind == SyntaxKind.FromKeyword ||
                   kind == SyntaxKind.WhereKeyword ||
                   kind == SyntaxKind.GroupKeyword ||
                   kind == SyntaxKind.HavingClause ||
                   kind == SyntaxKind.OrderKeyword;
        }

        public static bool CanFollowArgument(SyntaxKind kind)
        {
            return CanStartExpression(kind) ||
                   kind == SyntaxKind.CommaToken ||
                   kind == SyntaxKind.RightParenthesisToken ||
                   CanStartQueryClause(kind) ||
                   kind == SyntaxKind.EndOfFileToken;
        }

        public static bool ParenthesisIsRedundant(ParenthesizedExpressionSyntax expression)
        {
            if (expression == null)
                throw new ArgumentNullException(nameof(expression));

            var parentExpression = expression.Parent as ExpressionSyntax;
            var childExpression = expression.Expression;

            // If the parent isn't an expression, the parentheses are definitely
            // redundant.
            if (parentExpression == null)
                return true;

            var parentPrecedence = GetPrecedence(parentExpression);
            var childPrecedence = GetPrecedence(childExpression);
            var parentRequiresPrimary = parentExpression is MethodInvocationExpressionSyntax ||
                                        parentExpression is PropertyAccessExpressionSyntax ||
                                        parentExpression is IsNullExpressionSyntax;

            if (childPrecedence == 0)
            {
                // If the child expression doesn't have predence,
                // then parentheses are redundant.
                return true;
            }

            if (parentPrecedence == 0)
            {
                // If the parent doesn't have a precedence, it depends on whether it
                // requires a primary expression.
                return !parentRequiresPrimary;
            }

            var parentBinary = parentExpression as BinaryExpressionSyntax;
            var childIsOnRight = parentBinary != null && parentBinary.Right == expression;

            // NOTE: All expressions are left associative.

            if (parentPrecedence == childPrecedence)
                return !childIsOnRight;

            return parentPrecedence < childPrecedence;
        }

        private static int GetPrecedence(ExpressionSyntax expression)
        {
            if (expression is BinaryExpressionSyntax)
                return GetBinaryOperatorPrecedence(expression.Kind);

            if (expression is UnaryExpressionSyntax)
                return GetUnaryOperatorPrecedence(expression.Kind);

            if (expression is BetweenExpressionSyntax)
                return GetTernaryOperatorPrecedence(expression.Kind);

            return 0;
        }
    }
}