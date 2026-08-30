using System.Collections.Immutable;

using NQuery.CodeAnalysis;
using NQuery.CodeAnalysis.Symbols;
using NQuery.CodeAnalysis.Syntax;

namespace NQuery.Authoring.SignatureHelp.Providers;

internal sealed class CountAllSignatureHelpProvider : SignatureHelpProvider<CountAllExpressionSyntax>
{
    protected override SignatureHelpResult? GetResult(SemanticModel semanticModel, CountAllExpressionSyntax node, int position)
    {
        // TODO: We need to use the resolved symbol as the currently selected one.

        var name = node.IdentifierToken;
        var signatures = semanticModel.LookupSymbols(name.Span.Start)
                                      .OfType<AggregateSymbol>()
                                      .Where(f => name.Matches(f.Name))
                                      .ToSignatureItems()
                                      .ToImmutableArray();

        if (signatures.Length == 0)
            return null;

        var span = node.Span;
        var selected = signatures.FirstOrDefault();

        return new SignatureHelpResult(span, signatures, selected!, 0);
    }
}
