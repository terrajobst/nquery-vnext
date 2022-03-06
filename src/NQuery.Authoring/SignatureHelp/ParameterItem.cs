using NQuery.Text;

namespace NQuery.Authoring.SignatureHelp
{
    public sealed class ParameterItem : IEquatable<ParameterItem>
    {
        public ParameterItem(string name, TextSpan span)
        {
            Name = name;
            Span = span;
        }

        public string Name { get; }

        public TextSpan Span { get; }

        public bool Equals(ParameterItem other)
        {
            return other is not null &&
                   Name == other.Name &&
                   Span == other.Span;
        }

        public override bool Equals(object obj)
        {
            return obj is ParameterItem other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, Span);
        }
    }
}