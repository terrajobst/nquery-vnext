using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using NQuery.Language.Symbols;

namespace NQuery.Language.VSEditor.SignatureHelp
{
    [Export(typeof(ISignatureModelProvider))]
    internal sealed class MethodSignatureModelProvider : ISignatureModelProvider
    {
        public SignatureHelpModel GetModel(SemanticModel semanticModel, int position)
        {
            var syntaxTree = semanticModel.Compilation.SyntaxTree;
            var token = syntaxTree.Root.FindTokenTouched(position, descendIntoTrivia: true);
            var methodInvocation = token.Parent
                                        .AncestorsAndSelf()
                                        .OfType<MethodInvocationExpressionSyntax>()
                                        .FirstOrDefault(m => m.ArgumentList.LeftParenthesis.Span.Start < position);

            if (methodInvocation == null)
                return null;

            // TODO: We need to use the resolved symbol as the selected one.

            var targetType = semanticModel.GetExpressionType(methodInvocation.Target);
            var name = methodInvocation.Name;
            var symbols = semanticModel.LookupMethods(targetType)
                                       .Where(m => name.Matches(m.Name))
                                       .OrderBy(f => f.Parameters.Count);
            var signatures = ToSignatureItems(symbols).ToArray();
            if (signatures.Length == 0)
                return null;

            var span = methodInvocation.Span;
            var selected = signatures.FirstOrDefault();
            var parameterIndex = methodInvocation.ArgumentList.GetParameterIndex(position);

            return new SignatureHelpModel(span, signatures, selected, parameterIndex);
        }

        private static IEnumerable<SignatureItem> ToSignatureItems(IEnumerable<MethodSymbol> functionSymbols)
        {
            return functionSymbols.Select(ToSignatureItem);
        }

        private static SignatureItem ToSignatureItem(MethodSymbol symbol)
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