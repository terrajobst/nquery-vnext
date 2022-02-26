using System.Collections.Immutable;

namespace NQuery.Authoring.SignatureHelp
{
    public sealed class SignatureItem : IEquatable<SignatureItem>
    {
        public SignatureItem(string content, IEnumerable<ParameterItem> parameters)
        {
            Content = content;
            Parameters = parameters.ToImmutableArray();
        }

        public string Content { get; }

        public ImmutableArray<ParameterItem> Parameters { get; }

        public bool Equals(SignatureItem other)
        {
            return other != null &&
                   Content == other.Content &&
                   Parameters.SequenceEqual(other.Parameters);
        }

        public override bool Equals(object obj)
        {
            var other = obj as SignatureItem;
            return other != null && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Content.GetHashCode();
                hashCode = (hashCode*397) ^ Parameters.GetHashCode();
                return hashCode;
            }
        }
    }
}