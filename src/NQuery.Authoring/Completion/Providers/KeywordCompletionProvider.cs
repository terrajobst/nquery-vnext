using System;
using System.Collections.Generic;
using System.Linq;

using NQuery.Syntax;

namespace NQuery.Authoring.Completion.Providers
{
    internal sealed class KeywordCompletionProvider : ICompletionProvider
    {
        public IEnumerable<CompletionItem> GetItems(SemanticModel semanticModel, int position)
        {
            var syntaxTree = semanticModel.Compilation.SyntaxTree;
            var root = syntaxTree.Root;

            // For certain constructs we never want to show a keywod completion.
            if (root.InComment(position) ||
                root.InLiteral(position) ||
                root.InUserGivenName(position) ||
                IsInPropertyAccess(root, position))
            {
                return Enumerable.Empty<CompletionItem>();
            }

            return from k in GetAvailableKeywords(syntaxTree, position)
                   let text = k.GetText()
                   select new CompletionItem(text, text, null, NQueryGlyph.Keyword);
        }

        private static IEnumerable<SyntaxKind> GetAvailableKeywords(SyntaxTree syntaxTree, int position)
        {
            // NOT
            // CAST
            // COALESCE
            // NULLIF
            // TRUE
            // FALSE
            // EXISTS
            // CASE

            if (IsBeforeExpression(syntaxTree, position))
            {
                yield return SyntaxKind.NotKeyword;
                yield return SyntaxKind.CastKeyword;
                yield return SyntaxKind.CoalesceKeyword;
                yield return SyntaxKind.NullIfKeyword;
                yield return SyntaxKind.TrueKeyword;
                yield return SyntaxKind.FalseKeyword;
                yield return SyntaxKind.ExistsKeyword;
                yield return SyntaxKind.CaseKeyword;
            }

            // AND
            // OR
            // IS
            // LIKE
            // SOUNDS
            // SIMILAR
            // BETWEEN
            // IN

            if (IsAfterExpression(syntaxTree, position))
            {
                yield return SyntaxKind.AndKeyword;
                yield return SyntaxKind.OrKeyword;
                yield return SyntaxKind.IsKeyword;
                yield return SyntaxKind.LikeKeyword;
                yield return SyntaxKind.SoundsKeyword;
                yield return SyntaxKind.SimilarKeyword;
                yield return SyntaxKind.BetweenKeyword;
                yield return SyntaxKind.InKeyword;
            }

            // NULL

            if (IsBeforeExpression(syntaxTree, position) ||
                InIsNullAfterIsKeyword(syntaxTree, position))
            {
                yield return SyntaxKind.NullKeyword;
            }

            // LIKE

            if (IsAfterSoundsKeyword(syntaxTree, position))
            {
                yield return SyntaxKind.LikeKeyword;
            }

            // TO

            if (IsAfterSimilarKeyword(syntaxTree, position))
            {
                yield return SyntaxKind.ToKeyword;
            }

            // DISTINCT

            if (IsAfterSelectKeyword(syntaxTree, position))
            {
                yield return SyntaxKind.DistinctKeyword;
            }

            // TOP
            if (IsAfterSelectKeyword(syntaxTree, position) ||
                IsAfterDistinctKeyword(syntaxTree, position) ||
                IsAfterAllKeyword(syntaxTree, position))
            {
                yield return SyntaxKind.TopKeyword;
            }

            // WITH

            if (IsBeforeQuery(syntaxTree, position) ||
                IsInTopClauseAfterValue(syntaxTree, position))
            {
                yield return SyntaxKind.WithKeyword;
            }

            // TIES

            if (IsAfterWithKeyword(syntaxTree, position))
            {
                yield return SyntaxKind.TiesKeyword;
            }

            // AS

            if (IsInCastAfterExpression(syntaxTree, position) ||
                IsInCommonTableExpressionAfterNameOrColumnList(syntaxTree, position))
            {
                yield return SyntaxKind.AsKeyword;
            }

            // WHEN

            if (IsInCaseAndAfterLabelOrAtStart(syntaxTree, position))
            {
                yield return SyntaxKind.WhenKeyword;
            }

            // THEN

            if (IsInCaseLabelAndAfterExpression(syntaxTree, position))
                yield return SyntaxKind.ThenKeyword;

            // ELSE
            // END

            if (IsInCaseAndAfterLabel(syntaxTree, position))
            {
                yield return SyntaxKind.ElseKeyword;
                yield return SyntaxKind.EndKeyword;
            }

            // BY

            if (IsAfterOrderKeyword(syntaxTree, position) ||
                IsAfterGroupKeyword(syntaxTree, position))
            {
                yield return SyntaxKind.ByKeyword;
            }

            // ASC
            // DESC

            if (IsInOrderByAfterExpression(syntaxTree, position))
            {
                yield return SyntaxKind.AscKeyword;
                yield return SyntaxKind.DescKeyword;
            }

            // SELECT

            if (IsBeforeSelectQuery(syntaxTree, position))
            {
                yield return SyntaxKind.SelectKeyword;
            }

            // FROM

            if (InSelectQueryAndNoFromClause(syntaxTree, position))
            {
                yield return SyntaxKind.FromKeyword;
            }

            // WHERE

            if (InSelectQueryAndNoWhereClause(syntaxTree, position))
            {
                yield return SyntaxKind.WhereKeyword;
            }

            // GROUP

            if (InSelectQueryAndNoGroupByClause(syntaxTree, position))
            {
                yield return SyntaxKind.GroupKeyword;
            }

            // HAVING

            if (InSelectQueryAndNoHavingClause(syntaxTree, position))
            {
                yield return SyntaxKind.HavingKeyword;
            }

            // ORDER
            // UNION
            // INTERSECT
            // EXCEPT

            if (IsAfterQuery(syntaxTree, position))
            {
                yield return SyntaxKind.OrderKeyword;
                yield return SyntaxKind.UnionKeyword;
                yield return SyntaxKind.IntersectKeyword;
                yield return SyntaxKind.ExceptKeyword;
            }

            // All

            if (IsAfterSelectKeyword(syntaxTree, position) ||
                IsAfterUnionKeyword(syntaxTree, position) ||
                IsAfterAllAnyOperator(syntaxTree, position))
            {
                yield return SyntaxKind.AllKeyword;
            }

            // ANY
            // SOME

            if (IsAfterAllAnyOperator(syntaxTree, position))
            {
                yield return SyntaxKind.AnyKeyword;
                yield return SyntaxKind.SomeKeyword;
            }

            // JOIN

            if (IsAfterTableReference(syntaxTree, position) ||
                IsAfterInnerKeyword(syntaxTree, position) ||
                IsAfterOuterKeyword(syntaxTree, position) ||
                IsAfterLeftKeyword(syntaxTree, position) ||
                IsAfterRightKeyword(syntaxTree, position) ||
                IsAfterFullKeyword(syntaxTree, position))
            {
                yield return SyntaxKind.JoinKeyword;
            }

            // INNER
            // CROSS
            // LEFT
            // RIGHT
            // FULL

            if (IsAfterTableReference(syntaxTree, position))
            {
                yield return SyntaxKind.InnerKeyword;
                yield return SyntaxKind.CrossKeyword;
                yield return SyntaxKind.LeftKeyword;
                yield return SyntaxKind.RightKeyword;
                yield return SyntaxKind.FullKeyword;
            }

            // LEFT
            // RIGHT
            // FULL

            if (IsAfterLeftKeyword(syntaxTree, position) ||
                IsAfterRightKeyword(syntaxTree, position) ||
                IsAfterFullKeyword(syntaxTree, position))
            {
                yield return SyntaxKind.OuterKeyword;
            }

            // ON

            if (IsInConditionedJoinedTableReferenceAfterLeft(syntaxTree, position))
            {
                yield return SyntaxKind.OnKeyword;
            }
        }

        private static bool IsAfterQuery(SyntaxTree syntaxTree, int position)
        {
            if (IsAfterNode<QuerySyntax>(syntaxTree, position))
                return true;

            // FROM test xx|

            var token = syntaxTree.Root.FindTokenOnLeft(position);
            if (token == null)
                return false;

            var selectQuery = token.Parent.AncestorsAndSelf().OfType<SelectQuerySyntax>().FirstOrDefault();
            return selectQuery != null && token == selectQuery.LastToken();
        }

        private static bool IsInPropertyAccess(SyntaxNode root, int position)
        {
            var token = root.FindTokenOnLeft(position);
            if (token == null || !token.Span.ContainsOrTouches(position))
                return false;

            var propertyAccess = token.Parent.AncestorsAndSelf().OfType<PropertyAccessExpressionSyntax>().FirstOrDefault();
            return propertyAccess != null &&
                   (propertyAccess.Dot == token || propertyAccess.Name == token);
        }

        private static bool IsAfterNode<T>(SyntaxTree syntaxTree, int position)
            where T: SyntaxNode
        {
            var token = syntaxTree.Root.FindTokenOnLeft(position).GetPreviousIfCurrentContainsOrTouchesPosition(position);
            if (token == null)
                return false;

            return token.Parent
                .AncestorsAndSelf()
                .OfType<T>()
                .Any(parent => parent.LastToken() == token ||
                               parent.LastToken(includeZeroLength: true) == token);
        }

        private static bool IsBeforeNode<T>(SyntaxTree syntaxTree, int position)
            where T: SyntaxNode
        {
            var token = syntaxTree.Root.FindTokenContext(position);
            if (token == null)
                return false;

            return token.Parent
                .AncestorsAndSelf()
                .OfType<T>()
                .Any(parent => parent.FirstToken() == token ||
                               parent.FirstToken(includeZeroLength: true) == token);
        }

        private static bool IsAfterToken(SyntaxTree syntaxTree, int position, SyntaxKind syntaxKind)
        {
            var token = syntaxTree.Root.FindTokenOnLeft(position).GetPreviousIfCurrentContainsOrTouchesPosition(position);
            return token != null && token.Kind == syntaxKind;
        }

        private static bool IsAfterExpression(SyntaxTree syntaxTree, int position)
        {
            return IsAfterNode<ExpressionSyntax>(syntaxTree, position);
        }

        private static bool IsBeforeExpression(SyntaxTree syntaxTree, int position)
        {
            return IsBeforeNode<ExpressionSyntax>(syntaxTree, position);
        }

        private static bool IsBeforeSelectQuery(SyntaxTree syntaxTree, int position)
        {
            if (IsBeforeQuery(syntaxTree, position))
                return true;

            var token = syntaxTree.Root.FindTokenOnLeft(position).GetPreviousIfCurrentContainsOrTouchesPosition(position);
            if (token == null)
                return false;

            // (|
            var parenthesizedExpression = token.Parent.AncestorsAndSelf().OfType<ParenthesizedExpressionSyntax>().FirstOrDefault();
            if (parenthesizedExpression != null && token == parenthesizedExpression.LeftParenthesis)
                return true;

            // expression IN (|

            var inExpression = token.Parent.AncestorsAndSelf().OfType<InExpressionSyntax>().FirstOrDefault();
            if (inExpression != null && token == inExpression.ArgumentList.LeftParenthesis)
                return true;

            return false;
        }

        private static bool InSelectQueryAndNoFromClause(SyntaxTree syntaxTree, int position)
        {
            var tokenAtCaret = syntaxTree.Root.FindTokenOnLeft(position);
            var token = tokenAtCaret.GetPreviousIfCurrentContainsOrTouchesPosition(position);
            if (token == null)
                return false;

            var selectQuery = token.Parent.AncestorsAndSelf().OfType<SelectQuerySyntax>().FirstOrDefault();
            if (selectQuery == null)
                return false;

            if (selectQuery.FromClause != null && selectQuery.FromClause.FromKeyword != tokenAtCaret)
                return false;

            var lastTokenInSelect = selectQuery.SelectClause.LastToken();
            return token == lastTokenInSelect ||
                   tokenAtCaret == lastTokenInSelect;
        }

        private static bool InSelectQueryAndNoWhereClause(SyntaxTree syntaxTree, int position)
        {
            var tokenAtCaret = syntaxTree.Root.FindTokenOnLeft(position);
            var token = tokenAtCaret.GetPreviousIfCurrentContainsOrTouchesPosition(position);
            if (token == null)
                return false;

            var selectQuery = token.Parent.AncestorsAndSelf().OfType<SelectQuerySyntax>().FirstOrDefault();
            if (selectQuery == null)
                return false;

            if (selectQuery.WhereClause != null && selectQuery.WhereClause.WhereKeyword != tokenAtCaret)
                return false;

            var previousClause = selectQuery.FromClause ??
                                 (SyntaxNode) selectQuery.SelectClause;

            var lastTokenInSelect = previousClause.LastToken();
            return token == lastTokenInSelect ||
                   tokenAtCaret == lastTokenInSelect;
        }

        private static bool InSelectQueryAndNoGroupByClause(SyntaxTree syntaxTree, int position)
        {
            var tokenAtCaret = syntaxTree.Root.FindTokenOnLeft(position);
            var token = tokenAtCaret.GetPreviousIfCurrentContainsOrTouchesPosition(position);
            if (token == null)
                return false;

            var selectQuery = token.Parent.AncestorsAndSelf().OfType<SelectQuerySyntax>().FirstOrDefault();
            if (selectQuery == null)
                return false;

            if (selectQuery.GroupByClause != null && selectQuery.GroupByClause.GroupKeyword != tokenAtCaret)
                return false;

            var previousClause = selectQuery.WhereClause ??
                                 selectQuery.FromClause ??
                                 (SyntaxNode) selectQuery.SelectClause;

            var lastTokenInSelect = previousClause.LastToken();
            return token == lastTokenInSelect ||
                   tokenAtCaret == lastTokenInSelect;
        }

        private static bool InSelectQueryAndNoHavingClause(SyntaxTree syntaxTree, int position)
        {
            var tokenAtCaret = syntaxTree.Root.FindTokenOnLeft(position);
            var token = tokenAtCaret.GetPreviousIfCurrentContainsOrTouchesPosition(position);
            if (token == null)
                return false;

            var selectQuery = token.Parent.AncestorsAndSelf().OfType<SelectQuerySyntax>().FirstOrDefault();
            if (selectQuery == null)
                return false;

            if (selectQuery.HavingClause != null && selectQuery.HavingClause.HavingKeyword != tokenAtCaret)
                return false;

            var previousClause = selectQuery.GroupByClause ??
                                 selectQuery.WhereClause ??
                                 selectQuery.FromClause ??
                                 (SyntaxNode) selectQuery.SelectClause;

            var lastTokenInSelect = previousClause.LastToken();
            return token == lastTokenInSelect ||
                   tokenAtCaret == lastTokenInSelect;
        }

        private static bool IsInTopClauseAfterValue(SyntaxTree syntaxTree, int position)
        {
            var token = syntaxTree.Root.FindTokenOnLeft(position).GetPreviousIfCurrentContainsOrTouchesPosition(position);
            if (token == null)
                return false;

            var topClause = token.Parent as TopClauseSyntax;
            return topClause != null &&
                   topClause.Value == token;
        }

        private static bool IsBeforeQuery(SyntaxTree syntaxTree, int position)
        {
            var token = syntaxTree.Root.FindTokenContext(position);
            if (token != null && token.Kind == SyntaxKind.EndOfFileToken && syntaxTree.Root.Root is QuerySyntax)
                return true;

            return IsBeforeNode<QuerySyntax>(syntaxTree, position);
        }

        private static bool InIsNullAfterIsKeyword(SyntaxTree syntaxTree, int position)
        {
            var token = syntaxTree.Root.FindTokenOnLeft(position).GetPreviousIfCurrentContainsOrTouchesPosition(position);
            if (token == null)
                return false;

            var isNullExpression = token.Parent as IsNullExpressionSyntax;
            return isNullExpression != null &&
                   isNullExpression.IsKeyword == token;
        }

        private static bool IsInOrderByAfterExpression(SyntaxTree syntaxTree, int position)
        {
            var token = syntaxTree.Root.FindTokenOnLeft(position).GetPreviousIfCurrentContainsOrTouchesPosition(position);
            if (token == null)
                return false;

            var orderByColumn = token.Parent.AncestorsAndSelf().OfType<OrderByColumnSyntax>().FirstOrDefault();
            if (orderByColumn == null)
                return false;

            return orderByColumn.ColumnSelector.Span.End <= position;
        }

        private static bool IsInCaseAndAfterLabelOrAtStart(SyntaxTree syntaxTree, int position)
        {
            return IsBeforeExpression(syntaxTree, position) ||
                   IsAfterExpression(syntaxTree, position);
        }

        private static bool IsInCaseLabelAndAfterExpression(SyntaxTree syntaxTree, int position)
        {
            return IsBeforeExpression(syntaxTree, position) ||
                   IsAfterExpression(syntaxTree, position);
        }

        private static bool IsInCaseAndAfterLabel(SyntaxTree syntaxTree, int position)
        {
            return IsBeforeExpression(syntaxTree, position) ||
                   IsAfterExpression(syntaxTree, position);
        }

        private static bool IsInCastAfterExpression(SyntaxTree syntaxTree, int position)
        {
            var token = syntaxTree.Root.FindTokenContext(position);
            if (token == null)
                return false;

            var castExpression = token.Parent.AncestorsAndSelf().OfType<CastExpressionSyntax>().FirstOrDefault();
            return castExpression != null &&
                   castExpression.Expression.Span.End <= position;
        }

        private static bool IsInCommonTableExpressionAfterNameOrColumnList(SyntaxTree syntaxTree, int position)
        {
            var token = syntaxTree.Root.FindTokenContext(position);
            if (token == null)
                return false;

            var expression = token.Parent.AncestorsAndSelf().OfType<CommonTableExpressionSyntax>().FirstOrDefault();
            if (expression == null)
                return false;

            return expression.ColumnNameList == null && expression.Name.Span.End <= position ||
                   expression.ColumnNameList != null && expression.ColumnNameList.Span.End <= position;
        }

        private static bool IsAfterAllAnyOperator(SyntaxTree syntaxTree, int position)
        {
            var tokenAtCaret = syntaxTree.Root.FindTokenOnLeft(position);
            var token = tokenAtCaret.GetPreviousIfCurrentContainsOrTouchesPosition(position);
            if (token == null)
                return false;

            var allAny = token.Parent.AncestorsAndSelf().OfType<AllAnySubselectSyntax>().FirstOrDefault();
            if (allAny != null && allAny.Keyword == tokenAtCaret)
                return true;

            var binaryExpression = token.Parent.AncestorsAndSelf().OfType<BinaryExpressionSyntax>().FirstOrDefault();
            return binaryExpression != null &&
                   binaryExpression.Kind.IsValidAllAnyOperator() &&
                   token == binaryExpression.OperatorToken;
        }

        private static bool IsAfterTableReference(SyntaxTree syntaxTree, int position)
        {
            return IsAfterNode<TableReferenceSyntax>(syntaxTree, position);
        }

        private static bool IsInConditionedJoinedTableReferenceAfterLeft(SyntaxTree syntaxTree, int position)
        {
            var tokenAtCaret = syntaxTree.Root.FindTokenOnLeft(position);
            var token = tokenAtCaret.GetPreviousIfCurrentContainsOrTouchesPosition(position);
            if (token == null)
                return false;

            // JOIN foo f |

            var join = token.Parent.AncestorsAndSelf().OfType<ConditionedJoinedTableReferenceSyntax>().FirstOrDefault();
            if (join == null)
                return false;

            var lastOfRight = join.Right.LastToken();
            return token == lastOfRight ||
                   tokenAtCaret == lastOfRight;
        }

        private static bool IsAfterWithKeyword(SyntaxTree syntaxTree, int position)
        {
            return IsAfterToken(syntaxTree, position, SyntaxKind.WithKeyword);
        }

        private static bool IsAfterAllKeyword(SyntaxTree syntaxTree, int position)
        {
            return IsAfterToken(syntaxTree, position, SyntaxKind.AllKeyword);
        }

        private static bool IsAfterDistinctKeyword(SyntaxTree syntaxTree, int position)
        {
            return IsAfterToken(syntaxTree, position, SyntaxKind.DistinctKeyword);
        }

        private static bool IsAfterSelectKeyword(SyntaxTree syntaxTree, int position)
        {
            return IsAfterToken(syntaxTree, position, SyntaxKind.SelectKeyword);
        }

        private static bool IsAfterUnionKeyword(SyntaxTree syntaxTree, int position)
        {
            return IsAfterToken(syntaxTree, position, SyntaxKind.UnionKeyword);
        }

        private static bool IsAfterSoundsKeyword(SyntaxTree syntaxTree, int position)
        {
            return IsAfterToken(syntaxTree, position, SyntaxKind.SoundsKeyword);
        }

        private static bool IsAfterSimilarKeyword(SyntaxTree syntaxTree, int position)
        {
            return IsAfterToken(syntaxTree, position, SyntaxKind.SimilarKeyword);
        }

        private static bool IsAfterOrderKeyword(SyntaxTree syntaxTree, int position)
        {
            return IsAfterToken(syntaxTree, position, SyntaxKind.OrderKeyword);
        }

        private static bool IsAfterGroupKeyword(SyntaxTree syntaxTree, int position)
        {
            return IsAfterToken(syntaxTree, position, SyntaxKind.GroupKeyword);
        }

        private static bool IsAfterInnerKeyword(SyntaxTree syntaxTree, int position)
        {
            return IsAfterToken(syntaxTree, position, SyntaxKind.InnerKeyword);
        }

        private static bool IsAfterOuterKeyword(SyntaxTree syntaxTree, int position)
        {
            return IsAfterToken(syntaxTree, position, SyntaxKind.OuterKeyword);
        }

        private static bool IsAfterLeftKeyword(SyntaxTree syntaxTree, int position)
        {
            return IsAfterToken(syntaxTree, position, SyntaxKind.LeftKeyword);
        }

        private static bool IsAfterRightKeyword(SyntaxTree syntaxTree, int position)
        {
            return IsAfterToken(syntaxTree, position, SyntaxKind.RightKeyword);
        }

        private static bool IsAfterFullKeyword(SyntaxTree syntaxTree, int position)
        {
            return IsAfterToken(syntaxTree, position, SyntaxKind.FullKeyword);
        }
    }
}