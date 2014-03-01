using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;

namespace NQuery.Binding
{
    internal sealed class BoundCaseExpression : BoundExpression
    {
        private readonly ReadOnlyCollection<BoundCaseLabel> _caseLabels;
        private readonly BoundExpression _elseExpression;

        public BoundCaseExpression(IList<BoundCaseLabel> caseLabels, BoundExpression elseExpression)
        {
            _caseLabels = new ReadOnlyCollection<BoundCaseLabel>(caseLabels);
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

        public ReadOnlyCollection<BoundCaseLabel> CaseLabels
        {
            get { return _caseLabels; }
        }

        public BoundExpression ElseExpression
        {
            get { return _elseExpression; }
        }

        public BoundCaseExpression Update(IList<BoundCaseLabel> caseLabels, BoundExpression elseExpression)
        {
            if (caseLabels == _caseLabels && elseExpression == _elseExpression)
                return this;

            return new BoundCaseExpression(caseLabels, elseExpression);
        }
    }
}