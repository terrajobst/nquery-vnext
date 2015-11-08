using System;
using System.Collections.Generic;
using System.Linq;

using NQuery.Syntax;
using NQuery.Text;

namespace NQuery.Authoring.CodeActions.Fixes
{
    internal sealed class AddParenthesesCodeFixProvider : CodeFixProvider
    {
        public override IEnumerable<DiagnosticId> GetFixableDiagnosticIds()
        {
            return new[]
            {
                DiagnosticId.InvocationRequiresParenthesis
            };
        }

        protected override IEnumerable<ICodeAction> GetFixes(SemanticModel semanticModel, int position, Diagnostic diagnostic)
        {
            var root = semanticModel.SyntaxTree.Root;
            var expression = root.DescendantNodes()
                                 .Where(n => n.Span.ContainsOrTouches(position))
                                 .FirstOrDefault(n => n is NameExpressionSyntax || n is PropertyAccessExpressionSyntax);

            if (expression == null)
                return Enumerable.Empty<ICodeAction>();

            return new[] {new AddParenthesesCodeAction(expression)};
        }

        private sealed class AddParenthesesCodeAction : CodeAction
        {
            private readonly SyntaxNode _node;

            public AddParenthesesCodeAction(SyntaxNode node)
                : base(node.SyntaxTree)
            {
                _node = node;
            }

            public override string Description
            {
                get { return "Add parentheses"; }
            }

            protected override void GetChanges(TextChangeSet changeSet)
            {
                var position = _node.Span.End;
                changeSet.InsertText(position, "()");
            }
        }
    }
}