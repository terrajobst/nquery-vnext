using System;
using System.Collections.Generic;

namespace NQueryViewer.Syntax
{
    public sealed class OrderByColumnSyntax : SyntaxNode
    {
        private readonly ExpressionSyntax _columnSelector;
        private readonly SyntaxToken? _modifier;
        private readonly SyntaxToken? _comma;

        public OrderByColumnSyntax(ExpressionSyntax columnSelector, SyntaxToken? modifier, SyntaxToken? comma)
        {
            _columnSelector = columnSelector;
            _modifier = modifier;
            _comma = comma;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.OrderByColumn; }
        }

        public override IEnumerable<SyntaxNodeOrToken> GetChildren()
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