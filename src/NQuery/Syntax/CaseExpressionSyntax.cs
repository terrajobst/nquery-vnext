using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace NQuery.Syntax
{
    public sealed class CaseExpressionSyntax : ExpressionSyntax
    {
        private readonly SyntaxToken _caseKeyword;
        private readonly ExpressionSyntax _inputExpression;
        private readonly ImmutableArray<CaseLabelSyntax> _caseLabels;
        private readonly CaseElseLabelSyntax _elseLabel;
        private readonly SyntaxToken _endKeyword;

        internal CaseExpressionSyntax(SyntaxTree syntaxTree, SyntaxToken caseKeyword, ExpressionSyntax inputExpression, IEnumerable<CaseLabelSyntax> caseLabels, CaseElseLabelSyntax elseLabel, SyntaxToken endKeyword)
            : base(syntaxTree)
        {
            _caseKeyword = caseKeyword;
            _inputExpression = inputExpression;
            _caseLabels = caseLabels.ToImmutableArray();
            _elseLabel = elseLabel;
            _endKeyword = endKeyword;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.CaseExpression; }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
        {
            yield return _caseKeyword;

            if (_inputExpression != null)
                yield return _inputExpression;

            foreach (var caseLabel in _caseLabels)
                yield return caseLabel;

            if (_elseLabel != null)
                yield return _elseLabel;

            yield return _endKeyword;
        }

        public SyntaxToken CaseKeyword
        {
            get { return _caseKeyword; }
        }

        public ExpressionSyntax InputExpression
        {
            get { return _inputExpression; }
        }

        public ImmutableArray<CaseLabelSyntax> CaseLabels
        {
            get { return _caseLabels; }
        }

        public CaseElseLabelSyntax ElseLabel
        {
            get { return _elseLabel; }
        }

        public SyntaxToken EndKeyword
        {
            get { return _endKeyword; }
        }
    }
}