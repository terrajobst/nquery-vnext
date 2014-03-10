using System;

using ActiproSoftware.Text.Parsing;

namespace NQuery.Authoring.ActiproWpf
{
    public sealed class NQueryParseData : IParseData
    {
        private readonly SyntaxTree _syntaxTree;

        public NQueryParseData(SyntaxTree syntaxTree)
        {
            _syntaxTree = syntaxTree;
        }

        public SyntaxTree SyntaxTree
        {
            get { return _syntaxTree; }
        }
    }
}