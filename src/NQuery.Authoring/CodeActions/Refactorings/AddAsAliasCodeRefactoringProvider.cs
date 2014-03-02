using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

using NQuery.Syntax;

namespace NQuery.Authoring.CodeActions
{
    [Export(typeof(ICodeRefactoringProvider))]
    internal sealed class AddAsAliasCodeRefactoringProvider : CodeRefactoringProvider<AliasSyntax>
    {
        protected override IEnumerable<ICodeAction> GetRefactorings(SemanticModel semanticModel, int position, AliasSyntax node)
        {
            return node.AsKeyword == null
                ? new[] { new AddAsToAliasCodeAction(node) }
                : Enumerable.Empty<ICodeAction>();
        }

        private sealed class AddAsToAliasCodeAction : ICodeAction
        {
            private readonly AliasSyntax _node;

            public AddAsToAliasCodeAction(AliasSyntax node)
            {
                _node = node;
            }

            public string Description
            {
                get { return string.Format("Add 'AS' keyword"); }
            }

            public SyntaxTree GetEdit()
            {
                return _node.SyntaxTree.InsertText(_node.Span.Start, "AS ");
            }
        }
    }
}