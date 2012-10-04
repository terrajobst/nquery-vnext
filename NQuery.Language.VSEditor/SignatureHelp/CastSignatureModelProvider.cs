using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;

namespace NQuery.Language.VSEditor.SignatureHelp
{
    [Export(typeof(ISignatureModelProvider))]
    internal sealed class CastSignatureModelProvider : ISignatureModelProvider
    {
        public SignatureHelpModel GetModel(SemanticModel semanticModel, int position)
        {
            var syntaxTree = semanticModel.Compilation.SyntaxTree;
            var token = syntaxTree.Root.FindTokenTouched(position, descendIntoTrivia: true);
            var castExpression = token.Parent
                                      .AncestorsAndSelf()
                                      .OfType<CastExpressionSyntax>()
                                      .FirstOrDefault(c => c.IsBetweenParentheses(position));

            if (castExpression == null)
                return null;

            var span = castExpression.Span;
            var signatures = new[] { GetSignatureItem() };

            var selected = signatures.FirstOrDefault();
            var asKeyword = castExpression.AsKeyword;
            var isBeforeAsKeyword = asKeyword.IsMissing || position <= asKeyword.Span.Start;
            var parameterIndex = isBeforeAsKeyword ? 0 : 1;

            return new SignatureHelpModel(span, signatures, selected, parameterIndex);
        }

        private static SignatureItem GetSignatureItem()
        {
            var parameters = new List<ParameterItem>();
            var sb = new StringBuilder();

            sb.Append("CAST(");

            var p1Start = sb.Length;
            sb.Append("expression");
            var p1End = sb.Length;

            sb.Append(" AS ");

            var p2Start = sb.Length;
            sb.Append("dataType");
            var p2End = sb.Length;

            sb.Append(")");

            parameters.Add(new ParameterItem("expression", "expression of any type", TextSpan.FromBounds(p1Start, p1End)));
            parameters.Add(new ParameterItem("dataType", "the type the epression is converted to", TextSpan.FromBounds(p2Start, p2End)));

            var content = sb.ToString();

            return new SignatureItem(content, "Converts an expression of one data type to another", parameters);
        }
    }
}