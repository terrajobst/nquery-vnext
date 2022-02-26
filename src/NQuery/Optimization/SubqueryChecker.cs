using NQuery.Binding;

namespace NQuery.Optimization
{
    internal sealed class SubqueryChecker : BoundTreeWalker
    {
        public bool SubqueryFound { get; private set; }

        public static bool ContainsSubquery(BoundRelation node)
        {
            var checker = new SubqueryChecker();
            checker.VisitRelation(node);
            return checker.SubqueryFound;
        }

        public static bool ContainsSubquery(BoundExpression node)
        {
            var checker = new SubqueryChecker();
            checker.VisitExpression(node);
            return checker.SubqueryFound;
        }

        public override void VisitRelation(BoundRelation node)
        {
            if (SubqueryFound)
                return;

            base.VisitRelation(node);
        }

        public override void VisitExpression(BoundExpression node)
        {
            if (SubqueryFound)
                return;

            base.VisitExpression(node);
        }

        protected override void VisitSingleRowSubselect(BoundSingleRowSubselect node)
        {
            SubqueryFound = true;
        }

        protected override void VisitExistsSubselect(BoundExistsSubselect node)
        {
            SubqueryFound = true;
        }
    }
}