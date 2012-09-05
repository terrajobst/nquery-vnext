using System;
using System.Collections.Generic;

namespace NQuery.Language
{
    public sealed class InExpressionSyntax : ExpressionSyntax
    {
        private readonly ExpressionSyntax _expression;
        private readonly SyntaxToken? _notKeyword;
        private readonly SyntaxToken _inKeyword;
        private readonly ArgumentListSyntax _argumentList;

        public InExpressionSyntax(SyntaxTree syntaxTree, ExpressionSyntax expression, SyntaxToken? notKeyword, SyntaxToken inKeyword, ArgumentListSyntax argumentList)
            : base(syntaxTree)
        {
            _expression = expression;
            _notKeyword = notKeyword.WithParent(this);
            _inKeyword = inKeyword.WithParent(this);
            _argumentList = argumentList;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.InExpression; }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
        {
            yield return _expression;
            if (_notKeyword != null)
                yield return _notKeyword.Value;
            yield return _inKeyword;
            yield return _argumentList;
        }

        public ExpressionSyntax Expression
        {
            get { return _expression; }
        }

        public SyntaxToken? NotKeyword
        {
            get { return _notKeyword; }
        }

        public SyntaxToken InKeyword
        {
            get { return _inKeyword; }
        }

        public ArgumentListSyntax ArgumentList
        {
            get { return _argumentList; }
        }
    }
}