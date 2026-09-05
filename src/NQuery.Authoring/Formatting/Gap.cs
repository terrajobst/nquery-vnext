namespace NQuery.Authoring.Formatting;

// What the formatter puts between two adjacent tokens.
//
// Every character of a document is either part of a token or part of the trivia between two tokens,
// so deciding these gaps -- plus the handful of token texts the casing and identifier passes rewrite
// -- is the whole of formatting. Working in gaps rather than in a token's leading/trailing trivia is
// deliberate: the lexer splits trivia at the end of a line, so the same run of whitespace belongs to
// one token or the other depending on line breaks the formatter is about to change.
internal enum GapKind
{
    // The tokens are adjacent.
    None,

    // Exactly one space.
    Space,

    // Spaces up to Column, or one space if that column has already been passed. This is what makes
    // the tabular layout an alignment rather than an indentation.
    Pad,

    // A space if the enclosing group fits on one line, a line break if it doesn't. Unresolved until
    // rendering, since fitting depends on the column the group starts at.
    SoftLine,

    // A line break, followed by indentation to Column.
    Line
}

// Column is the target column for Pad and the indentation for SoftLine/Line; Group is the innermost
// group the gap belongs to, or -1 when it belongs to none (which reads as "always broken", since
// only a group can be flat).
internal readonly record struct Gap(GapKind Kind, int Column = 0, int Group = -1);

// A range of tokens that breaks or doesn't break as a unit -- an argument list, a parenthesized
// subquery, a CASE. Always node-shaped, so the ranges nest properly.
internal readonly record struct GapGroup(int Start, int End, int Parent);
