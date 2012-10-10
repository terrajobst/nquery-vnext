using System.Collections.Generic;

namespace NQuery.Language
{
    public sealed class FromClauseSyntax : SyntaxNode
    {
        private readonly SyntaxToken _fromKeyword;
        private readonly SeparatedSyntaxList<TableReferenceSyntax> _tableReferences;

        public FromClauseSyntax(SyntaxTree syntaxTree, SyntaxToken fromKeyword, SeparatedSyntaxList<TableReferenceSyntax> tableReferences)
            : base(syntaxTree)
        {
            _fromKeyword = fromKeyword;
            _tableReferences = tableReferences;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.FromClause; }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
        {
            yield return _fromKeyword;
            foreach (var nodeOrToken in _tableReferences.GetWithSeparators())
                yield return nodeOrToken;
        }

        public SyntaxToken FromKeyword
        {
            get { return _fromKeyword; }
        }

        public SeparatedSyntaxList<TableReferenceSyntax> TableReferences
        {
            get { return _tableReferences; }
        }
    }
}