using NQuery.CodeAnalysis.Text;

namespace NQuery.Authoring.BraceMatching;

public struct BraceMatchingResult
{
    public static BraceMatchingResult None { get; } = new();

    public BraceMatchingResult(TextSpan left, TextSpan right)
    {
        IsValid = true;
        Left = left;
        Right = right;
    }

    public bool IsValid { get; }

    public TextSpan Left { get; }

    public TextSpan Right { get; }
}
