using System;
using System.Collections.Generic;

namespace NQuery.Language
{
    public sealed class ExpressionSelectColumnSyntax : SelectColumnSyntax
    {
        private readonly ExpressionSyntax _expression;
        private readonly AliasSyntax _alias;
        private readonly SyntaxToken? _commaToken;

        public ExpressionSelectColumnSyntax(ExpressionSyntax expression, AliasSyntax alias, SyntaxToken? commaToken)
            : base(commaToken)
        {
            _expression = expression;
            _alias = alias;
            _commaToken = commaToken;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.ExpressionSelectColumn; }
        }

        public override IEnumerable<SyntaxNodeOrToken> GetChildren()
        {
            yield return _expression;
            if (_alias != null)
                yield return _alias;
            if (_commaToken != null)
                yield return _commaToken.Value;
        }

        public ExpressionSyntax Expression
        {
            get { return _expression; }
        }

        public AliasSyntax Alias
        {
            get { return _alias; }
        }
    }
}