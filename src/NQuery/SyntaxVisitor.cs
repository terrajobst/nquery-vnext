using NQuery.Syntax;

namespace NQuery
{
    public abstract class SyntaxVisitor
    {
        protected void Dispatch(SyntaxNode node)
        {
            if (node is null)
                throw new ArgumentNullException(nameof(node));

            switch (node.Kind)
            {
                case SyntaxKind.CompilationUnit:
                    VisitCompilationUnit((CompilationUnitSyntax)node);
                    break;

                case SyntaxKind.SkippedTokensTrivia:
                    VisitSkippedTokensTrivia((SkippedTokensTriviaSyntax)node);
                    break;

                case SyntaxKind.ComplementExpression:
                case SyntaxKind.IdentityExpression:
                case SyntaxKind.NegationExpression:
                case SyntaxKind.LogicalNotExpression:
                    VisitUnaryExpression((UnaryExpressionSyntax)node);
                    break;

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
                    VisitBinaryExpression((BinaryExpressionSyntax)node);
                    break;

                case SyntaxKind.LikeExpression:
                    VisitLikeExpression((LikeExpressionSyntax)node);
                    break;

                case SyntaxKind.SoundsLikeExpression:
                    VisitSoundsLikeExpression((SoundsLikeExpressionSyntax)node);
                    break;

                case SyntaxKind.SimilarToExpression:
                    VisitSimilarToExpression((SimilarToExpressionSyntax)node);
                    break;

                case SyntaxKind.ParenthesizedExpression:
                    VisitParenthesizedExpression((ParenthesizedExpressionSyntax)node);
                    break;

                case SyntaxKind.BetweenExpression:
                    VisitBetweenExpression((BetweenExpressionSyntax)node);
                    break;

                case SyntaxKind.IsNullExpression:
                    VisitIsNullExpression((IsNullExpressionSyntax)node);
                    break;

                case SyntaxKind.CastExpression:
                    VisitCastExpression((CastExpressionSyntax)node);
                    break;

                case SyntaxKind.CaseExpression:
                    VisitCaseExpression((CaseExpressionSyntax)node);
                    break;

                case SyntaxKind.CaseLabel:
                    VisitCaseLabel((CaseLabelSyntax)node);
                    break;

                case SyntaxKind.CoalesceExpression:
                    VisitCoalesceExpression((CoalesceExpressionSyntax)node);
                    break;

                case SyntaxKind.NullIfExpression:
                    VisitNullIfExpression((NullIfExpressionSyntax)node);
                    break;

                case SyntaxKind.InExpression:
                    VisitInExpression((InExpressionSyntax)node);
                    break;

                case SyntaxKind.LiteralExpression:
                    VisitLiteralExpression((LiteralExpressionSyntax)node);
                    break;

                case SyntaxKind.VariableExpression:
                    VisitParameterExpression((VariableExpressionSyntax)node);
                    break;

                case SyntaxKind.NameExpression:
                    VisitNameExpression((NameExpressionSyntax)node);
                    break;

                case SyntaxKind.PropertyAccessExpression:
                    VisitPropertyAccessExpression((PropertyAccessExpressionSyntax)node);
                    break;

                case SyntaxKind.CountAllExpression:
                    VisitCountAllExpression((CountAllExpressionSyntax)node);
                    break;

                case SyntaxKind.FunctionInvocationExpression:
                    VisitFunctionInvocationExpression((FunctionInvocationExpressionSyntax)node);
                    break;

                case SyntaxKind.MethodInvocationExpression:
                    VisitMethodInvocationExpression((MethodInvocationExpressionSyntax)node);
                    break;

                case SyntaxKind.ArgumentList:
                    VisitArgumentList((ArgumentListSyntax)node);
                    break;

                case SyntaxKind.SingleRowSubselect:
                    VisitSingleRowSubselect((SingleRowSubselectSyntax)node);
                    break;

                case SyntaxKind.ExistsSubselect:
                    VisitExistsSubselect((ExistsSubselectSyntax)node);
                    break;

                case SyntaxKind.AllAnySubselect:
                    VisitAllAnySubselect((AllAnySubselectSyntax)node);
                    break;

                case SyntaxKind.ParenthesizedTableReference:
                    VisitParenthesizedTableReference((ParenthesizedTableReferenceSyntax)node);
                    break;

                case SyntaxKind.NamedTableReference:
                    VisitNamedTableReference((NamedTableReferenceSyntax)node);
                    break;

                case SyntaxKind.CrossJoinedTableReference:
                    VisitCrossJoinedTableReference((CrossJoinedTableReferenceSyntax)node);
                    break;

                case SyntaxKind.InnerJoinedTableReference:
                    VisitInnerJoinedTableReference((InnerJoinedTableReferenceSyntax)node);
                    break;

                case SyntaxKind.OuterJoinedTableReference:
                    VisitOuterJoinedTableReference((OuterJoinedTableReferenceSyntax)node);
                    break;

                case SyntaxKind.DerivedTableReference:
                    VisitDerivedTableReference((DerivedTableReferenceSyntax)node);
                    break;

                case SyntaxKind.ExceptQuery:
                    VisitExceptQuery((ExceptQuerySyntax)node);
                    break;

                case SyntaxKind.UnionQuery:
                    VisitUnionQuery((UnionQuerySyntax)node);
                    break;

                case SyntaxKind.IntersectQuery:
                    VisitIntersectQuery((IntersectQuerySyntax)node);
                    break;

                case SyntaxKind.OrderedQuery:
                    VisitOrderedQuery((OrderedQuerySyntax)node);
                    break;

                case SyntaxKind.OrderByColumn:
                    VisitOrderByColumn((OrderByColumnSyntax)node);
                    break;

                case SyntaxKind.ParenthesizedQuery:
                    VisitParenthesizedQuery((ParenthesizedQuerySyntax)node);
                    break;

                case SyntaxKind.CommonTableExpressionQuery:
                    VisitCommonTableExpressionQuery((CommonTableExpressionQuerySyntax)node);
                    break;

                case SyntaxKind.CommonTableExpression:
                    VisitCommonTableExpression((CommonTableExpressionSyntax)node);
                    break;

                case SyntaxKind.CommonTableExpressionColumnName:
                    VisitCommonTableExpressionColumnName((CommonTableExpressionColumnNameSyntax)node);
                    break;

                case SyntaxKind.CommonTableExpressionColumnNameList:
                    VisitCommonTableExpressionColumnNameList((CommonTableExpressionColumnNameListSyntax)node);
                    break;

                case SyntaxKind.SelectQuery:
                    VisitSelectQuery((SelectQuerySyntax)node);
                    break;

                case SyntaxKind.TopClause:
                    VisitTopClause((TopClauseSyntax)node);
                    break;

                case SyntaxKind.WildcardSelectColumn:
                    VisitWildcardSelectColumn((WildcardSelectColumnSyntax)node);
                    break;

                case SyntaxKind.ExpressionSelectColumn:
                    VisitExpressionSelectColumn((ExpressionSelectColumnSyntax)node);
                    break;

                case SyntaxKind.SelectClause:
                    VisitSelectClause((SelectClauseSyntax)node);
                    break;

                case SyntaxKind.FromClause:
                    VisitFromClause((FromClauseSyntax)node);
                    break;

                case SyntaxKind.WhereClause:
                    VisitWhereClause((WhereClauseSyntax)node);
                    break;

                case SyntaxKind.GroupByClause:
                    VisitGroupByClause((GroupByClauseSyntax)node);
                    break;

                case SyntaxKind.GroupByColumn:
                    VisitGroupByColumn((GroupByColumnSyntax)node);
                    break;

                case SyntaxKind.HavingClause:
                    VisitHavingClause((HavingClauseSyntax)node);
                    break;

                case SyntaxKind.Alias:
                    VisitAlias((AliasSyntax)node);
                    break;

                default:
                    throw ExceptionBuilder.UnexpectedValue(node.Kind);
            }
        }

        public virtual void DefaultVisit(SyntaxNode node)
        {
        }

        public virtual void VisitAlias(AliasSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitArgumentList(ArgumentListSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitCaseLabel(CaseLabelSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitCommonTableExpressionColumnNameList(CommonTableExpressionColumnNameListSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitCommonTableExpressionColumnName(CommonTableExpressionColumnNameSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitCommonTableExpression(CommonTableExpressionSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitCompilationUnit(CompilationUnitSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitBetweenExpression(BetweenExpressionSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitBinaryExpression(BinaryExpressionSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitCaseExpression(CaseExpressionSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitCastExpression(CastExpressionSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitCoalesceExpression(CoalesceExpressionSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitCountAllExpression(CountAllExpressionSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitFunctionInvocationExpression(FunctionInvocationExpressionSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitInExpression(InExpressionSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitIsNullExpression(IsNullExpressionSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitLikeExpression(LikeExpressionSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitLiteralExpression(LiteralExpressionSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitMethodInvocationExpression(MethodInvocationExpressionSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitNameExpression(NameExpressionSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitNullIfExpression(NullIfExpressionSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitParameterExpression(VariableExpressionSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitParenthesizedExpression(ParenthesizedExpressionSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitPropertyAccessExpression(PropertyAccessExpressionSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitSimilarToExpression(SimilarToExpressionSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitSoundsLikeExpression(SoundsLikeExpressionSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitAllAnySubselect(AllAnySubselectSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitExistsSubselect(ExistsSubselectSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitSingleRowSubselect(SingleRowSubselectSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitSkippedTokensTrivia(SkippedTokensTriviaSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitUnaryExpression(UnaryExpressionSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitSelectClause(SelectClauseSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitFromClause(FromClauseSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitGroupByClause(GroupByClauseSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitGroupByColumn(GroupByColumnSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitHavingClause(HavingClauseSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitOrderByColumn(OrderByColumnSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitCommonTableExpressionQuery(CommonTableExpressionQuerySyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitExceptQuery(ExceptQuerySyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitIntersectQuery(IntersectQuerySyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitOrderedQuery(OrderedQuerySyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitParenthesizedQuery(ParenthesizedQuerySyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitSelectQuery(SelectQuerySyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitUnionQuery(UnionQuerySyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitExpressionSelectColumn(ExpressionSelectColumnSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitWildcardSelectColumn(WildcardSelectColumnSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitDerivedTableReference(DerivedTableReferenceSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitCrossJoinedTableReference(CrossJoinedTableReferenceSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitInnerJoinedTableReference(InnerJoinedTableReferenceSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitOuterJoinedTableReference(OuterJoinedTableReferenceSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitNamedTableReference(NamedTableReferenceSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitParenthesizedTableReference(ParenthesizedTableReferenceSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitTopClause(TopClauseSyntax node)
        {
            DefaultVisit(node);
        }

        public virtual void VisitWhereClause(WhereClauseSyntax node)
        {
            DefaultVisit(node);
        }
    }
}