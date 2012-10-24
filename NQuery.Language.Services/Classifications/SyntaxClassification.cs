using System;

namespace NQuery.Language.Services.Classifications
{
    public enum SyntaxClassification
    {
        Whitespace,
        Comment,
        Keyword,
        Punctuation,
        Identifier,
        StringLiteral,
        NumberLiteral,
    }
}