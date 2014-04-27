using System;

using NQuery.Text;

namespace NQuery.Authoring.Document
{
    public abstract class TextBufferProvider
    {
        private TextBuffer _current;

        public TextBuffer Current
        {
            get { return _current; }
            protected set
            {
                if (_current != value)
                {
                    _current = value;
                    OnCurrentChanged();
                }
            }
        }

        protected void OnCurrentChanged()
        {
            var handler = CurrentChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        public event EventHandler<EventArgs> CurrentChanged;
    }
}