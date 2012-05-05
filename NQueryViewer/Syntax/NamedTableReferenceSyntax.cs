using System;
using System.Collections.Generic;

namespace NQueryViewer.Syntax
{
    public sealed class NamedTableReferenceSyntax : TableReferenceSyntax
    {
        private readonly SyntaxToken _tableName;
        private readonly AliasSyntax _alias;

        public NamedTableReferenceSyntax(SyntaxToken tableName, AliasSyntax alias, SyntaxToken? commaToken)
            : base(commaToken)
        {
            _tableName = tableName;
            _alias = alias;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.NamedTableReference; }
        }

        public override IEnumerable<SyntaxNodeOrToken> GetChildren()
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