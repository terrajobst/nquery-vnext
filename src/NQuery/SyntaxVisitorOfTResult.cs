using System;

using NQuery.Syntax;

namespace NQuery
{
    public abstract class SyntaxVisitor<TResult>
    {
        protected TResult Dispatch(SyntaxNode node)
        {
            switch (node.Kind)
            {
                case SyntaxKind.CompilationUnit:
                    return VisitCompilationUnit((CompilationUnitSyntax)node);

                case SyntaxKind.SkippedTokensTrivia:
                    return VisitSkippedTokensTrivia((SkippedTokensTriviaSyntax)node);

                case SyntaxKind.ComplementExpression:
                case SyntaxKind.IdentityExpression:
                case SyntaxKind.NegationExpression:
                case SyntaxKind.LogicalNotExpression:
                     return VisitUnaryExpression((UnaryExpressionSyntax)node);

                case SyntaxKind.BitwiseAndExpression:
                case SyntaxKind.BitwiseOrExpression:
                case SyntaxKind.ExclusiveOrExpression:
                case SyntaxKind.AddExpression:
                case SyntaxKind.SubExpression:
                case SyntaxKind.MultiplyExpression:
                case SyntaxKind.DivideExpression:
                case SyntaxKind.ModuloExpression:
                case SyntaxKind.PowerExpression:
                case SyntaxKind.EqualExpression:
                case SyntaxKind.NotEqualExpression:
                case SyntaxKind.LessExpression:
                case SyntaxKind.LessOrEqualExpression:
                case SyntaxKind.GreaterExpression:
                case SyntaxKind.GreaterOrEqualExpression:
                case SyntaxKind.NotLessExpression:
                case SyntaxKind.NotGreaterExpression:
                case SyntaxKind.LeftShiftExpression:
                case SyntaxKind.RightShiftExpression:
                case SyntaxKind.LogicalAndExpression:
                case SyntaxKind.LogicalOrExpression:
                     return VisitBinaryExpression((BinaryExpressionSyntax)node);

                case SyntaxKind.LikeExpression:
                     return VisitLikeExpression((LikeExpressionSyntax)node);

                case SyntaxKind.SoundslikeExpression:
                     return VisitSoundslikeExpression((SoundslikeExpressionSyntax)node);

                case SyntaxKind.SimilarToExpression:
                     return VisitSimilarToExpression((SimilarToExpressionSyntax)node);

                case SyntaxKind.ParenthesizedExpression:
                     return VisitParenthesizedExpression((ParenthesizedExpressionSyntax)node);

                case SyntaxKind.BetweenExpression:
                     return VisitBetweenExpression((BetweenExpressionSyntax)node);

                case SyntaxKind.IsNullExpression:
                     return VisitIsNullExpression((IsNullExpressionSyntax)node);

                case SyntaxKind.CastExpression:
                     return VisitCastExpression((CastExpressionSyntax)node);

                case SyntaxKind.CaseExpression:
                     return VisitCaseExpression((CaseExpressionSyntax)node);

                case SyntaxKind.CaseLabel:
                     return VisitCaseLabel((CaseLabelSyntax)node);

                case SyntaxKind.CoalesceExpression:
                     return VisitCoalesceExpression((CoalesceExpressionSyntax)node);

                case SyntaxKind.NullIfExpression:
                     return VisitNullIfExpression((NullIfExpressionSyntax)node);

                case SyntaxKind.InExpression:
                     return VisitInExpression((InExpressionSyntax)node);

                case SyntaxKind.LiteralExpression:
                     return VisitLiteralExpression((LiteralExpressionSyntax)node);

                case SyntaxKind.VariableExpression:
                     return VisitParameterExpression((VariableExpressionSyntax)node);

                case SyntaxKind.NameExpression:
                     return VisitNameExpression((NameExpressionSyntax)node);

                case SyntaxKind.PropertyAccessExpression:
                     return VisitPropertyAccessExpression((PropertyAccessExpressionSyntax)node);

                case SyntaxKind.CountAllExpression:
                     return VisitCountAllExpression((CountAllExpressionSyntax)node);

                case SyntaxKind.FunctionInvocationExpression:
                     return VisitFunctionInvocationExpression((FunctionInvocationExpressionSyntax)node);

                case SyntaxKind.MethodInvocationExpression:
                     return VisitMethodInvocationExpression((MethodInvocationExpressionSyntax)node);

                case SyntaxKind.ArgumentList:
                     return VisitArgumentList((ArgumentListSyntax)node);

                case SyntaxKind.SingleRowSubselect:
                     return VisitSingleRowSubselect((SingleRowSubselectSyntax)node);

                case SyntaxKind.ExistsSubselect:
                     return VisitExistsSubselect((ExistsSubselectSyntax)node);

                case SyntaxKind.AllAnySubselect:
                     return VisitAllAnySubselect((AllAnySubselectSyntax)node);

                case SyntaxKind.ParenthesizedTableReference:
                     return VisitParenthesizedTableReference((ParenthesizedTableReferenceSyntax)node);

                case SyntaxKind.NamedTableReference:
                     return VisitNamedTableReference((NamedTableReferenceSyntax)node);

                case SyntaxKind.CrossJoinedTableReference:
                     return VisitCrossJoinedTableReference((CrossJoinedTableReferenceSyntax)node);

                case SyntaxKind.InnerJoinedTableReference:
                     return VisitInnerJoinedTableReference((InnerJoinedTableReferenceSyntax)node);

                case SyntaxKind.OuterJoinedTableReference:
                     return VisitOuterJoinedTableReference((OuterJoinedTableReferenceSyntax)node);

                case SyntaxKind.DerivedTableReference:
                     return VisitDerivedTableReference((DerivedTableReferenceSyntax)node);

                case SyntaxKind.ExceptQuery:
                     return VisitExceptQuery((ExceptQuerySyntax)node);

                case SyntaxKind.UnionQuery:
                     return VisitUnionQuery((UnionQuerySyntax)node);

                case SyntaxKind.IntersectQuery:
                     return VisitIntersectQuery((IntersectQuerySyntax)node);

                case SyntaxKind.OrderedQuery:
                     return VisitOrderedQuery((OrderedQuerySyntax)node);

                case SyntaxKind.OrderByColumn:
                     return VisitOrderByColumn((OrderByColumnSyntax)node);

                case SyntaxKind.ParenthesizedQuery:
                     return VisitParenthesizedQuery((ParenthesizedQuerySyntax)node);

                case SyntaxKind.CommonTableExpressionQuery:
                     return VisitCommonTableExpressionQuery((CommonTableExpressionQuerySyntax)node);

                case SyntaxKind.CommonTableExpression:
                     return VisitCommonTableExpression((CommonTableExpressionSyntax)node);

                case SyntaxKind.CommonTableExpressionColumnName:
                     return VisitCommonTableExpressionColumnName((CommonTableExpressionColumnNameSyntax)node);

                case SyntaxKind.CommonTableExpressionColumnNameList:
                     return VisitCommonTableExpressionColumnNameList((CommonTableExpressionColumnNameListSyntax)node);

                case SyntaxKind.SelectQuery:
                     return VisitSelectQuery((SelectQuerySyntax)node);

                case SyntaxKind.TopClause:
                     return VisitTopClause((TopClauseSyntax)node);

                case SyntaxKind.WildcardSelectColumn:
                     return VisitWildcardSelectColumn((WildcardSelectColumnSyntax)node);

                case SyntaxKind.ExpressionSelectColumn:
                     return VisitExpressionSelectColumn((ExpressionSelectColumnSyntax)node);

                case SyntaxKind.SelectClause:
                     return VisitSelectClause((SelectClauseSyntax)node);

                case SyntaxKind.FromClause:
                     return VisitFromClause((FromClauseSyntax)node);

                case SyntaxKind.WhereClause:
                     return VisitWhereClause((WhereClauseSyntax)node);

                case SyntaxKind.GroupByClause:
                     return VisitGroupByClause((GroupByClauseSyntax)node);

                case SyntaxKind.GroupByColumn:
                     return VisitGroupByColumn((GroupByColumnSyntax)node);

                case SyntaxKind.HavingClause:
                     return VisitHavingClause((HavingClauseSyntax)node);

                case SyntaxKind.Alias:
                     return VisitAlias((AliasSyntax)node);

                default:
                    throw new ArgumentException($"Unknown node kind: {node.Kind}", nameof(node));
            }
        }

        public virtual TResult DefaultVisit(SyntaxNode node)
        {
            return default(TResult);
        }

        public virtual TResult VisitAlias(AliasSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual TResult VisitArgumentList(ArgumentListSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual TResult VisitCaseLabel(CaseLabelSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual TResult VisitCommonTableExpressionColumnNameList(CommonTableExpressionColumnNameListSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual TResult VisitCommonTableExpressionColumnName(CommonTableExpressionColumnNameSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual TResult VisitCommonTableExpression(CommonTableExpressionSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual TResult VisitCompilationUnit(CompilationUnitSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual TResult VisitBetweenExpression(BetweenExpressionSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual TResult VisitBinaryExpression(BinaryExpressionSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual TResult VisitCaseExpression(CaseExpressionSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual TResult VisitCastExpression(CastExpressionSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual TResult VisitCoalesceExpression(CoalesceExpressionSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual TResult VisitCountAllExpression(CountAllExpressionSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual TResult VisitFunctionInvocationExpression(FunctionInvocationExpressionSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual TResult VisitInExpression(InExpressionSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual TResult VisitIsNullExpression(IsNullExpressionSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual TResult VisitLikeExpression(LikeExpressionSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual TResult VisitLiteralExpression(LiteralExpressionSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual TResult VisitMethodInvocationExpression(MethodInvocationExpressionSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual TResult VisitNameExpression(NameExpressionSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual TResult VisitNullIfExpression(NullIfExpressionSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual TResult VisitParameterExpression(VariableExpressionSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual TResult VisitParenthesizedExpression(ParenthesizedExpressionSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual TResult VisitPropertyAccessExpression(PropertyAccessExpressionSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual TResult VisitSimilarToExpression(SimilarToExpressionSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual TResult VisitSoundslikeExpression(SoundslikeExpressionSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual TResult VisitAllAnySubselect(AllAnySubselectSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual TResult VisitExistsSubselect(ExistsSubselectSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual TResult VisitSingleRowSubselect(SingleRowSubselectSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual TResult VisitSkippedTokensTrivia(SkippedTokensTriviaSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual TResult VisitUnaryExpression(UnaryExpressionSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual TResult VisitSelectClause(SelectClauseSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual TResult VisitFromClause(FromClauseSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual TResult VisitGroupByClause(GroupByClauseSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual TResult VisitGroupByColumn(GroupByColumnSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual TResult VisitHavingClause(HavingClauseSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual TResult VisitOrderByColumn(OrderByColumnSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual TResult VisitCommonTableExpressionQuery(CommonTableExpressionQuerySyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual TResult VisitExceptQuery(ExceptQuerySyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual TResult VisitIntersectQuery(IntersectQuerySyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual TResult VisitOrderedQuery(OrderedQuerySyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual TResult VisitParenthesizedQuery(ParenthesizedQuerySyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual TResult VisitSelectQuery(SelectQuerySyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual TResult VisitUnionQuery(UnionQuerySyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual TResult VisitExpressionSelectColumn(ExpressionSelectColumnSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual TResult VisitWildcardSelectColumn(WildcardSelectColumnSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual TResult VisitDerivedTableReference(DerivedTableReferenceSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual TResult VisitCrossJoinedTableReference(CrossJoinedTableReferenceSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual TResult VisitInnerJoinedTableReference(InnerJoinedTableReferenceSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual TResult VisitOuterJoinedTableReference(OuterJoinedTableReferenceSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual TResult VisitNamedTableReference(NamedTableReferenceSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual TResult VisitParenthesizedTableReference(ParenthesizedTableReferenceSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual TResult VisitTopClause(TopClauseSyntax node)
        {
            return DefaultVisit(node);
        }

        public virtual TResult VisitWhereClause(WhereClauseSyntax node)
        {
            return DefaultVisit(node);
        }
    }
}