using System;
using System.Collections.Generic;

namespace NQuery.Language
{
    public sealed class CastExpressionSyntax : ExpressionSyntax
    {
        private readonly SyntaxToken _castKeyword;
        private readonly SyntaxToken _leftParenthesisToken;
        private readonly ExpressionSyntax _expression;
        private readonly SyntaxToken _asKeyword;
        private readonly TypeReferenceSyntax _typeReference;
        private readonly SyntaxToken _rightParenthesisToken;

        public CastExpressionSyntax(SyntaxTree syntaxTree, SyntaxToken castKeyword, SyntaxToken leftParenthesisToken, ExpressionSyntax expression, SyntaxToken asKeyword, TypeReferenceSyntax typeReference, SyntaxToken rightParenthesisToken)
            : base(syntaxTree)
        {
            _castKeyword = castKeyword.WithParent(this);
            _leftParenthesisToken = leftParenthesisToken.WithParent(this);
            _expression = expression;
            _asKeyword = asKeyword.WithParent(this);
            _typeReference = typeReference;
            _rightParenthesisToken = rightParenthesisToken.WithParent(this);
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.CastExpression; }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
        {
            yield return _castKeyword;
            yield return _leftParenthesisToken;
            yield return _expression;
            yield return _asKeyword;
            yield return _typeReference;
            yield return _rightParenthesisToken;
        }

        public SyntaxToken CastKeyword
        {
            get { return _castKeyword; }
        }

        public SyntaxToken LeftParenthesisToken
        {
            get { return _leftParenthesisToken; }
        }

        public ExpressionSyntax Expression
        {
            get { return _expression; }
        }

        public SyntaxToken AsKeyword
        {
            get { return _asKeyword; }
        }

        public TypeReferenceSyntax TypeReference
        {
            get { return _typeReference; }
        }

        public SyntaxToken RightParenthesisToken
        {
            get { return _rightParenthesisToken; }
        }
    }
}