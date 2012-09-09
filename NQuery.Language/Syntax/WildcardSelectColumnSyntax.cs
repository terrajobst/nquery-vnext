using System;
using System.Collections.Generic;

namespace NQuery.Language
{
    public sealed class WildcardSelectColumnSyntax : SelectColumnSyntax
    {
        private readonly SyntaxToken? _tableName;
        private readonly SyntaxToken? _dotToken;
        private readonly SyntaxToken _asteriskToken;

        public WildcardSelectColumnSyntax(SyntaxTree syntaxTree, SyntaxToken? tableName, SyntaxToken? dotToken, SyntaxToken asteriskToken)
            : base(syntaxTree)
        {
            _tableName = tableName.WithParent(this);
            _dotToken = dotToken.WithParent(this);
            _asteriskToken = asteriskToken.WithParent(this);
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.WildcardSelectColumn; }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
        {
            if (_tableName != null)
                yield return _tableName.Value;
            if (_dotToken != null)
                yield return _dotToken.Value;
            yield return _asteriskToken;
        }

        public SyntaxToken? TableName
        {
            get { return _tableName; }
        }

        public SyntaxToken? DotToken
        {
            get { return _dotToken; }
        }

        public SyntaxToken AsteriskToken
        {
            get { return _asteriskToken; }
        }
    }
}