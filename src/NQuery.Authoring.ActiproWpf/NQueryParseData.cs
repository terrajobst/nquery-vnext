using System;

using ActiproSoftware.Text.Parsing;

namespace NQuery.Authoring.ActiproWpf
{
    public sealed class NQueryParseData : IParseData
    {
        public NQueryParseData(SyntaxTree syntaxTree)
        {
            SyntaxTree = syntaxTree;
        }

        public SyntaxTree SyntaxTree { get; }
    }
}