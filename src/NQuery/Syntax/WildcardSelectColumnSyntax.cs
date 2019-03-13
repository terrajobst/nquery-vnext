#nullable enable

using System;
using System.Collections.Generic;

namespace NQuery.Syntax
{
    public sealed class WildcardSelectColumnSyntax : SelectColumnSyntax
    {
        internal WildcardSelectColumnSyntax(SyntaxTree syntaxTree, SyntaxToken? tableName, SyntaxToken? dotToken, SyntaxToken asteriskToken)
            : base(syntaxTree)
        {
            TableName = tableName;
            DotToken = dotToken;
            AsteriskToken = asteriskToken;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.WildcardSelectColumn; }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
        {
            if (TableName != null)
                yield return TableName;
            if (DotToken != null)
                yield return DotToken;
            yield return AsteriskToken;
        }

        public SyntaxToken? TableName { get; }

        public SyntaxToken? DotToken { get; }

        public SyntaxToken AsteriskToken { get; }
    }
}