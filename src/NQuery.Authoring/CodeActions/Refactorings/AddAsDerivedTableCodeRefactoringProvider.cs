using System;
using System.Collections.Generic;
using System.Linq;

using NQuery.Syntax;

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

        private sealed class AddAsToDerivedTableCodeAction : ICodeAction
        {
            private readonly DerivedTableReferenceSyntax _node;

            public AddAsToDerivedTableCodeAction(DerivedTableReferenceSyntax node)
            {
                _node = node;
            }

            public string Description
            {
                get { return "Add 'AS' keyword"; }
            }

            public SyntaxTree GetEdit()
            {
                return _node.SyntaxTree.InsertText(_node.Name.Span.Start, "AS ");
            }
        }
    }
}