using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;

namespace NQuery.Language.VSEditor.SignatureHelp
{
    [Export(typeof(ISignatureModelProvider))]
    internal sealed class NullIfSignatureModelProvider : ISignatureModelProvider
    {
        public SignatureHelpModel GetModel(SemanticModel semanticModel, int position)
        {
            var syntaxTree = semanticModel.Compilation.SyntaxTree;
            var token = syntaxTree.Root.FindTokenTouched(position);
            var nullIfExpression = token.Parent
                                        .AncestorsAndSelf()
                                        .OfType<NullIfExpressionSyntax>()
                                        .FirstOrDefault(n => n.LeftParenthesisToken.Span.Start < position);

            if (nullIfExpression == null)
                return null;

            var span = nullIfExpression.Span;
            var signatures = new[] { GetSignatureItem() };

            var selected = signatures.FirstOrDefault();
            var parameterIndex = position <= nullIfExpression.CommaToken.Span.Start ? 0 : 1;

            return new SignatureHelpModel(span, signatures, selected, parameterIndex);
        }

        private static SignatureItem GetSignatureItem()
        {
            var parameters = new List<ParameterItem>();
            var sb = new StringBuilder();

            sb.Append("NULLIF(");

            var p1Start = sb.Length;
            sb.Append("expression1");
            var p1End = sb.Length;

            sb.Append(", ");

            var p2Start = sb.Length;
            sb.Append("expression2");
            var p2End = sb.Length;

            sb.Append(")");

            parameters.Add(new ParameterItem("expression1", "expression of any type", TextSpan.FromBounds(p1Start, p1End)));
            parameters.Add(new ParameterItem("expression2", "expression of any type", TextSpan.FromBounds(p2Start, p2End)));

            var content = sb.ToString();

            return new SignatureItem(content, "Returns a null value if the two specified expressions are equal.", parameters);
        }
    }
}