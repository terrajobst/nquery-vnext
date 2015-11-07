using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace NQuery.Binding
{
    internal sealed class BoundCaseExpression : BoundExpression
    {
        public BoundCaseExpression(IEnumerable<BoundCaseLabel> caseLabels, BoundExpression elseExpression)
        {
            CaseLabels = caseLabels.ToImmutableArray();
            ElseExpression = elseExpression;
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.CaseExpression; }
        }

        public override Type Type
        {
            get { return CaseLabels.First().ThenExpression.Type; }
        }

        public ImmutableArray<BoundCaseLabel> CaseLabels { get; }

        public BoundExpression ElseExpression { get; }

        public BoundCaseExpression Update(IEnumerable<BoundCaseLabel> caseLabels, BoundExpression elseExpression)
        {
            var newCaseLabels = caseLabels.ToImmutableArray();

            if (newCaseLabels == CaseLabels && elseExpression == ElseExpression)
                return this;

            return new BoundCaseExpression(newCaseLabels, elseExpression);
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("CASE ");

            foreach (var boundCaseLabel in CaseLabels)
            {
                sb.Append("WHEN ");
                sb.Append(boundCaseLabel.Condition);
                sb.Append(" THEN ");
                sb.Append(boundCaseLabel.ThenExpression);
            }

            if (ElseExpression != null)
            {
                sb.Append(" ELSE ");
                sb.Append(ElseExpression);
            }

            sb.Append(" END");

            return sb.ToString();
        }
    }
}