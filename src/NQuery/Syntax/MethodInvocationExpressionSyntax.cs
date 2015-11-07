using System;
using System.Collections.Generic;

namespace NQuery.Syntax
{
    public sealed class MethodInvocationExpressionSyntax : ExpressionSyntax
    {
        internal MethodInvocationExpressionSyntax(SyntaxTree syntaxTree, ExpressionSyntax target, SyntaxToken dot, SyntaxToken name, ArgumentListSyntax argumentList)
            : base(syntaxTree)
        {
            Target = target;
            Dot = dot;
            Name = name;
            ArgumentList = argumentList;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.MethodInvocationExpression; }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
        {
            yield return Target;
            yield return Dot;
            yield return Name;
            yield return ArgumentList;
        }

        public ExpressionSyntax Target { get; }

        public SyntaxToken Dot { get; }

        public SyntaxToken Name { get; }

        public ArgumentListSyntax ArgumentList { get; }
    }
}