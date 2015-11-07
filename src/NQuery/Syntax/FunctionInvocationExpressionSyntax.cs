using System;
using System.Collections.Generic;

namespace NQuery.Syntax
{
    public sealed class FunctionInvocationExpressionSyntax : ExpressionSyntax
    {
        internal FunctionInvocationExpressionSyntax(SyntaxTree syntaxTree, SyntaxToken name, ArgumentListSyntax argumentList)
            : base(syntaxTree)
        {
            Name = name;
            ArgumentList = argumentList;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.FunctionInvocationExpression; }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
        {
            yield return Name;
            yield return ArgumentList;
        }

        public SyntaxToken Name { get; }

        public ArgumentListSyntax ArgumentList { get; }
    }
}