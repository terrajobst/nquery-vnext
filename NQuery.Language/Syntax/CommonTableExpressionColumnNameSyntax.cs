using System;
using System.Collections.Generic;

namespace NQuery.Language
{
    public sealed class CommonTableExpressionColumnNameSyntax : SyntaxNode
    {
        private readonly SyntaxToken _identifier;
        private readonly SyntaxToken? _commaToken;

        public CommonTableExpressionColumnNameSyntax(SyntaxTree syntaxTree, SyntaxToken identifier, SyntaxToken? commaToken)
            : base(syntaxTree)
        {
            _identifier = identifier.WithParent(this);
            _commaToken = commaToken.WithParent(this);
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.CommonTableExpressionColumnName; }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
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