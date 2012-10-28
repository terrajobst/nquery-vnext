using System;
using System.ComponentModel.Composition;
using System.Linq;

namespace NQuery.Authoring.SignatureHelp
{
    [Export(typeof(ISignatureModelProvider))]
    internal sealed class CastSignatureModelProvider : ISignatureModelProvider
    {
        public SignatureHelpModel GetModel(SemanticModel semanticModel, int position)
        {
            var syntaxTree = semanticModel.Compilation.SyntaxTree;
            var token = syntaxTree.Root.FindTokenOnLeft(position);
            var castExpression = token.Parent
                                      .AncestorsAndSelf()
                                      .OfType<CastExpressionSyntax>()
                                      .FirstOrDefault(c => c.IsBetweenParentheses(position));

            if (castExpression == null)
                return null;

            var span = castExpression.Span;
            var signatures = new[] { SignatureHelpExtensions.GetCastSignatureItem() };

            var selected = signatures.FirstOrDefault();
            var asKeyword = castExpression.AsKeyword;
            var isBeforeAsKeyword = asKeyword.IsMissing || position <= asKeyword.Span.Start;
            var parameterIndex = isBeforeAsKeyword ? 0 : 1;

            return new SignatureHelpModel(span, signatures, selected, parameterIndex);
        }
    }
}