using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace NQuery.Syntax
{
    public sealed class CaseExpressionSyntax : ExpressionSyntax
    {
        internal CaseExpressionSyntax(SyntaxTree syntaxTree, SyntaxToken caseKeyword, ExpressionSyntax inputExpression, IEnumerable<CaseLabelSyntax> caseLabels, CaseElseLabelSyntax elseLabel, SyntaxToken endKeyword)
            : base(syntaxTree)
        {
            CaseKeyword = caseKeyword;
            InputExpression = inputExpression;
            CaseLabels = caseLabels.ToImmutableArray();
            ElseLabel = elseLabel;
            EndKeyword = endKeyword;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.CaseExpression; }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
        {
            yield return CaseKeyword;

            if (InputExpression != null)
                yield return InputExpression;

            foreach (var caseLabel in CaseLabels)
                yield return caseLabel;

            if (ElseLabel != null)
                yield return ElseLabel;

            yield return EndKeyword;
        }

        public SyntaxToken CaseKeyword { get; }

        public ExpressionSyntax InputExpression { get; }

        public ImmutableArray<CaseLabelSyntax> CaseLabels { get; }

        public CaseElseLabelSyntax ElseLabel { get; }

        public SyntaxToken EndKeyword { get; }
    }
}