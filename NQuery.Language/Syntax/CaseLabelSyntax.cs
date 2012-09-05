using System;
using System.Collections.Generic;

namespace NQuery.Language
{
    public sealed class CaseLabelSyntax : SyntaxNode
    {
        private readonly SyntaxToken _whenKeyword;
        private readonly ExpressionSyntax _whenExpression;
        private readonly SyntaxToken _thenKeyword;
        private readonly ExpressionSyntax _thenExpression;

        public CaseLabelSyntax(SyntaxTree syntaxTree, SyntaxToken whenKeyword, ExpressionSyntax whenExpression, SyntaxToken thenKeyword, ExpressionSyntax thenExpression)
            : base(syntaxTree)
        {
            _whenKeyword = whenKeyword.WithParent(this);
            _whenExpression = whenExpression;
            _thenKeyword = thenKeyword.WithParent(this);
            _thenExpression = thenExpression;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.CaseLabel; }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
        {
            yield return _whenKeyword;
            yield return _whenExpression;
            yield return _thenKeyword;
            yield return _thenExpression;
        }

        public SyntaxToken WhenKeyword
        {
            get { return _whenKeyword; }
        }

        public ExpressionSyntax WhenExpression
        {
            get { return _whenExpression; }
        }

        public SyntaxToken ThenKeyword
        {
            get { return _thenKeyword; }
        }

        public ExpressionSyntax ThenExpression
        {
            get { return _thenExpression; }
        }
    }
}