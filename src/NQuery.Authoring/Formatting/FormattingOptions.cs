namespace NQuery.Authoring.Formatting;

// How a clause keyword relates to its payload. This is the choice the other layout options hang
// off, which is why the presets below set it together with the values that go with it rather than
// leaving every combination reachable: KeywordColumn means nothing to Stacked.
public enum LayoutStyle
{
    // Keyword flush left, payload padded to KeywordColumn, continuations aligned under the payload.
    Tabular,

    // Keyword flush left. A clause whose payload is a list puts the keyword on its own line and
    // indents the items; a clause with a single payload keeps it on the keyword's line.
    Stacked
}

public enum ListStyle
{
    OnePerLine,
    WrapWhenTooLong
}

public enum JoinIndentation
{
    AtFromLevel,
    Indented
}

public enum OnPlacement
{
    SameLine,
    OwnLine,
    OwnLineWhenMultiple
}

public enum Casing
{
    Upper,
    Lower,
    Preserve
}

public enum IdentifierQuoting
{
    // Leave every identifier exactly as written.
    Preserve,

    // Strip brackets and quotes from identifiers that don't need them.
    WhenRequired
}

// What the formatter is allowed to do, resolved. Nothing here is optional or layered -- a host that
// merges settings from several sources (an editor's per-request options, a config file) resolves
// them into one of these first, which is why the presets are the entry point rather than a
// parameterless constructor.
//
// IndentSize, UseTabs, MaxLineLength, InsertFinalNewline and NewLine deliberately mirror the
// standard EditorConfig properties of the same meaning (indent_size, indent_style, max_line_length,
// insert_final_newline, end_of_line), so a future config reader maps onto them rather than
// inventing a parallel vocabulary. There is no trim_trailing_whitespace: the formatter never emits
// any, so it isn't a choice.
public sealed record FormattingOptions
{
    public static FormattingOptions Tabular { get; } = new()
    {
        Layout = LayoutStyle.Tabular,
        KeywordColumn = 8,
        SelectColumns = ListStyle.OnePerLine,
        Joins = JoinIndentation.Indented
    };

    public static FormattingOptions Stacked { get; } = new()
    {
        Layout = LayoutStyle.Stacked,
        SelectColumns = ListStyle.OnePerLine,
        Joins = JoinIndentation.AtFromLevel
    };

    // Stacked, but nothing breaks until it has to. The difference between the two is entirely in
    // SelectColumns, which is why Compact is a preset rather than a third layout.
    public static FormattingOptions Compact { get; } = new()
    {
        Layout = LayoutStyle.Stacked,
        SelectColumns = ListStyle.WrapWhenTooLong,
        Joins = JoinIndentation.AtFromLevel
    };

    // Tabular, because the corpus, docs and samples in this repo are all written that way -- a
    // formatter whose default reformats the entire existing code base is a formatter nobody runs.
    public static FormattingOptions Default => Tabular;

    public LayoutStyle Layout { get; init; } = LayoutStyle.Tabular;

    public int IndentSize { get; init; } = 4;

    public bool UseTabs { get; init; }

    // The column a clause's payload starts at under Tabular. Ignored by the other layouts.
    public int KeywordColumn { get; init; } = 8;

    // 0 disables wrapping, which is the only way to get output that never depends on line width.
    public int MaxLineLength { get; init; } = 100;

    // Governs the select list. The other lists (GROUP BY, ORDER BY, arguments, FROM) always wrap
    // on demand: one-per-line reads well for a projection and badly for a two-column GROUP BY.
    public ListStyle SelectColumns { get; init; } = ListStyle.OnePerLine;

    public JoinIndentation Joins { get; init; } = JoinIndentation.Indented;

    public OnPlacement On { get; init; } = OnPlacement.SameLine;

    public Casing Keywords { get; init; } = Casing.Upper;

    public IdentifierQuoting Identifiers { get; init; } = IdentifierQuoting.Preserve;

    // Blank lines the author wrote are kept, up to this many, because they carry grouping the
    // grammar doesn't. Zero collapses them all.
    public int MaxBlankLines { get; init; } = 1;

    public bool InsertFinalNewline { get; init; } = true;

    public string NewLine { get; init; } = Environment.NewLine;
}
