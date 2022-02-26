using System.Collections.Immutable;

using NQuery.Text;

namespace NQuery.Authoring.CodeActions
{
    public sealed class CodeIssue
    {
        public CodeIssue(CodeIssueKind kind, TextSpan span, string description)
            : this(kind, span, description, Enumerable.Empty<ICodeAction>())
        {
        }

        public CodeIssue(CodeIssueKind kind, TextSpan span, IEnumerable<ICodeAction> actions)
            : this(kind, span, null, actions)
        {
        }

        public CodeIssue(CodeIssueKind kind, TextSpan span, string description, IEnumerable<ICodeAction> actions)
        {
            Kind = kind;
            Span = span;
            Description = description;
            Actions = actions.ToImmutableArray();
        }

        public CodeIssueKind Kind { get; }

        public TextSpan Span { get; }

        public string Description { get; }

        public ImmutableArray<ICodeAction> Actions { get; }
    }
}