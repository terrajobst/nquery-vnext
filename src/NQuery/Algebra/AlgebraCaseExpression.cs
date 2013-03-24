using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace NQuery.Algebra
{
    internal sealed class AlgebraCaseExpression : AlgebraExpression
    {
        private readonly ReadOnlyCollection<AlgebraCaseLabel> _caseLabels;
        private readonly AlgebraExpression _elseExpression;

        public AlgebraCaseExpression(IList<AlgebraCaseLabel> caseLabels, AlgebraExpression elseExpression)
        {
            _caseLabels = new ReadOnlyCollection<AlgebraCaseLabel>(caseLabels);
            _elseExpression = elseExpression;
        }

        public override AlgebraKind Kind
        {
            get { return AlgebraKind.CaseExpression; }
        }

        public override Type Type
        {
            get { return _caseLabels.First().Result.Type; }
        }

        public ReadOnlyCollection<AlgebraCaseLabel> CaseLabels
        {
            get { return _caseLabels; }
        }

        public AlgebraExpression ElseExpression
        {
            get { return _elseExpression; }
        }
    }
}