using System;
using System.Collections.Generic;

namespace NQueryViewer.Syntax
{
    public sealed class CrossJoinedTableReferenceSyntax : JoinedTableReferenceSyntax
    {
        private readonly SyntaxToken _crossKeyword;
        private readonly SyntaxToken _joinKeyword;

        public CrossJoinedTableReferenceSyntax(TableReferenceSyntax left, SyntaxToken crossKeyword, SyntaxToken joinKeyword, TableReferenceSyntax right, SyntaxToken? commaToken)
            : base(left, right, commaToken)
        {
            _crossKeyword = crossKeyword;
            _joinKeyword = joinKeyword;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.CrossJoinedTableReference; }
        }

        public override IEnumerable<SyntaxNodeOrToken> GetChildren()
        {
            yield return Left;
            yield return _crossKeyword;
            yield return _joinKeyword;
            yield return Right;
            if (CommaToken != null)
                yield return CommaToken.Value;
        }
    }
}