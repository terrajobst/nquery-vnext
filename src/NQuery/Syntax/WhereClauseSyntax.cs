using System;
using System.Collections.Generic;

namespace NQuery.Syntax
{
    public sealed class WhereClauseSyntax : SyntaxNode
    {
        internal WhereClauseSyntax(SyntaxTree syntaxTree, SyntaxToken whereKeyword, ExpressionSyntax predicate)
            : base(syntaxTree)
        {
            WhereKeyword = whereKeyword;
            Predicate = predicate;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.WhereClause; }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
        {
            yield return WhereKeyword;
            yield return Predicate;
        }

        public SyntaxToken WhereKeyword { get; }

        public ExpressionSyntax Predicate { get; }
    }
}