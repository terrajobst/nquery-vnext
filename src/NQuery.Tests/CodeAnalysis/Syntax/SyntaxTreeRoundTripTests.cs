using NQuery.CodeAnalysis;

namespace NQuery.Tests.CodeAnalysis.Syntax;

// ToString() is the public "give me the source back" API and must be lossless --
// including for inputs that trigger error recovery, where unexpected tokens are
// folded into SkippedTokensTrivia. That trivia stores its text in its Structure
// (its own Text is empty), so WriteTo must descend into the structure; otherwise
// the skipped text silently disappears from ToString(). (Regression guard.)
public class SyntaxTreeRoundTripTests
{
    [Theory]
    [InlineData("SELECT * FROM t")]              // valid query, no skipped tokens
    [InlineData("GROUP")]                         // bare keyword -> all skipped
    [InlineData("SELECT 1 GROUP")]                // trailing unexpected token
    [InlineData("SELECT 1 foo bar baz")]          // several skipped tokens
    [InlineData("SELECT * FROM t WHERE )")]       // skipped punctuation
    [InlineData("  SELECT 1  extra  ")]           // skipped token keeps surrounding trivia
    public void ParseQuery_ToString_RoundTripsSource(string source)
    {
        var syntaxTree = SyntaxTree.ParseQuery(source);

        Assert.Equal(source, syntaxTree.Root.ToString());
    }

    [Theory]
    [InlineData("1 + 2")]
    [InlineData("SELECT")]                         // keyword is not a valid expression -> skipped
    [InlineData("1 + 2 extra tokens")]
    public void ParseExpression_ToString_RoundTripsSource(string source)
    {
        var syntaxTree = SyntaxTree.ParseExpression(source);

        Assert.Equal(source, syntaxTree.Root.ToString());
    }
}
