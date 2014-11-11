using System;
using System.Threading;

namespace NQuery.Text
{
    public abstract class SourceText
    {
        private SourceTextContainer _container;

        protected SourceText()
            : this(null)
        {
        }

        protected SourceText(SourceTextContainer container)
        {
            _container = container;
        }

        public static SourceText From(string text)
        {
            return new StringText(text);
        }

        public SourceTextContainer Container
        {
            get
            {
                if (_container == null)
                {
                    var container = new StaticSourceTextContainer(this);
                    Interlocked.CompareExchange(ref _container, container, null);
                }
                return _container;
            }
        }

        public TextLine GetLineFromPosition(int position)
        {
            if (position < 0 || position > Length)
                throw new ArgumentOutOfRangeException("position");

            var lineNumber = GetLineNumberFromPosition(position);
            return Lines[lineNumber];
        }

        public abstract int GetLineNumberFromPosition(int position);

        public TextLocation GetTextLocation(int position)
        {
            var line = GetLineFromPosition(position);
            var lineNumber = line.LineNumber;
            var column = position - line.Span.Start;
            return new TextLocation(lineNumber, column);
        }

        public int GetPosition(TextLocation location)
        {
            var textLine = Lines[location.Line];
            return textLine.Span.Start + location.Column;
        }

        public abstract string GetText(TextSpan textSpan);

        public string GetText(int position, int length)
        {
            return GetText(new TextSpan(position, length));
        }

        public string GetText(int position)
        {
            var remaining = Length - position;
            return GetText(position, remaining);
        }

        public string GetText()
        {
            return GetText(0, Length);
        }

        public abstract char this[int index] { get; }

        public abstract int Length { get; }

        public abstract TextLineCollection Lines { get; }
    }
}