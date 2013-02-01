using System;
using System.Collections.Generic;

namespace NQuery.Syntax
{
    public sealed class CaseElseLabelSyntax : SyntaxNode
    {
        private readonly SyntaxToken _elseKeyword;
        private readonly ExpressionSyntax _expression;

        public CaseElseLabelSyntax(SyntaxTree syntaxTree, SyntaxToken elseKeyword, ExpressionSyntax expression)
            : base(syntaxTree)
        {
            _elseKeyword = elseKeyword;
            _expression = expression;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.CaseElseLabel; }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
        {
            yield return _elseKeyword;
            yield return _expression;
        }

        public SyntaxToken ElseKeyword
        {
            get { return _elseKeyword; }
        }

        public ExpressionSyntax Expression
        {
            get { return _expression; }
        }
    }
}