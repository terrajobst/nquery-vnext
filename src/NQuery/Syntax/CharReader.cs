using System;

namespace NQuery.Syntax
{
    internal sealed class CharReader
    {
        private readonly string _source;
        private int _position;

        public CharReader(string source)
        {
            _source = source;
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
            return index < _source.Length
                       ? _source[index]
                       : '\0';
        }
    }
}