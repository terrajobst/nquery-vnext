using System.Collections.Immutable;

using NQuery.CodeAnalysis;
using NQuery.CodeAnalysis.Symbols;
using NQuery.CodeAnalysis.Syntax;

namespace NQuery.Authoring.SignatureHelp.Providers;

internal sealed class FunctionSignatureHelpProvider : SignatureHelpProvider<FunctionInvocationExpressionSyntax>
{
    protected override SignatureHelpResult? GetResult(SemanticModel semanticModel, FunctionInvocationExpressionSyntax node, int position)
    {
        // TODO: We need to use the resolved symbol as the currently selected one.

        var name = node.IdentifierToken;
        var functionSignatures = semanticModel.LookupSymbols(name.Span.Start)
                                              .OfType<FunctionSymbol>()
                                              .Where(f => name.Matches(f.Name))
                                              .ToSignatureItems();

        var aggregateSignatures = semanticModel.LookupSymbols(name.Span.Start)
                                               .OfType<AggregateSymbol>()
                                               .Where(f => name.Matches(f.Name))
                                               .ToSignatureItems();

        var signatures = functionSignatures.Concat(aggregateSignatures).OrderBy(s => s.Parameters.Length).ToImmutableArray();

        if (signatures.Length == 0)
            return null;

        var span = node.Span;
        var parameterIndex = node.ArgumentList.GetParameterIndex(position);
        var selected = signatures.FirstOrDefault(s => s.Parameters.Length > parameterIndex);

        return new SignatureHelpResult(span, signatures, selected!, parameterIndex);
    }
}
