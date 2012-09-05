using System;
using System.Collections.Generic;

namespace NQuery.Language
{
    public sealed class OrderByColumnSyntax : SyntaxNode
    {
        private readonly ExpressionSyntax _columnSelector;
        private readonly SyntaxToken? _modifier;
        private readonly SyntaxToken? _comma;

        public OrderByColumnSyntax(SyntaxTree syntaxTree, ExpressionSyntax columnSelector, SyntaxToken? modifier, SyntaxToken? comma)
            : base(syntaxTree)
        {
            _columnSelector = columnSelector;
            _modifier = modifier.WithParent(this);
            _comma = comma.WithParent(this);
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.OrderByColumn; }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
        {
            yield return _columnSelector;
            if (_modifier != null)
                yield return _modifier.Value;
            if (_comma != null)
                yield return _comma.Value;
        }

        public ExpressionSyntax ColumnSelector
        {
            get { return _columnSelector; }
        }

        public SyntaxToken? Modifier
        {
            get { return _modifier; }
        }

        public SyntaxToken? Comma
        {
            get { return _comma; }
        }
    }
}