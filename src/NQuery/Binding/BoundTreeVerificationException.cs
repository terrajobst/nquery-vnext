using System.Collections.Immutable;

namespace NQuery.Binding
{
    internal sealed class BoundTreeVerificationException : Exception
    {
        public BoundTreeVerificationException(IEnumerable<string> problems)
            : base(FormatMessage(problems, out var captured))
        {
            Problems = captured;
        }

        public ImmutableArray<string> Problems { get; }

        private static string FormatMessage(IEnumerable<string> problems, out ImmutableArray<string> captured)
        {
            captured = problems.ToImmutableArray();
            return $"The bound tree is invalid:{Environment.NewLine}{string.Join(Environment.NewLine, captured.Select(p => "  - " + p))}";
        }
    }
}
