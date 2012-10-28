using System;
using System.ComponentModel.Composition;
using System.Linq;

using NQuery.Language.Symbols;

namespace NQuery.Language.Services.SignatureHelp
{
    [Export(typeof (ISignatureModelProvider))]
    internal sealed class CountAllSignatureModelProvider : ISignatureModelProvider
    {
        public SignatureHelpModel GetModel(SemanticModel semanticModel, int position)
        {
            var syntaxTree = semanticModel.Compilation.SyntaxTree;
            var token = syntaxTree.Root.FindTokenOnLeft(position);
            var countAllExpression = token.Parent
                                          .AncestorsAndSelf()
                                          .OfType<CountAllExpressionSyntax>()
                                          .FirstOrDefault(f => f.IsBetweenParentheses(position));

            if (countAllExpression == null)
                return null;

            // TODO: We need to use the resolved symbol as the currently selected one.

            var name = countAllExpression.Name;
            var signatures = semanticModel.LookupSymbols(name.Span.Start)
                                          .OfType<AggregateSymbol>()
                                          .Where(f => name.Matches(f.Name))
                                          .ToSignatureItems()
                                          .ToArray();

            if (signatures.Length == 0)
                return null;

            var span = countAllExpression.Span;
            var selected = signatures.FirstOrDefault();

            return new SignatureHelpModel(span, signatures, selected, 0);
        }
    }
}