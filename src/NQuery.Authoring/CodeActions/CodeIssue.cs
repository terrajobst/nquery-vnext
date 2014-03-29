using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using NQuery.Text;

namespace NQuery.Authoring.CodeActions
{
    public sealed class CodeIssue
    {
        private readonly CodeIssueKind _kind;
        private readonly TextSpan _span;
        private readonly string _description;
        private readonly ImmutableArray<ICodeAction> _actions;

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
            _kind = kind;
            _span = span;
            _description = description;
            _actions = actions.ToImmutableArray();
        }

        public CodeIssueKind Kind
        {
            get { return _kind; }
        }

        public TextSpan Span
        {
            get { return _span; }
        }

        public string Description
        {
            get { return _description; }
        }

        public ImmutableArray<ICodeAction> Actions
        {
            get { return _actions; }
        }
    }
}