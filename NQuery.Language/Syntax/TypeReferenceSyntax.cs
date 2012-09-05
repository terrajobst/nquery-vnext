using System;
using System.Collections.Generic;

namespace NQuery.Language
{
    public sealed class TypeReferenceSyntax : SyntaxNode
    {
        private readonly SyntaxToken _token;
        private readonly string _typeName;

        public TypeReferenceSyntax(SyntaxTree syntaxTree, SyntaxToken token, string typeName)
            : base(syntaxTree)
        {
            _token = token.WithParent(this);
            _typeName = typeName;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.TypeReference; }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
        {
            yield return _token;
        }

        public SyntaxToken Token
        {
            get { return _token; }
        }

        public string TypeName
        {
            get { return _typeName; }
        }
    }
}