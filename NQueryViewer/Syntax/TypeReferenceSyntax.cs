using System;
using System.Collections.Generic;

namespace NQueryViewer.Syntax
{
    public sealed class TypeReferenceSyntax : SyntaxNode
    {
        private readonly SyntaxToken _token;
        private readonly string _typeName;

        public TypeReferenceSyntax(SyntaxToken token, string typeName)
        {
            _token = token;
            _typeName = typeName;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.TypeReference; }
        }

        public override IEnumerable<SyntaxNodeOrToken> GetChildren()
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