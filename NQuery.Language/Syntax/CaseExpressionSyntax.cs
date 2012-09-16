using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace NQuery.Language
{
    public sealed class CaseExpressionSyntax : ExpressionSyntax
    {
        private readonly SyntaxToken _caseKeyword;
        private readonly ExpressionSyntax _inputExpression;
        private readonly ReadOnlyCollection<CaseLabelSyntax> _caseLabels;
        private readonly CaseElseLabelSyntax _elseLabel;
        private readonly SyntaxToken _endKeyword;

        public CaseExpressionSyntax(SyntaxTree syntaxTree, SyntaxToken caseKeyword, ExpressionSyntax inputExpression, IList<CaseLabelSyntax> caseLabels, CaseElseLabelSyntax elseLabel, SyntaxToken endKeyword)
            : base(syntaxTree)
        {
            _caseKeyword = caseKeyword.WithParent(this);
            _inputExpression = inputExpression;
            _caseLabels = new ReadOnlyCollection<CaseLabelSyntax>(caseLabels);
            _elseLabel = elseLabel;
            _endKeyword = endKeyword.WithParent(this);
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

        public ReadOnlyCollection<CaseLabelSyntax> CaseLabels
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