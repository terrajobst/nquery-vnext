using System;
using System.Linq;

using NQuery.Syntax;

namespace NQuery.Authoring.SignatureHelp
{
    internal sealed class MethodSignatureHelpModelProvider : SignatureHelpModelProvider<MethodInvocationExpressionSyntax>
    {
        protected override SignatureHelpModel GetModel(SemanticModel semanticModel, MethodInvocationExpressionSyntax node, int position)
        {
            // TODO: We need to use the resolved symbol as the selected one.

            var targetType = semanticModel.GetExpressionType(node.Target);
            var name = node.Name;
            var signatures = semanticModel.LookupMethods(targetType)
                                          .Where(m => name.Matches(m.Name))
                                          .OrderBy(f => f.Parameters.Count)
                                          .ToSignatureItems()
                                          .ToArray();
            if (signatures.Length == 0)
                return null;

            var span = node.Span;
            var parameterIndex = node.ArgumentList.GetParameterIndex(position);
            var selected = signatures.FirstOrDefault(s => s.Parameters.Count > parameterIndex);

            return new SignatureHelpModel(span, signatures, selected, parameterIndex);
        }
    }
}