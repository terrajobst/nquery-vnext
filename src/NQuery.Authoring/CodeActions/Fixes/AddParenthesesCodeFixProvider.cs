using System;
using System.Collections.Generic;
using System.Linq;

using NQuery.Syntax;

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
            var root = semanticModel.Compilation.SyntaxTree.Root;
            var expression = root.DescendantNodes()
                                 .Where(n => n.Span.ContainsOrTouches(position))
                                 .FirstOrDefault(n => n is NameExpressionSyntax || n is PropertyAccessExpressionSyntax);

            if (expression == null)
                return Enumerable.Empty<ICodeAction>();

            return new[] {new CodeAction(expression)};
        }

        private sealed class CodeAction : ICodeAction
        {
            private readonly SyntaxNode _node;

            public CodeAction(SyntaxNode node)
            {
                _node = node;
            }

            public string Description
            {
                get { return "Add parentheses"; }
            }

            public SyntaxTree GetEdit()
            {
                var syntaxTree = _node.SyntaxTree;
                var position = _node.Span.End;
                return syntaxTree.InsertText(position, "()");
            }
        }
    }
}