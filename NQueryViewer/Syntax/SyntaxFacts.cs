using System;

namespace NQueryViewer.Syntax
{
    public static class SyntaxFacts
    {
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

                case "LEFT":
                    return SyntaxKind.LeftKeyword;

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

                default:
                    return SyntaxKind.IdentifierToken;
            }
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
                case SyntaxKind.BitwiseAndToken:
                    return SyntaxKind.BitAndExpression;

                case SyntaxKind.BitwiseOrToken:
                    return SyntaxKind.BitOrExpression;

                case SyntaxKind.BitwiseXorToken:
                    return SyntaxKind.BitXorExpression;

                case SyntaxKind.PlusToken:
                    return SyntaxKind.AddExpression;

                case SyntaxKind.MinusToken:
                    return SyntaxKind.SubExpression;

                case SyntaxKind.MultiplyToken:
                    return SyntaxKind.MultiplyExpression;

                case SyntaxKind.DivideToken:
                    return SyntaxKind.DivideExpression;

                case SyntaxKind.ModulusToken:
                    return SyntaxKind.ModulusExpression;

                case SyntaxKind.PowerToken:
                    return SyntaxKind.PowerExpression;

                case SyntaxKind.EqualsToken:
                    return SyntaxKind.EqualExpression;

                case SyntaxKind.UnequalsToken:
                    return SyntaxKind.NotEqualExpression;

                case SyntaxKind.LessToken:
                    return SyntaxKind.LessExpression;

                case SyntaxKind.LessOrEqualToken:
                    return SyntaxKind.LessOrEqualExpression;

                case SyntaxKind.GreaterToken:
                    return SyntaxKind.GreaterExpression;

                case SyntaxKind.GreaterOrEqualToken:
                    return SyntaxKind.GreaterOrEqualExpression;

                case SyntaxKind.NotLessToken:
                    return SyntaxKind.NotLessExpression;

                case SyntaxKind.NotGreaterToken:
                    return SyntaxKind.NotGreaterExpression;

                case SyntaxKind.LeftShiftToken:
                    return SyntaxKind.LeftShiftExpression;

                case SyntaxKind.RightShiftToken:
                    return SyntaxKind.RightShiftExpression;

                case SyntaxKind.AndKeyword:
                    return SyntaxKind.LogicalAndExpression;

                case SyntaxKind.OrKeyword:
                    return SyntaxKind.LogicalOrExpression;

                case SyntaxKind.LikeKeyword:
                    return SyntaxKind.LikeExpression;

                case SyntaxKind.SoundslikeKeyword:
                    return SyntaxKind.SoundexExpression;

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
                case SyntaxKind.BitAndExpression:
                    return 5;

                case SyntaxKind.BitOrExpression:
                    return 5;

                case SyntaxKind.BitXorExpression:
                    return 5;

                case SyntaxKind.AddExpression:
                    return 7;

                case SyntaxKind.SubExpression:
                    return 7;

                case SyntaxKind.MultiplyExpression:
                    return 8;

                case SyntaxKind.DivideExpression:
                    return 8;

                case SyntaxKind.ModulusExpression:
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

                case SyntaxKind.SoundexExpression:
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
    }
}