using System.Collections.Generic;

namespace NQuery.Language
{
    public sealed class SelectClauseSyntax : SyntaxNode
    {
        private readonly SyntaxToken _selectKeyword;
        private readonly SyntaxToken _distinctAllKeyword;
        private readonly TopClauseSyntax _topClause;
        private readonly SeparatedSyntaxList<SelectColumnSyntax> _columns;

        public SelectClauseSyntax(SyntaxTree syntaxTree, SyntaxToken selectKeyword, SyntaxToken distinctAllKeyword, TopClauseSyntax topClause, SeparatedSyntaxList<SelectColumnSyntax> selectColumns)
            : base(syntaxTree)
        {
            _selectKeyword = selectKeyword;
            _distinctAllKeyword = distinctAllKeyword;
            _topClause = topClause;
            _columns = selectColumns;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.SelectClause; }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
        {
            yield return _selectKeyword;
            if (_distinctAllKeyword != null)
                yield return _distinctAllKeyword;
            if (_topClause != null)
                yield return _topClause;
            foreach (var nodeOrToken in _columns.GetWithSeparators())
                yield return nodeOrToken;
        }

        public SyntaxToken SelectKeyword
        {
            get { return _selectKeyword; }
        }

        public SyntaxToken DistinctAllKeyword
        {
            get { return _distinctAllKeyword; }
        }

        public TopClauseSyntax TopClause
        {
            get { return _topClause; }
        }

        public SeparatedSyntaxList<SelectColumnSyntax> Columns
        {
            get { return _columns; }
        }
    }
}