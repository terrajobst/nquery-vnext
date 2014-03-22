using System;
using System.Linq;

using NQuery.Symbols;
using NQuery.Syntax;

namespace NQuery.Authoring.SignatureHelp.Providers
{
    internal sealed class CountAllSignatureHelpModelProvider : SignatureHelpModelProvider<CountAllExpressionSyntax>
    {
        protected override SignatureHelpModel GetModel(SemanticModel semanticModel, CountAllExpressionSyntax node, int position)
        {
            // TODO: We need to use the resolved symbol as the currently selected one.

            var name = node.Name;
            var signatures = semanticModel.LookupSymbols(name.Span.Start)
                                          .OfType<AggregateSymbol>()
                                          .Where(f => name.Matches(f.Name))
                                          .ToSignatureItems()
                                          .ToArray();

            if (signatures.Length == 0)
                return null;

            var span = node.Span;
            var selected = signatures.FirstOrDefault();

            return new SignatureHelpModel(span, signatures, selected, 0);
        }
    }
}