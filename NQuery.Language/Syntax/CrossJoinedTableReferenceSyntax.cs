using System.Collections.Generic;

namespace NQuery.Language
{
    public sealed class CrossJoinedTableReferenceSyntax : JoinedTableReferenceSyntax
    {
        private readonly SyntaxToken _crossKeyword;
        private readonly SyntaxToken _joinKeyword;

        public CrossJoinedTableReferenceSyntax(SyntaxTree syntaxTree, TableReferenceSyntax left, SyntaxToken crossKeyword, SyntaxToken joinKeyword, TableReferenceSyntax right)
            : base(syntaxTree, left, right)
        {
            _crossKeyword = crossKeyword;
            _joinKeyword = joinKeyword;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.CrossJoinedTableReference; }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
        {
            yield return Left;
            yield return _crossKeyword;
            yield return _joinKeyword;
            yield return Right;
        }

        public SyntaxToken CrossKeyword
        {
            get { return _crossKeyword; }
        }

        public SyntaxToken JoinKeyword
        {
            get { return _joinKeyword; }
        }
    }
}