using System.Runtime.CompilerServices;

namespace NQuery.Tests.CodeAnalysis.Syntax;

public class GrammarTests
{
    // The generated EBNF is committed as documentation under docs/. This
    // regenerates it from the syntax-tree classes and fails if the committed copy
    // is stale (rewriting it so the diff is ready to review) -- a doc that can't
    // drift from the language.
    [Fact]
    public void Grammar_Ebnf_IsUpToDate()
    {
        var expected = GrammarEbnfWriter.Write(Grammar.FromSyntaxTree());
        var path = Path.Combine(RepoRoot(), "docs", "grammar.ebnf");

        var actual = File.Exists(path) ? File.ReadAllText(path) : null;
        if (Normalize(actual) != Normalize(expected))
        {
            File.WriteAllText(path, expected);
            Assert.Fail($"grammar.ebnf was out of date; regenerated at {path}. Review the diff and re-run.");
        }
    }

    private static string Normalize(string? text) => (text ?? "").Replace("\r\n", "\n");

    private static string RepoRoot() => Directory.GetParent(SrcDirectory())!.FullName;

    private static string SrcDirectory([CallerFilePath] string path = "")
    {
        var dir = new DirectoryInfo(Path.GetDirectoryName(path)!);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "NQuery.slnx")))
            dir = dir.Parent;

        return dir?.FullName ?? throw new InvalidOperationException("Could not locate the src directory (NQuery.slnx).");
    }
}
