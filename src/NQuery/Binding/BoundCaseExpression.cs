using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace NQuery.Binding
{
    internal sealed class BoundCaseExpression : BoundExpression
    {
        private readonly ImmutableArray<BoundCaseLabel> _caseLabels;
        private readonly BoundExpression _elseExpression;

        public BoundCaseExpression(IEnumerable<BoundCaseLabel> caseLabels, BoundExpression elseExpression)
        {
            _caseLabels = caseLabels.ToImmutableArray();
            _elseExpression = elseExpression;
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.CaseExpression; }
        }

        public override Type Type
        {
            get { return _caseLabels.First().ThenExpression.Type; }
        }

        public ImmutableArray<BoundCaseLabel> CaseLabels
        {
            get { return _caseLabels; }
        }

        public BoundExpression ElseExpression
        {
            get { return _elseExpression; }
        }

        public BoundCaseExpression Update(IEnumerable<BoundCaseLabel> caseLabels, BoundExpression elseExpression)
        {
            var newCaseLabels = caseLabels.ToImmutableArray();

            if (newCaseLabels == _caseLabels && elseExpression == _elseExpression)
                return this;

            return new BoundCaseExpression(newCaseLabels, elseExpression);
        }
    }
}