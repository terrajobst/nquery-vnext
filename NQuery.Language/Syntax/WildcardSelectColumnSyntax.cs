using System;
using System.Collections.Generic;

namespace NQuery.Language
{
    public sealed class WildcardSelectColumnSyntax : SelectColumnSyntax
    {
        private readonly SyntaxToken? _tableName;
        private readonly SyntaxToken? _dotToken;
        private readonly SyntaxToken _asteriskToken;

        public WildcardSelectColumnSyntax(SyntaxToken? tableName, SyntaxToken? dotToken, SyntaxToken asteriskToken, SyntaxToken? commaToken)
            : base(commaToken)
        {
            _tableName = tableName;
            _dotToken = dotToken;
            _asteriskToken = asteriskToken;
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
            if (CommaToken != null)
                yield return CommaToken.Value;
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