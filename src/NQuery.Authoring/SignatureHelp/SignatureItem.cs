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
            return other is not null &&
                   Content == other.Content &&
                   Parameters.SequenceEqual(other.Parameters);
        }

        public override bool Equals(object obj)
        {
            var other = obj as SignatureItem;
            return other is not null && Equals(other);
        }

        public override int GetHashCode()
        {
            var result = new HashCode();
            result.Add(Content);

            foreach (var p in Parameters)
                result.Add(p);

            return result.ToHashCode();
        }
    }
}