using System;
using System.Collections.Generic;

namespace NQuery.Syntax
{
    public sealed class OrderByColumnSyntax : SyntaxNode
    {
        private readonly ExpressionSyntax _columnSelector;
        private readonly SyntaxToken _modifier;

        public OrderByColumnSyntax(SyntaxTree syntaxTree, ExpressionSyntax columnSelector, SyntaxToken modifier)
            : base(syntaxTree)
        {
            _columnSelector = columnSelector;
            _modifier = modifier;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.OrderByColumn; }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
        {
            yield return _columnSelector;
            if (_modifier != null)
                yield return _modifier;
        }

        public ExpressionSyntax ColumnSelector
        {
            get { return _columnSelector; }
        }

        public SyntaxToken Modifier
        {
            get { return _modifier; }
        }
    }
}