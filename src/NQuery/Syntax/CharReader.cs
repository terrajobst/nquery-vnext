using System;

using NQuery.Text;

namespace NQuery.Syntax
{
    internal sealed class CharReader
    {
        private readonly SourceText _text;
        private int _position;

        public CharReader(SourceText text)
        {
            _text = text;
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
            return index < _text.Length
                       ? _text[index]
                       : '\0';
        }
    }
}