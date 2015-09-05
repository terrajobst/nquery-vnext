using System;
using System.Collections.Generic;
using System.Linq;

using NQuery.Syntax;
using NQuery.Text;

namespace NQuery.Authoring.CodeActions.Refactorings
{
    internal sealed class AddAsAliasCodeRefactoringProvider : CodeRefactoringProvider<AliasSyntax>
    {
        protected override IEnumerable<ICodeAction> GetRefactorings(SemanticModel semanticModel, int position, AliasSyntax node)
        {
            return node.AsKeyword == null
                ? new[] { new AddAsToAliasCodeAction(node) }
                : Enumerable.Empty<ICodeAction>();
        }

        private sealed class AddAsToAliasCodeAction : CodeAction
        {
            private readonly AliasSyntax _node;

            public AddAsToAliasCodeAction(AliasSyntax node)
                : base(node.SyntaxTree)
            {
                _node = node;
            }

            public override string Description
            {
                get { return "Add 'AS' keyword"; }
            }

            protected override void GetChanges(TextChangeSet changeSet)
            {
                changeSet.InsertText(_node.Span.Start, "AS ");
            }
        }
    }
}