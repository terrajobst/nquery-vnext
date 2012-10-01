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
                                          .FirstOrDefault(f => f.ArgumentList.LeftParenthesis.Span.Start < position);

            if (functionInvocation == null)
                return null;

            var name = functionInvocation.Name;
            var span = functionInvocation.Span;

            // TODO: We shouldn't lookup the symbols this way.
            // TODO: This should also handle aggregates.
            // Instead, the semantic model should expose the overload resolution result.
            var functionSymbols = semanticModel.LookupSymbols(span.Start)
                                               .OfType<FunctionSymbol>()
                                               .Where(f => name.Matches(f.Name))
                                               .OrderBy(f => f.Parameters.Count);
            var signatures = ToSignatureItems(functionSymbols).ToArray();
            if (signatures.Length == 0)
                return null;

            var selected = signatures.FirstOrDefault();
            var parameterIndex = functionInvocation.ArgumentList.GetParameterIndex(position);

            return new SignatureHelpModel(span, signatures, selected, parameterIndex);
        }

        private static IEnumerable<SignatureItem> ToSignatureItems(IEnumerable<FunctionSymbol> functionSymbols)
        {
            return functionSymbols.Select(ToSignatureItem);
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
    }
}