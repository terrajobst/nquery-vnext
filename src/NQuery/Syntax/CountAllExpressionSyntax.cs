#nullable enable

using System;
using System.Collections.Generic;

namespace NQuery.Syntax
{
    public sealed class CountAllExpressionSyntax : ExpressionSyntax
    {
        internal CountAllExpressionSyntax(SyntaxTree syntaxTree, SyntaxToken identifier, SyntaxToken leftParenthesis, SyntaxToken asteriskToken, SyntaxToken rightParenthesis)
            : base(syntaxTree)
        {
            Name = identifier;
            LeftParenthesis = leftParenthesis;
            AsteriskToken = asteriskToken;
            RightParenthesis = rightParenthesis;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.CountAllExpression; }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
        {
            yield return Name;
            yield return LeftParenthesis;
            yield return AsteriskToken;
            yield return RightParenthesis;
        }

        public SyntaxToken Name { get; }

        public SyntaxToken LeftParenthesis { get; }

        public SyntaxToken AsteriskToken { get; }

        public SyntaxToken RightParenthesis { get; }
    }
}