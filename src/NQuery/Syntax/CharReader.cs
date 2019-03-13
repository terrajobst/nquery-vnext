#nullable enable

using System;

using NQuery.Text;

namespace NQuery.Syntax
{
    internal sealed class CharReader
    {
        private readonly SourceText _text;

        public CharReader(SourceText text)
        {
            _text = text;
        }

        public void NextChar()
        {
            Position++;
        }

        public int Position { get; private set; }

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

        public char Peek(int offset)
        {
            var index = Position + offset;
            return index < _text.Length
                       ? _text[index]
                       : '\0';
        }
    }
}