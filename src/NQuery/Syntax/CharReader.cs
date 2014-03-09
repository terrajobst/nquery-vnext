using System;

using NQuery.Text;

namespace NQuery.Syntax
{
    internal sealed class CharReader
    {
        private readonly TextBuffer _textBuffer;
        private int _position;

        public CharReader(TextBuffer textBuffer)
        {
            _textBuffer = textBuffer;
        }

        public void NextChar()
        {
            _position++;
        }

        public int Position
        {
            get { return _position; }
        }

        public char Current
        {
            get
            {
                return Peek(0);
            }
        }

        public char Peek()
        {
            return Peek(1);
        }

        private char Peek(int offset)
        {
            var index = _position + offset;
            return index < _textBuffer.Length
                       ? _textBuffer[index]
                       : '\0';
        }
    }
}