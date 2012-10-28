namespace NQuery.Language.BoundNodes
{
    internal sealed class BoundCaseLabel : BoundNode
    {
        private readonly BoundExpression _condition;
        private readonly BoundExpression _thenExpression;

        public BoundCaseLabel(BoundExpression condition, BoundExpression thenExpression)
        {
            _condition = condition;
            _thenExpression = thenExpression;
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.CaseLabel; }
        }

        public BoundExpression Condition
        {
            get { return _condition; }
        }

        public BoundExpression ThenExpression
        {
            get { return _thenExpression; }
        }
    }
}