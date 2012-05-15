using System;
using System.Collections.Generic;

namespace NQuery.Language
{
    public sealed class CompilationUnitSyntax : SyntaxNode
    {
        private readonly SyntaxNode _root;
        private readonly SyntaxToken _endOfFileToken;

        public CompilationUnitSyntax(SyntaxNode root, SyntaxToken endOfFileToken)
        {
            _root = root;
            _endOfFileToken = endOfFileToken;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.CompilationUnit; }
        }

        public override IEnumerable<SyntaxNodeOrToken> GetChildren()
        {
            yield return _root;
            yield return _endOfFileToken;
        }

        public SyntaxNode Root
        {
            get { return _root; }
        }

        public SyntaxToken EndOfFileToken
        {
            get { return _endOfFileToken; }
        }
    }
}