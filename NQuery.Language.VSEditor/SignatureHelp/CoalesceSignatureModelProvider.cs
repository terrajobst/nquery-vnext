using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;

namespace NQuery.Language.VSEditor.SignatureHelp
{
    [Export(typeof(ISignatureModelProvider))]
    internal sealed class CoalesceSignatureModelProvider : ISignatureModelProvider
    {
        public SignatureHelpModel GetModel(SemanticModel semanticModel, int position)
        {
            var syntaxTree = semanticModel.Compilation.SyntaxTree;
            var token = syntaxTree.Root.FindTokenTouched(position, descendIntoTrivia: true);
            var coalesceExpression = token.Parent
                                          .AncestorsAndSelf()
                                          .OfType<CoalesceExpressionSyntax>()
                                          .FirstOrDefault(c => c.ArgumentList.LeftParenthesis.Span.Start < position);

            if (coalesceExpression == null)
                return null;

            var span = coalesceExpression.Span;
            var signatures = new[] { GetSignatureItem() };

            var selected = signatures.FirstOrDefault();
            var parameterIndex = GetParameterIndex(coalesceExpression.ArgumentList, position);

            return new SignatureHelpModel(span, signatures, selected, parameterIndex);
        }

        private static int GetParameterIndex(ArgumentListSyntax argumentList, int position)
        {
            var arguments = argumentList.Arguments;
            return arguments.TakeWhile(a => a.FullSpan.End < position).Count();
        }

        private static SignatureItem GetSignatureItem()
        {
            var parameters = new List<ParameterItem>();
            var sb = new StringBuilder();

            sb.Append("COALESCE(");

            var p1Start = sb.Length;
            sb.Append("expression1");
            var p1End = sb.Length;

            sb.Append(", ");

            var p2Start = sb.Length;
            sb.Append("expression2");
            var p2End = sb.Length;

            sb.Append(" ");

            var pNStart = sb.Length;
            sb.Append("[, ...]");
            var pNEnd = sb.Length;

            sb.Append(")");

            parameters.Add(new ParameterItem("expression1", "expression of any type", TextSpan.FromBounds(p1Start, p1End)));
            parameters.Add(new ParameterItem("expression2", "expression of any type", TextSpan.FromBounds(p2Start, p2End)));
            parameters.Add(new ParameterItem("expressionN", "expression of any type", TextSpan.FromBounds(pNStart, pNEnd)));

            var content = sb.ToString();

            return new SignatureItem(content, "Returns the first nonnull expression among its arguments.", parameters);
        }
    }
}