using System;

namespace NQuery.Text
{
    internal class StaticSourceTextContainer : SourceTextContainer
    {
        private readonly SourceText _current;

        public StaticSourceTextContainer(SourceText current)
        {
            _current = current;
        }

        public override SourceText Current
        {
            get { return _current; }
        }

        public override event EventHandler<EventArgs> CurrentChanged
        {
            add {}
            remove {}
        }
    }
}