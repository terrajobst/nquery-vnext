using System;
using System.Collections.Generic;

namespace NQuery.Syntax
{
    public sealed class OrderByColumnSyntax : SyntaxNode
    {
        private readonly ExpressionSyntax _columnSelector;
        public OrderByColumnSyntax(SyntaxTree syntaxTree, OrderBySelectorSyntax selector, SyntaxToken modifier)
            : base(syntaxTree)
        {
            Selector = selector;
            Modifier = modifier;
        }

        public override SyntaxKind Kind
        {
            get {  return SyntaxKind.OrderByColumn; }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
        {
            yield return Selector;

            if (Modifier != null)
                yield return Modifier;
        }

        public OrderBySelectorSyntax Selector { get; }

        public SyntaxToken Modifier { get; }
    }
}