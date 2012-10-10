using System.ComponentModel.Composition;
using System.Linq;

namespace NQuery.Language.VSEditor.SignatureHelp
{
    [Export(typeof(ISignatureModelProvider))]
    internal sealed class NullIfSignatureModelProvider : ISignatureModelProvider
    {
        public SignatureHelpModel GetModel(SemanticModel semanticModel, int position)
        {
            var syntaxTree = semanticModel.Compilation.SyntaxTree;
            var token = syntaxTree.Root.FindTokenOnLeft(position);
            var nullIfExpression = token.Parent
                                        .AncestorsAndSelf()
                                        .OfType<NullIfExpressionSyntax>()
                                        .FirstOrDefault(n => n.IsBetweenParentheses(position));

            if (nullIfExpression == null)
                return null;

            var span = nullIfExpression.Span;
            var signatures = new[] { SignatureHelpExtensions.GetNullIfSignatureItem() };

            var selected = signatures.FirstOrDefault();
            var commaToken = nullIfExpression.CommaToken;
            var isBeforeComma = commaToken.IsMissing || position <= commaToken.Span.Start;
            var parameterIndex = isBeforeComma ? 0 : 1;

            return new SignatureHelpModel(span, signatures, selected, parameterIndex);
        }
    }
}