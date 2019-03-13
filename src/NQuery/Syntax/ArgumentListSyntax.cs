#nullable enable

using System;
using System.Collections.Generic;

namespace NQuery.Syntax
{
    public sealed class ArgumentListSyntax : SyntaxNode
    {
        internal ArgumentListSyntax(SyntaxTree syntaxTree, SyntaxToken leftParenthesis, SeparatedSyntaxList<ExpressionSyntax> arguments, SyntaxToken rightParenthesis)
            : base(syntaxTree)
        {
            LeftParenthesis = leftParenthesis;
            Arguments = arguments;
            RightParenthesis = rightParenthesis;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.ArgumentList; }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
        {
            yield return LeftParenthesis;

            foreach (var nodeOrToken in Arguments.GetWithSeparators())
                yield return nodeOrToken;

            yield return RightParenthesis;
        }

        public SyntaxToken LeftParenthesis { get; }

        public SeparatedSyntaxList<ExpressionSyntax> Arguments { get; }

        public SyntaxToken RightParenthesis { get; }
    }
}