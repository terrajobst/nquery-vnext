using System;

using NQuery.Text;

namespace NQuery.Authoring.Rearrangement
{
    public sealed class ArrangementOperation
    {
        private readonly TextSpan _span;
        private readonly IArrangementAction _action;

        public ArrangementOperation(TextSpan span, IArrangementAction action)
        {
            _span = span;
            _action = action;
        }

        public TextSpan Span
        {
            get { return _span; }
        }

        public IArrangementAction Action
        {
            get { return _action; }
        }
    }
}