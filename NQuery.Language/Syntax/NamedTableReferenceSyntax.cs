using System;
using System.Collections.Generic;

namespace NQuery.Language
{
    public sealed class NamedTableReferenceSyntax : TableReferenceSyntax
    {
        private readonly SyntaxToken _tableName;
        private readonly AliasSyntax _alias;

        public NamedTableReferenceSyntax(SyntaxTree syntaxTree, SyntaxToken tableName, AliasSyntax alias, SyntaxToken? commaToken)
            : base(syntaxTree, commaToken)
        {
            _tableName = tableName.WithParent(this);
            _alias = alias;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.NamedTableReference; }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
        {
            yield return _tableName;
            if (_alias != null)
                yield return _alias;
            if (CommaToken != null)
                yield return CommaToken.Value;
        }

        public SyntaxToken TableName
        {
            get { return _tableName; }
        }

        public AliasSyntax Alias
        {
            get { return _alias; }
        }
    }
}