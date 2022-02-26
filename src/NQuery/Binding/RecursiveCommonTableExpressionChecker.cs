using NQuery.Symbols;
using NQuery.Syntax;

namespace NQuery.Binding
{
    internal sealed class RecursiveCommonTableExpressionChecker : BoundTreeWalker
    {
        private readonly CommonTableExpressionSyntax _syntax;
        private readonly List<Diagnostic> _diagnostics;
        private readonly CommonTableExpressionSymbol _symbol;

        private int _subqueryCounter;
        private bool _hasSeenRecursiveReference;

        public RecursiveCommonTableExpressionChecker(CommonTableExpressionSyntax syntax, List<Diagnostic> diagnostics, CommonTableExpressionSymbol symbol)
        {
            _syntax = syntax;
            _diagnostics = diagnostics;
            _symbol = symbol;
        }

        protected override void VisitSingleRowSubselect(BoundSingleRowSubselect node)
        {
            _subqueryCounter++;
            base.VisitSingleRowSubselect(node);
            _subqueryCounter--;
        }

        protected override void VisitExistsSubselect(BoundExistsSubselect node)
        {
            _subqueryCounter++;
            base.VisitExistsSubselect(node);
            _subqueryCounter--;
        }

        protected override void VisitTableRelation(BoundTableRelation node)
        {
            var hasRecursiveReference = node.TableInstance.Table == _symbol;
            var hasRecursiveReferenceInSubquery = _subqueryCounter > 0 && hasRecursiveReference;
            var hasMultipleRecursiveReferences = _hasSeenRecursiveReference && hasRecursiveReference;
            if (hasRecursiveReference)
                _hasSeenRecursiveReference = true;

            if (hasRecursiveReferenceInSubquery)
                _diagnostics.ReportCteContainsRecursiveReferenceInSubquery(_syntax.Name);

            if (hasMultipleRecursiveReferences)
                _diagnostics.ReportCteContainsMultipleRecursiveReferences(_syntax.Name);

            base.VisitTableRelation(node);
        }

        protected override void VisitDerivedTableRelation(BoundDerivedTableRelation node)
        {
            // Don't visit children.
        }

        protected override void VisitUnionRelation(BoundUnionRelation node)
        {
            _diagnostics.ReportCteContainsUnion(_syntax.Name);
            base.VisitUnionRelation(node);
        }

        protected override void VisitSortRelation(BoundSortRelation node)
        {
            if (node.IsDistinct)
                _diagnostics.ReportCteContainsDistinct(_syntax.Name);

            base.VisitSortRelation(node);
        }

        protected override void VisitTopRelation(BoundTopRelation node)
        {
            _diagnostics.ReportCteContainsTop(_syntax.Name);
            base.VisitTopRelation(node);
        }

        protected override void VisitJoinRelation(BoundJoinRelation node)
        {
            if (node.JoinType == BoundJoinType.FullOuter ||
                node.JoinType == BoundJoinType.LeftOuter ||
                node.JoinType == BoundJoinType.RightOuter)
            {
                _diagnostics.ReportCteContainsOuterJoin(_syntax.Name);
            }

            base.VisitJoinRelation(node);
        }

        protected override void VisitGroupByAndAggregationRelation(BoundGroupByAndAggregationRelation node)
        {
            _diagnostics.ReportCteContainsGroupByHavingOrAggregate(_syntax.Name);
            base.VisitGroupByAndAggregationRelation(node);
        }
    }
}