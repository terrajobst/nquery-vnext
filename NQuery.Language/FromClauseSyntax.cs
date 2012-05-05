using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace NQuery.Language
{
    public sealed class FromClauseSyntax : SyntaxNode
    {
        private readonly SyntaxToken _fromKeyword;
        private readonly ReadOnlyCollection<TableReferenceSyntax> _tableReferences;

        public FromClauseSyntax(SyntaxToken fromKeyword, IList<TableReferenceSyntax> tableReferences)
        {
            _fromKeyword = fromKeyword;
            _tableReferences = new ReadOnlyCollection<TableReferenceSyntax>(tableReferences);
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.FromClause; }
        }

        public override IEnumerable<SyntaxNodeOrToken> GetChildren()
        {
            yield return _fromKeyword;
            foreach (var tableReference in TableReferences)
                yield return tableReference;
        }

        public SyntaxToken FromKeyword
        {
            get { return _fromKeyword; }
        }

        public ReadOnlyCollection<TableReferenceSyntax> TableReferences
        {
            get { return _tableReferences; }
        }
    }
}