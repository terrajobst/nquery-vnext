using System.Diagnostics.CodeAnalysis;

using NQuery.Authoring.Configuration;

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
// insert_final_newline, end_of_line), which is what WithEditorConfig maps onto rather than
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

    // Overlays whatever the config actually says onto these options, leaving everything it is
    // silent about alone. That is what makes the precedence chain a matter of who calls this and
    // with what, rather than a rule written down somewhere: a caller resolves its own settings
    // first and hands them in, and only the keys present in the file win over them.
    public FormattingOptions WithEditorConfig(EditorConfig editorConfig)
    {
        ThrowIfNull(editorConfig);

        var result = this;

        // The preset goes first and every other key tunes it, which is the same order as
        // FormattingOptions.Stacked with { On = OwnLine }. It can't be "whichever came last in the
        // file": these are a dictionary by the time they reach here, so file order is gone.
        //
        // Only the four properties a preset actually decides are copied. Replacing the whole
        // options object would also reset the ones the file never mentioned, which is exactly what
        // this method promises not to do.
        if (TryGetStyle(editorConfig, out var style))
        {
            result = result with
            {
                Layout = style.Layout,
                KeywordColumn = style.KeywordColumn,
                SelectColumns = style.SelectColumns,
                Joins = style.Joins
            };
        }

        // The standard properties.

        if (TryGetUseTabs(editorConfig, out var useTabs))
            result = result with { UseTabs = useTabs };

        if (TryGetIndentSize(editorConfig, out var indentSize))
            result = result with { IndentSize = indentSize };

        if (TryGetMaxLineLength(editorConfig, out var maxLineLength))
            result = result with { MaxLineLength = maxLineLength };

        if (editorConfig.TryGetBoolean(@"insert_final_newline", out var insertFinalNewline))
            result = result with { InsertFinalNewline = insertFinalNewline };

        // The formatter renders every gap between two tokens from scratch, so this converts the
        // breaks already in the document as well as the ones it adds. The exception is a break
        // inside a token's own text -- a multi-line comment -- which is copied through.
        if (TryGetNewLine(editorConfig, out var newLine))
            result = result with { NewLine = newLine };

        // The ones only this formatter has. There is deliberately no nquery_layout: LayoutStyle is
        // the choice the others hang off, and picking it without its companions is what
        // nquery_style exists to prevent.

        if (TryGetSize(editorConfig, @"nquery_keyword_column", out var keywordColumn))
            result = result with { KeywordColumn = keywordColumn };

        if (TryGetSelectColumns(editorConfig, out var selectColumns))
            result = result with { SelectColumns = selectColumns };

        if (TryGetJoinIndentation(editorConfig, out var joins))
            result = result with { Joins = joins };

        if (TryGetOnPlacement(editorConfig, out var on))
            result = result with { On = on };

        if (TryGetKeywordCase(editorConfig, out var keywords))
            result = result with { Keywords = keywords };

        if (TryGetIdentifierQuoting(editorConfig, out var identifiers))
            result = result with { Identifiers = identifiers };

        if (TryGetSize(editorConfig, @"nquery_max_blank_lines", out var maxBlankLines))
            result = result with { MaxBlankLines = maxBlankLines };

        return result;
    }

    // The spelling in the file and the name in C# are deliberately kept apart: one is written by
    // hand into a checked-in file and can never change again, the other is ours to rename. That is
    // why these are written out rather than handed to Enum.TryParse, which would also take numbers
    // and every member we ever add.

    private static bool TryGetStyle(EditorConfig editorConfig, [NotNullWhen(true)] out FormattingOptions? style)
    {
        style = null;

        if (!editorConfig.TryGetString(@"nquery_style", out var value))
            return false;

        if (Is(value, @"tabular"))
            style = Tabular;
        else if (Is(value, @"stacked"))
            style = Stacked;
        else if (Is(value, @"compact"))
            style = Compact;
        else
            return false;

        return true;
    }

    private static bool TryGetUseTabs(EditorConfig editorConfig, out bool useTabs)
    {
        useTabs = default;

        if (!editorConfig.TryGetString(@"indent_style", out var value))
            return false;

        if (Is(value, @"tab"))
            useTabs = true;
        else if (Is(value, @"space"))
            useTabs = false;
        else
            return false;

        return true;
    }

    private static bool TryGetNewLine(EditorConfig editorConfig, [NotNullWhen(true)] out string? newLine)
    {
        newLine = null;

        if (!editorConfig.TryGetString(@"end_of_line", out var value))
            return false;

        if (Is(value, @"lf"))
            newLine = "\n";
        else if (Is(value, @"crlf"))
            newLine = "\r\n";
        else if (Is(value, @"cr"))
            newLine = "\r";
        else
            return false;

        return true;
    }

    private static bool TryGetSelectColumns(EditorConfig editorConfig, out ListStyle selectColumns)
    {
        selectColumns = default;

        if (!editorConfig.TryGetString(@"nquery_select_columns", out var value))
            return false;

        if (Is(value, @"one_per_line"))
            selectColumns = ListStyle.OnePerLine;
        else if (Is(value, @"wrap_when_too_long"))
            selectColumns = ListStyle.WrapWhenTooLong;
        else
            return false;

        return true;
    }

    private static bool TryGetJoinIndentation(EditorConfig editorConfig, out JoinIndentation joins)
    {
        joins = default;

        if (!editorConfig.TryGetString(@"nquery_join_indentation", out var value))
            return false;

        if (Is(value, @"from_level"))
            joins = JoinIndentation.AtFromLevel;
        else if (Is(value, @"indented"))
            joins = JoinIndentation.Indented;
        else
            return false;

        return true;
    }

    private static bool TryGetOnPlacement(EditorConfig editorConfig, out OnPlacement on)
    {
        on = default;

        if (!editorConfig.TryGetString(@"nquery_on_placement", out var value))
            return false;

        if (Is(value, @"same_line"))
            on = OnPlacement.SameLine;
        else if (Is(value, @"own_line"))
            on = OnPlacement.OwnLine;
        else if (Is(value, @"own_line_when_multiple"))
            on = OnPlacement.OwnLineWhenMultiple;
        else
            return false;

        return true;
    }

    private static bool TryGetKeywordCase(EditorConfig editorConfig, out Casing keywords)
    {
        keywords = default;

        if (!editorConfig.TryGetString(@"nquery_keyword_case", out var value))
            return false;

        if (Is(value, @"upper"))
            keywords = Casing.Upper;
        else if (Is(value, @"lower"))
            keywords = Casing.Lower;
        else if (Is(value, @"preserve"))
            keywords = Casing.Preserve;
        else
            return false;

        return true;
    }

    private static bool TryGetIdentifierQuoting(EditorConfig editorConfig, out IdentifierQuoting identifiers)
    {
        identifiers = default;

        if (!editorConfig.TryGetString(@"nquery_identifier_quoting", out var value))
            return false;

        if (Is(value, @"preserve"))
            identifiers = IdentifierQuoting.Preserve;
        else if (Is(value, @"when_required"))
            identifiers = IdentifierQuoting.WhenRequired;
        else
            return false;

        return true;
    }

    // indent_size = tab means "whatever a tab is worth here", which is the one question tab_width
    // answers. The spec only defaults in that direction -- tab_width falls back to indent_size,
    // never the reverse -- so a file setting tab_width alone doesn't change the indent.
    private static bool TryGetIndentSize(EditorConfig editorConfig, out int indentSize)
    {
        if (editorConfig.TryGetString(@"indent_size", out var value) && Is(value, @"tab"))
            return TryGetSize(editorConfig, @"tab_width", out indentSize);

        return TryGetSize(editorConfig, @"indent_size", out indentSize);
    }

    private static bool TryGetMaxLineLength(EditorConfig editorConfig, out int maxLineLength)
    {
        // off is the standard way to say "don't wrap", which is what 0 means here.
        if (editorConfig.TryGetString(@"max_line_length", out var value) && Is(value, @"off"))
        {
            maxLineLength = 0;
            return true;
        }

        return TryGetSize(editorConfig, @"max_line_length", out maxLineLength);
    }

    // A negative width is nonsense rather than a preference, and is dropped the same way an
    // unparsable one is: the file belongs to the repository, and one bad line shouldn't decide how
    // the whole document is laid out.
    private static bool TryGetSize(EditorConfig editorConfig, string key, out int size)
    {
        if (editorConfig.TryGetInt32(key, out size) && size >= 0)
            return true;

        size = 0;
        return false;
    }

    // EditorConfig values are case-insensitive, and comparing rather than lowercasing is what keeps
    // reading a config free of allocation.
    private static bool Is(string value, string name)
    {
        return string.Equals(value, name, StringComparison.OrdinalIgnoreCase);
    }
}
