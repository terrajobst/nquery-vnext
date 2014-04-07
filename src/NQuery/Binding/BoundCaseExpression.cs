using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

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

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("CASE ");

            foreach (var boundCaseLabel in _caseLabels)
            {
                sb.Append("WHEN ");
                sb.Append(boundCaseLabel.Condition);
                sb.Append(" THEN ");
                sb.Append(boundCaseLabel.ThenExpression);
            }

            if (_elseExpression != null)
            {
                sb.Append(" ELSE ");
                sb.Append(_elseExpression);
            }

            sb.Append(" END");

            return sb.ToString();
        }
    }
}