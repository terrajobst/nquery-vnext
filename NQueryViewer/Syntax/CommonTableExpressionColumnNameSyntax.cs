using System;
using System.Collections.Generic;

namespace NQueryViewer.Syntax
{
    public sealed class CommonTableExpressionColumnNameSyntax : SyntaxNode
    {
        private readonly SyntaxToken _identifier;
        private readonly SyntaxToken? _commaToken;

        public CommonTableExpressionColumnNameSyntax(SyntaxToken identifier, SyntaxToken? commaToken)
        {
            _identifier = identifier;
            _commaToken = commaToken;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.CommonTableExpressionColumnName; }
        }

        public override IEnumerable<SyntaxNodeOrToken> GetChildren()
        {
            yield return _identifier;
            if (_commaToken != null)
                yield return _commaToken.Value;
        }

        public SyntaxToken Identifier
        {
            get { return _identifier; }
        }

        public SyntaxToken? CommaToken
        {
            get { return _commaToken; }
        }
    }
}