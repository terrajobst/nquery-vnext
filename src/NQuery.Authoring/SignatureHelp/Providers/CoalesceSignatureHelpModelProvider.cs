using System;
using System.Linq;

using NQuery.Syntax;

namespace NQuery.Authoring.SignatureHelp
{
    internal sealed class CoalesceSignatureHelpModelProvider : ISignatureHelpModelProvider
    {
        public SignatureHelpModel GetModel(SemanticModel semanticModel, int position)
        {
            var syntaxTree = semanticModel.Compilation.SyntaxTree;
            var token = syntaxTree.Root.FindTokenOnLeft(position);
            var coalesceExpression = token.Parent
                                          .AncestorsAndSelf()
                                          .OfType<CoalesceExpressionSyntax>()
                                          .FirstOrDefault(c => c.IsBetweenParentheses(position));

            if (coalesceExpression == null)
                return null;

            var span = coalesceExpression.Span;
            var signatures = new[] { SignatureHelpExtensions.GetCoalesceSignatureItem() };

            var selected = signatures.FirstOrDefault();
            var parameterIndex = coalesceExpression.ArgumentList.GetParameterIndex(position);

            return new SignatureHelpModel(span, signatures, selected, parameterIndex);
        }
    }
}