using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace NQuery.Binding
{
    internal sealed class BoundCaseExpression : BoundExpression
    {
        private readonly ReadOnlyCollection<BoundCaseLabel> _caseLabels;
        private readonly BoundExpression _elseExpresion;

        public BoundCaseExpression(IList<BoundCaseLabel> caseLabels, BoundExpression elseExpresion)
        {
            _caseLabels = new ReadOnlyCollection<BoundCaseLabel>(caseLabels);
            _elseExpresion = elseExpresion;
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

        public BoundExpression ElseExpresion
        {
            get { return _elseExpresion; }
        }
    }
}