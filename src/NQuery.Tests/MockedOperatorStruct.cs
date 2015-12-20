using System;

namespace NQuery.Tests
{
    public struct MockedOperatorStruct
    {
        private readonly string _text;

        public MockedOperatorStruct(string text)
        {
            _text = text;
        }

        // Unary

        public static MockedOperatorStruct operator +(MockedOperatorStruct value)
        {
            return new MockedOperatorStruct("+ " + value._text);
        }

        public static MockedOperatorStruct operator -(MockedOperatorStruct value)
        {
            return new MockedOperatorStruct("- " + value._text);
        }

        public static MockedOperatorStruct operator ~(MockedOperatorStruct value)
        {
            return new MockedOperatorStruct("~ " + value._text);
        }

        public static MockedOperatorStruct operator !(MockedOperatorStruct value)
        {
            return new MockedOperatorStruct("NOT " + value._text);
        }

        // Binary

        public static MockedOperatorStruct operator *(MockedOperatorStruct left, MockedOperatorStruct right)
        {
            return new MockedOperatorStruct(left + " * " + right);
        }

        public static MockedOperatorStruct operator /(MockedOperatorStruct left, MockedOperatorStruct right)
        {
            return new MockedOperatorStruct(left + " / " + right);
        }

        public static MockedOperatorStruct operator %(MockedOperatorStruct left, MockedOperatorStruct right)
        {
            return new MockedOperatorStruct(left + " % " + right);
        }

        public static MockedOperatorStruct operator +(MockedOperatorStruct left, MockedOperatorStruct right)
        {
            return new MockedOperatorStruct(left + " + " + right);
        }

        public static MockedOperatorStruct operator -(MockedOperatorStruct left, MockedOperatorStruct right)
        {
            return new MockedOperatorStruct(left + " - " + right);
        }

        public static MockedOperatorStruct operator ==(MockedOperatorStruct left, MockedOperatorStruct right)
        {
            return new MockedOperatorStruct(left + " = " + right);
        }

        public static MockedOperatorStruct operator !=(MockedOperatorStruct left, MockedOperatorStruct right)
        {
            return new MockedOperatorStruct(left + " != " + right);
        }

        public static MockedOperatorStruct operator <(MockedOperatorStruct left, MockedOperatorStruct right)
        {
            return new MockedOperatorStruct(left + " < " + right);
        }

        public static MockedOperatorStruct operator <=(MockedOperatorStruct left, MockedOperatorStruct right)
        {
            return new MockedOperatorStruct(left + " <= " + right);
        }

        public static MockedOperatorStruct operator >(MockedOperatorStruct left, MockedOperatorStruct right)
        {
            return new MockedOperatorStruct(left + " > " + right);
        }

        public static MockedOperatorStruct operator >=(MockedOperatorStruct left, MockedOperatorStruct right)
        {
            return new MockedOperatorStruct(left + " >= " + right);
        }

        public static MockedOperatorStruct operator ^(MockedOperatorStruct left, MockedOperatorStruct right)
        {
            return new MockedOperatorStruct(left + " ^ " + right);
        }

        public static MockedOperatorStruct operator &(MockedOperatorStruct left, MockedOperatorStruct right)
        {
            return new MockedOperatorStruct(left + " & " + right);
        }

        public static MockedOperatorStruct operator |(MockedOperatorStruct left, MockedOperatorStruct right)
        {
            return new MockedOperatorStruct(left + " | " + right);
        }

        public static MockedOperatorStruct operator <<(MockedOperatorStruct left, int right)
        {
            return new MockedOperatorStruct(left + " << " + right);
        }

        public static MockedOperatorStruct operator >>(MockedOperatorStruct left, int right)
        {
            return new MockedOperatorStruct(left + " >> " + right);
        }

        public override bool Equals(object obj)
        {
            var other = obj as MockedOperatorStruct?;
            return other != null && string.Equals(_text, other.Value._text, StringComparison.Ordinal);
        }

        public override int GetHashCode()
        {
            return _text.GetHashCode();
        }

        public override string ToString()
        {
            return _text;
        }
    }
}