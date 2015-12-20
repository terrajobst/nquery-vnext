using System;

namespace NQuery.Tests
{
    public class MockedOperatorClass
    {
        private readonly string _text;

        public MockedOperatorClass(string text)
        {
            _text = text;
        }

        // Unary

        public static MockedOperatorClass operator +(MockedOperatorClass value)
        {
            return new MockedOperatorClass("+ " + value._text);
        }

        public static MockedOperatorClass operator -(MockedOperatorClass value)
        {
            return new MockedOperatorClass("- " + value._text);
        }

        public static MockedOperatorClass operator ~(MockedOperatorClass value)
        {
            return new MockedOperatorClass("~ " + value._text);
        }

        public static MockedOperatorClass operator !(MockedOperatorClass value)
        {
            return new MockedOperatorClass("NOT " + value._text);
        }

        // Binary

        public static MockedOperatorClass operator *(MockedOperatorClass left, MockedOperatorClass right)
        {
            return new MockedOperatorClass(left + " * " + right);
        }

        public static MockedOperatorClass operator /(MockedOperatorClass left, MockedOperatorClass right)
        {
            return new MockedOperatorClass(left + " / " + right);
        }

        public static MockedOperatorClass operator %(MockedOperatorClass left, MockedOperatorClass right)
        {
            return new MockedOperatorClass(left + " % " + right);
        }

        public static MockedOperatorClass operator +(MockedOperatorClass left, MockedOperatorClass right)
        {
            return new MockedOperatorClass(left + " + " + right);
        }

        public static MockedOperatorClass operator -(MockedOperatorClass left, MockedOperatorClass right)
        {
            return new MockedOperatorClass(left + " - " + right);
        }

        public static MockedOperatorClass operator ==(MockedOperatorClass left, MockedOperatorClass right)
        {
            return new MockedOperatorClass(left + " = " + right);
        }

        public static MockedOperatorClass operator !=(MockedOperatorClass left, MockedOperatorClass right)
        {
            return new MockedOperatorClass(left + " != " + right);
        }

        public static MockedOperatorClass operator <(MockedOperatorClass left, MockedOperatorClass right)
        {
            return new MockedOperatorClass(left + " < " + right);
        }

        public static MockedOperatorClass operator <=(MockedOperatorClass left, MockedOperatorClass right)
        {
            return new MockedOperatorClass(left + " <= " + right);
        }

        public static MockedOperatorClass operator >(MockedOperatorClass left, MockedOperatorClass right)
        {
            return new MockedOperatorClass(left + " > " + right);
        }

        public static MockedOperatorClass operator >=(MockedOperatorClass left, MockedOperatorClass right)
        {
            return new MockedOperatorClass(left + " >= " + right);
        }

        public static MockedOperatorClass operator ^(MockedOperatorClass left, MockedOperatorClass right)
        {
            return new MockedOperatorClass(left + " ^ " + right);
        }

        public static MockedOperatorClass operator &(MockedOperatorClass left, MockedOperatorClass right)
        {
            return new MockedOperatorClass(left + " & " + right);
        }

        public static MockedOperatorClass operator |(MockedOperatorClass left, MockedOperatorClass right)
        {
            return new MockedOperatorClass(left + " | " + right);
        }

        public static MockedOperatorClass operator <<(MockedOperatorClass left, int right)
        {
            return new MockedOperatorClass(left + " << " + right);
        }

        public static MockedOperatorClass operator >>(MockedOperatorClass left, int right)
        {
            return new MockedOperatorClass(left + " >> " + right);
        }

        public override bool Equals(object obj)
        {
            var other = obj as MockedOperatorClass;
            return !ReferenceEquals(other, null) &&
                   string.Equals(_text, other._text, StringComparison.Ordinal);
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