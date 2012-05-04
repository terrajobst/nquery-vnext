using System;

namespace NQueryViewer.Syntax
{
    public sealed class TextLine
    {
        private readonly string _text;
        private readonly TextSpan _textSpan;
        private readonly int _lineBreakLength;
        private readonly int _index;

        public TextLine(string text, TextSpan textSpan, int lineBreakLength, int index)
        {
            _text = text;
            _textSpan = textSpan;
            _lineBreakLength = lineBreakLength;
            _index = index;
        }

        public string Text
        {
            get { return _text; }
        }

        public TextSpan TextSpan
        {
            get { return _textSpan; }
        }

        public TextSpan TextSpanWithLineBreak
        {
            get { return new TextSpan(_textSpan.Start, _textSpan.Length + _lineBreakLength); }
        }

        public int Index
        {
            get { return _index; }
        }
    }
}