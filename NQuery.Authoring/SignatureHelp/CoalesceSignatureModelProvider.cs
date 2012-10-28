using System;
using System.ComponentModel.Composition;
using System.Linq;

namespace NQuery.Language.Services.SignatureHelp
{
    [Export(typeof(ISignatureModelProvider))]
    internal sealed class CoalesceSignatureModelProvider : ISignatureModelProvider
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