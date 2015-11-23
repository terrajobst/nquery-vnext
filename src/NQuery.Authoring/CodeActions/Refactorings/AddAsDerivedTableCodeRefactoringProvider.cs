using System;
using System.Collections.Generic;
using System.Linq;

using NQuery.Syntax;
using NQuery.Text;

namespace NQuery.Authoring.CodeActions.Refactorings
{
    internal sealed class AddAsDerivedTableCodeRefactoringProvider : CodeRefactoringProvider<DerivedTableReferenceSyntax>
    {
        protected override IEnumerable<ICodeAction> GetRefactorings(SemanticModel semanticModel, int position, DerivedTableReferenceSyntax node)
        {
            return node.AsKeyword == null
                ? new[] { new AddAsToDerivedTableCodeAction(node) }
                : Enumerable.Empty<ICodeAction>();
        }

        private sealed class AddAsToDerivedTableCodeAction : CodeAction
        {
            private readonly DerivedTableReferenceSyntax _node;

            public AddAsToDerivedTableCodeAction(DerivedTableReferenceSyntax node)
                : base(node.SyntaxTree)
            {
                _node = node;
            }

            public override string Description
            {
                get { return Resources.CodeActionAddAsKeyword; }
            }

            protected override void GetChanges(TextChangeSet changeSet)
            {
                changeSet.InsertText(_node.Name.Span.Start, @"AS ");
            }
        }
    }
}