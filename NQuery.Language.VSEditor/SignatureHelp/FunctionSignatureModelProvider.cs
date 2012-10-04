using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using NQuery.Language.Symbols;

namespace NQuery.Language.VSEditor.SignatureHelp
{
    // TODO: Could we introduce a common base class between FunctionSignatureModelProvider and MethodSignatureModelProvider?

    [Export(typeof(ISignatureModelProvider))]
    internal sealed class FunctionSignatureModelProvider : ISignatureModelProvider
    {
        public SignatureHelpModel GetModel(SemanticModel semanticModel, int position)
        {
            var syntaxTree = semanticModel.Compilation.SyntaxTree;
            var token = syntaxTree.Root.FindTokenTouched(position, descendIntoTrivia: true);
            var functionInvocation = token.Parent
                                          .AncestorsAndSelf()
                                          .OfType<FunctionInvocationExpressionSyntax>()
                                          .FirstOrDefault(f => f.IsBetweenParentheses(position));

            if (functionInvocation == null)
                return null;

            // TODO: We need to use the resolved symbol as the currently selected one.

            var name = functionInvocation.Name;
            var functionSymbols = semanticModel.LookupSymbols(name.Span.Start)
                                               .OfType<FunctionSymbol>()
                                               .Where(f => name.Matches(f.Name));
            var functionSignatures = ToSignatureItems(functionSymbols);

            var aggregateSymbols = semanticModel.LookupSymbols(name.Span.Start)
                                                .OfType<AggregateSymbol>()
                                                .Where(f => name.Matches(f.Name));
            var aggregateSignatures = ToSignatureItems(aggregateSymbols);

            var signatures = functionSignatures.Concat(aggregateSignatures).OrderBy(s => s.Parameters.Count).ToArray();

            if (signatures.Length == 0)
                return null;

            var span = functionInvocation.Span;
            var selected = signatures.FirstOrDefault();
            var parameterIndex = functionInvocation.ArgumentList.GetParameterIndex(position);

            return new SignatureHelpModel(span, signatures, selected, parameterIndex);
        }

        private static IEnumerable<SignatureItem> ToSignatureItems(IEnumerable<FunctionSymbol> symbols)
        {
            return symbols.Select(ToSignatureItem);
        }

        private static IEnumerable<SignatureItem> ToSignatureItems(IEnumerable<AggregateSymbol> symbols)
        {
            return symbols.Select(ToSignatureItem);
        }

        private static SignatureItem ToSignatureItem(FunctionSymbol symbol)
        {
            var parameters = new List<ParameterItem>();
            var sb = new StringBuilder();

            sb.Append(symbol.Name);
            sb.Append("(");

            var isFirst = true;
            foreach (var parameter in symbol.Parameters)
            {
                if (isFirst)
                    isFirst = false;
                else
                    sb.Append(", ");

                var start = sb.Length;

                sb.Append(parameter.Name);
                sb.Append(" AS ");
                sb.Append(parameter.Type.Name.ToUpper());

                var end = sb.Length;
                var span = TextSpan.FromBounds(start, end);
                parameters.Add(new ParameterItem(parameter.Name, "Docs for " + parameter.Name, span));
            }

            sb.Append(")");
            sb.Append(" AS ");
            sb.Append(symbol.Type.Name.ToUpper());

            var content = sb.ToString();

            return new SignatureItem(content, "Docs for " + symbol.Name, parameters);
        }

        private static SignatureItem ToSignatureItem(AggregateSymbol symbol)
        {
            var parameters = new List<ParameterItem>();
            var sb = new StringBuilder();

            sb.Append("AGGREGATE ");
            sb.Append(symbol.Name);
            sb.Append("(");

            var p1Start = sb.Length;
            sb.Append("expression");
            var p1End = sb.Length;

            sb.Append(")");

            parameters.Add(new ParameterItem("expression", null, TextSpan.FromBounds(p1Start, p1End)));

            var content = sb.ToString();

            return new SignatureItem(content, "Docs for " + symbol.Name, parameters);
        }
    }
}