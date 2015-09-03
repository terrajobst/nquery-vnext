using System;
using System.Collections.Generic;
using System.Linq;

using NQuery.Syntax;
using NQuery.Symbols;

namespace NQuery.Authoring.CodeActions.Refactorings
{
    internal sealed class QualifyColumnCodeRefactoringProvider : CodeRefactoringProvider<NameExpressionSyntax>
    {
        protected override IEnumerable<ICodeAction> GetRefactorings(SemanticModel semanticModel, int position, NameExpressionSyntax node)
        {
            if (node.Parent is PropertyAccessExpressionSyntax)
                return Enumerable.Empty<ICodeAction>();

            var column = semanticModel.GetSymbol(node) as TableColumnInstanceSymbol;
            if (column == null)
                return Enumerable.Empty<ICodeAction>();

            return new ICodeAction[] { new QualifyColumnCodeAction(node, column) };
        }

        private sealed class QualifyColumnCodeAction : ICodeAction
        {
            private readonly NameExpressionSyntax _node;
            private readonly TableColumnInstanceSymbol _symbol;

            public QualifyColumnCodeAction(NameExpressionSyntax node, TableColumnInstanceSymbol symbol)
            {
                _node = node;
                _symbol = symbol;
            }

            public string Description
            {
                get { return "Qualify column"; }
            }

            public SyntaxTree GetEdit()
            {
                var name = SyntaxFacts.GetValidIdentifier(_symbol.TableInstance.Name);
                return _node.SyntaxTree.InsertText(_node.Span.Start, name + ".");
            }
        }
    }
}
