using System;
using System.Collections.Generic;

namespace NQuery.Syntax
{
    public sealed class NamedTableReferenceSyntax : TableReferenceSyntax
    {
        private readonly SyntaxToken _tableName;
        private readonly AliasSyntax _alias;

        internal NamedTableReferenceSyntax(SyntaxTree syntaxTree, SyntaxToken tableName, AliasSyntax alias)
            : base(syntaxTree)
        {
            _tableName = tableName;
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