using System.Collections.Immutable;

using NQuery.CodeAnalysis.Text;

namespace NQuery.Authoring.CodeActions;

public sealed class CodeIssue
{
    public CodeIssue(CodeIssueKind kind, TextSpan span, string description)
        : this(kind, span, description, [])
    {
    }

    public CodeIssue(CodeIssueKind kind, TextSpan span, IEnumerable<ICodeAction> actions)
        : this(kind, span, null, actions)
    {
    }

    public CodeIssue(CodeIssueKind kind, TextSpan span, string? description, IEnumerable<ICodeAction> actions)
    {
        ThrowIfNull(actions);

        Kind = kind;
        Span = span;
        Description = description;
        Actions = [.. actions];
    }

    public CodeIssueKind Kind { get; }

    public TextSpan Span { get; }

    public string? Description { get; }

    public ImmutableArray<ICodeAction> Actions { get; }
}
