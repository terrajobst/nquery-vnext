using ActiproSoftware.Text.Parsing;
using NQuery.CodeAnalysis;

namespace NQuery.Authoring.ActiproWpf;

public sealed class NQueryParseData : IParseData
{
    public NQueryParseData(SyntaxTree syntaxTree)
    {
        SyntaxTree = syntaxTree;
    }

    public SyntaxTree SyntaxTree { get; }
}
