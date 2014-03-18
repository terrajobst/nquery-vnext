using System;

using NQuery.Text;

namespace NQuery.Authoring.SignatureHelp
{
    public sealed class ParameterItem : IEquatable<ParameterItem>
    {
        private readonly string _name;
        private readonly TextSpan _span;

        public ParameterItem(string name, TextSpan span)
        {
            _name = name;
            _span = span;
        }

        public string Name
        {
            get { return _name; }
        }

        public TextSpan Span
        {
            get { return _span; }
        }

        public bool Equals(ParameterItem other)
        {
            return other != null &&
                   _name == other.Name &&
                   _span == other.Span;
        }

        public override bool Equals(object obj)
        {
            var other = obj as ParameterItem;
            return other != null && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (_name != null ? _name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ _span.GetHashCode();
                return hashCode;
            }
        }
    }
}