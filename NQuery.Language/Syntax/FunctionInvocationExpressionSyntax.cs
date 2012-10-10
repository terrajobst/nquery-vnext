using System;
using System.Collections.Generic;

namespace NQuery.Language
{
    public sealed class FunctionInvocationExpressionSyntax : ExpressionSyntax
    {
        private readonly SyntaxToken _name;
        private readonly ArgumentListSyntax _argumentList;

        public FunctionInvocationExpressionSyntax(SyntaxTree syntaxTree, SyntaxToken name, ArgumentListSyntax argumentList)
            : base(syntaxTree)
        {
            _name = name;
            _argumentList = argumentList;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.FunctionInvocationExpression; }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
        {
            yield return _name;
            yield return _argumentList;
        }

        public SyntaxToken Name
        {
            get { return _name; }
        }

        public ArgumentListSyntax ArgumentList
        {
            get { return _argumentList; }
        }
    }
}