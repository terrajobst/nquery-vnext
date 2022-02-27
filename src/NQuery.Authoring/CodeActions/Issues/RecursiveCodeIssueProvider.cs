﻿using NQuery.Syntax;
using NQuery.Text;

namespace NQuery.Authoring.CodeActions.Issues
{
    internal sealed class RecursiveCodeIssueProvider : CodeIssueProvider<CommonTableExpressionSyntax>
    {
        protected override IEnumerable<CodeIssue> GetIssues(SemanticModel semanticModel, CommonTableExpressionSyntax node)
        {
            var isRecursive = IsRecursive(semanticModel, node);
            if (isRecursive && node.RecursiveKeyword is null)
            {
                return new[]
                {
                    new CodeIssue(CodeIssueKind.Warning, node.Name.Span, Resources.CodeIssueShouldSpecifyRecursive, new[]
                    {
                        new InsertMissingRecursiveKeywordCodeAction(node)
                    })
                };
            }

            if (!isRecursive && node.RecursiveKeyword is not null)
            {
                return new[]
                {
                    new CodeIssue(CodeIssueKind.Unnecessary, node.RecursiveKeyword.Span, Resources.CodeIssueRecursiveIsNotNeeded, new[]
                    {
                        new RemoveUnnecessaryRecursiveKeywordCodeAction(node.RecursiveKeyword),
                    })
                };
            }

            return Enumerable.Empty<CodeIssue>();
        }

        private static bool IsRecursive(SemanticModel semanticModel, CommonTableExpressionSyntax node)
        {
            var symbol = semanticModel.GetDeclaredSymbol(node);
            return node.DescendantNodes()
                .OfType<NamedTableReferenceSyntax>()
                .Select(semanticModel.GetDeclaredSymbol)
                .Any(t => t is not null && t.Table == symbol);
        }

        private sealed class RemoveUnnecessaryRecursiveKeywordCodeAction : CodeAction
        {
            private readonly SyntaxToken _recursiveKeyword;

            public RemoveUnnecessaryRecursiveKeywordCodeAction(SyntaxToken recursiveKeyword)
                : base(recursiveKeyword.Parent.SyntaxTree)
            {
                _recursiveKeyword = recursiveKeyword;
            }

            public override string Description
            {
                get { return Resources.CodeActionRemoveRecursive; }
            }

            protected override void GetChanges(TextChangeSet changeSet)
            {
                var start = _recursiveKeyword.Span.Start;
                var end = _recursiveKeyword.FullSpan.End;
                var span = TextSpan.FromBounds(start, end);
                changeSet.DeleteText(span);
            }
        }

        private sealed class InsertMissingRecursiveKeywordCodeAction : CodeAction
        {
            private readonly CommonTableExpressionSyntax _node;

            public InsertMissingRecursiveKeywordCodeAction(CommonTableExpressionSyntax node)
                : base(node.SyntaxTree)
            {
                _node = node;
            }

            public override string Description
            {
                get { return Resources.CodeActionAddRecursive; }
            }

            protected override void GetChanges(TextChangeSet changeSet)
            {
                changeSet.InsertText(_node.Name.Span.Start, @"RECURSIVE ");
            }
        }
    }
}