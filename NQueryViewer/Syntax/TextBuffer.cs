using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace NQueryViewer.Syntax
{
    public sealed class TextBuffer
    {
        private readonly string _text;
        private readonly ReadOnlyCollection<TextLine> _lines;

        public TextBuffer(string text)
        {
            _text = text;
            var textLines = ParseTextLines(text);
            _lines = new ReadOnlyCollection<TextLine>(textLines);
        }

        private static List<TextLine> ParseTextLines(string text)
        {
            var textLines = new List<TextLine>();
            var position = 0;
            var lineStart = 0;

            while (position < text.Length)
            {
                var lineBreakWidth = GetLineBreakWidth(text, position);

                if (lineBreakWidth == 0)
                {
                    position++;
                }
                else
                {
                    AddLine(textLines, text, lineStart, position, lineBreakWidth);

                    position += lineBreakWidth;
                    lineStart = position;
                }
            }

            if (lineStart < position)
                AddLine(textLines, text, lineStart, text.Length, 0);

            return textLines;
        }

        private static void AddLine(List<TextLine> textLines, string text, int lineStart, int lineEnd, int lineBreakWidth)
        {
            var textSpan = TextSpan.FromBounds(lineStart, lineEnd);
            var str = text.Substring(textSpan.Start, textSpan.Length);
            var index = textLines.Count;
            var textLine = new TextLine(str, textSpan, lineBreakWidth, index);
            textLines.Add(textLine);
        }

        private static int GetLineBreakWidth(string text, int position)
        {
            const char eof = '\0';
            const char cr = '\r';
            const char lf = '\n';

            var n = position + 1;
            var c = text[position];
            var l = n < text.Length ? text[n] : eof;

            if (c == cr && l == lf)
                return 2;

            if (c == cr || c == lf)
                return 1;

            return 0;
        }

        public TextLine GetLineFromPosition(int position)
        {
            var lower = 0;
            var upper = _lines.Count - 1;
            while (lower <= upper)
            {
                var index = lower + ((upper - lower) >> 1);
                var current = _lines[index];
                var start = current.TextSpan.Start;
                if (start == position)
                    return current;

                if (start > position)
                    upper = index - 1;
                else
                    lower = index + 1;
            }

            var r = lower - 1;
            return r < _lines.Count ? _lines[r] : null;
        }

        public string Text
        {
            get { return _text; }
        }

        public ReadOnlyCollection<TextLine> Lines
        {
            get { return _lines; }
        }
    }
}