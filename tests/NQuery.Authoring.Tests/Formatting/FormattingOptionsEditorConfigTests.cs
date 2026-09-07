using NQuery.Authoring.Configuration;
using NQuery.Authoring.Formatting;

namespace NQuery.Authoring.Tests.Formatting;

// Options in, options out -- no document, no file system, no resolution order. The config is
// written as text here rather than assembled from properties, because what is being tested is the
// mapping from what someone actually writes in an .editorconfig.
public class FormattingOptionsEditorConfigTests
{
    [Fact]
    public void WithEditorConfig_ChangesNothingWhenTheConfigIsSilent()
    {
        var editorConfig = """
            [*]
            charset = utf-8
            trim_trailing_whitespace = true
            """;

        var options = Apply(editorConfig);

        Assert.Equal(FormattingOptions.Default, options);
    }

    [Fact]
    public void WithEditorConfig_ChangesNothingWhenNoSectionMatches()
    {
        var editorConfig = """
            [*.nqe]
            indent_size = 2
            """;

        var options = Apply(editorConfig);

        Assert.Equal(FormattingOptions.Default, options);
    }

    [Fact]
    public void WithEditorConfig_KeepsEverythingItIsSilentAbout()
    {
        var editorConfig = """
            [*]
            indent_size = 2
            """;

        var baseline = FormattingOptions.Compact with { Keywords = Casing.Lower, MaxBlankLines = 3 };

        var options = Apply(editorConfig, baseline);

        Assert.Equal(baseline with { IndentSize = 2 }, options);
    }

    [Fact]
    public void WithEditorConfig_ReadsIndentStyle()
    {
        var tab = """
            [*]
            indent_style = tab
            """;

        var space = """
            [*]
            indent_style = space
            """;

        var baseline = FormattingOptions.Default with { UseTabs = true };

        Assert.True(Apply(tab).UseTabs);
        Assert.False(Apply(space, baseline).UseTabs);
    }

    [Fact]
    public void WithEditorConfig_IgnoresAnIndentStyleItDoesNotKnow()
    {
        var editorConfig = """
            [*]
            indent_style = wat
            """;

        var baseline = FormattingOptions.Default with { UseTabs = true };

        var options = Apply(editorConfig, baseline);

        Assert.True(options.UseTabs);
    }

    [Fact]
    public void WithEditorConfig_ReadsIndentSize()
    {
        var editorConfig = """
            [*]
            indent_size = 2
            """;

        var options = Apply(editorConfig);

        Assert.Equal(2, options.IndentSize);
    }

    [Fact]
    public void WithEditorConfig_ResolvesAnIndentSizeOfTabAgainstTabWidth()
    {
        var editorConfig = """
            [*]
            indent_style = tab
            indent_size = tab
            tab_width = 8
            """;

        var options = Apply(editorConfig);

        Assert.Equal(8, options.IndentSize);
        Assert.True(options.UseTabs);
    }

    [Fact]
    public void WithEditorConfig_IgnoresAnIndentSizeOfTabWithoutATabWidth()
    {
        var editorConfig = """
            [*]
            indent_size = tab
            """;

        var options = Apply(editorConfig);

        Assert.Equal(FormattingOptions.Default.IndentSize, options.IndentSize);
    }

    [Fact]
    public void WithEditorConfig_IgnoresTabWidthOnItsOwn()
    {
        // EditorConfig only defaults tab_width from indent_size, never the reverse.
        var editorConfig = """
            [*]
            tab_width = 8
            """;

        var options = Apply(editorConfig);

        Assert.Equal(FormattingOptions.Default.IndentSize, options.IndentSize);
    }

    [Fact]
    public void WithEditorConfig_IgnoresAnIndentSizeThatIsNotANumber()
    {
        var editorConfig = """
            [*]
            indent_size = wide
            """;

        var options = Apply(editorConfig);

        Assert.Equal(FormattingOptions.Default.IndentSize, options.IndentSize);
    }

    [Fact]
    public void WithEditorConfig_IgnoresANegativeIndentSize()
    {
        var editorConfig = """
            [*]
            indent_size = -2
            """;

        var options = Apply(editorConfig);

        Assert.Equal(FormattingOptions.Default.IndentSize, options.IndentSize);
    }

    [Fact]
    public void WithEditorConfig_ReadsMaxLineLength()
    {
        var editorConfig = """
            [*]
            max_line_length = 80
            """;

        var options = Apply(editorConfig);

        Assert.Equal(80, options.MaxLineLength);
    }

    [Fact]
    public void WithEditorConfig_TurnsMaxLineLengthOff()
    {
        // 0 is how these options spell "never wrap", which is what off means.
        var editorConfig = """
            [*]
            max_line_length = off
            """;

        var options = Apply(editorConfig);

        Assert.Equal(0, options.MaxLineLength);
    }

    [Fact]
    public void WithEditorConfig_IgnoresAMaxLineLengthItCannotRead()
    {
        var editorConfig = """
            [*]
            max_line_length = wide
            """;

        var options = Apply(editorConfig);

        Assert.Equal(FormattingOptions.Default.MaxLineLength, options.MaxLineLength);
    }

    [Fact]
    public void WithEditorConfig_ReadsInsertFinalNewline()
    {
        var off = """
            [*]
            insert_final_newline = false
            """;

        var on = """
            [*]
            insert_final_newline = true
            """;

        var baseline = FormattingOptions.Default with { InsertFinalNewline = false };

        Assert.False(Apply(off).InsertFinalNewline);
        Assert.True(Apply(on, baseline).InsertFinalNewline);
    }

    [Fact]
    public void WithEditorConfig_IgnoresAnInsertFinalNewlineThatIsNotABoolean()
    {
        var editorConfig = """
            [*]
            insert_final_newline = yes
            """;

        var baseline = FormattingOptions.Default with { InsertFinalNewline = false };

        var options = Apply(editorConfig, baseline);

        Assert.False(options.InsertFinalNewline);
    }

    [Fact]
    public void WithEditorConfig_ReadsEndOfLine()
    {
        var lf = """
            [*]
            end_of_line = lf
            """;

        var crlf = """
            [*]
            end_of_line = crlf
            """;

        var cr = """
            [*]
            end_of_line = cr
            """;

        Assert.Equal("\n", Apply(lf).NewLine);
        Assert.Equal("\r\n", Apply(crlf).NewLine);
        Assert.Equal("\r", Apply(cr).NewLine);
    }

    [Fact]
    public void WithEditorConfig_IgnoresAnEndOfLineItDoesNotKnow()
    {
        var editorConfig = """
            [*]
            end_of_line = nel
            """;

        var baseline = FormattingOptions.Default with { NewLine = "\n" };

        var options = Apply(editorConfig, baseline);

        Assert.Equal("\n", options.NewLine);
    }

    [Fact]
    public void WithEditorConfig_ReadsKeysAndValuesRegardlessOfCase()
    {
        var editorConfig = """
            [*]
            INDENT_STYLE = TAB
            End_Of_Line = CRLF
            NQuery_Keyword_Case = Lower
            """;

        var options = Apply(editorConfig);

        Assert.True(options.UseTabs);
        Assert.Equal("\r\n", options.NewLine);
        Assert.Equal(Casing.Lower, options.Keywords);
    }

    [Fact]
    public void WithEditorConfig_LetsTheLaterSectionWin()
    {
        var editorConfig = """
            [*]
            indent_size = 2

            [*.nql]
            indent_size = 8
            """;

        var options = Apply(editorConfig);

        Assert.Equal(8, options.IndentSize);
    }

    [Fact]
    public void WithEditorConfig_LeavesUnsetPropertiesToTheBaseline()
    {
        var editorConfig = """
            [*]
            indent_size = 2

            [*.nql]
            indent_size = unset
            """;

        var options = Apply(editorConfig);

        Assert.Equal(FormattingOptions.Default.IndentSize, options.IndentSize);
    }

    [Fact]
    public void WithEditorConfig_ReadsTheStylePreset()
    {
        var tabular = """
            [*]
            nquery_style = tabular
            """;

        var stacked = """
            [*]
            nquery_style = stacked
            """;

        var compact = """
            [*]
            nquery_style = compact
            """;

        AssertPreset(FormattingOptions.Tabular, Apply(tabular, FormattingOptions.Stacked));
        AssertPreset(FormattingOptions.Stacked, Apply(stacked));
        AssertPreset(FormattingOptions.Compact, Apply(compact));
    }

    [Fact]
    public void WithEditorConfig_StyleOnlyDecidesWhatAPresetDecides()
    {
        // A preset picks a layout and the values that go with it. Everything else the host had
        // resolved stays, which is why this copies four properties instead of the whole object.
        var editorConfig = """
            [*]
            nquery_style = stacked
            """;

        var baseline = FormattingOptions.Default with
        {
            Keywords = Casing.Lower,
            Identifiers = IdentifierQuoting.WhenRequired,
            IndentSize = 2,
            MaxBlankLines = 3
        };

        var options = Apply(editorConfig, baseline);

        AssertPreset(FormattingOptions.Stacked, options);
        Assert.Equal(Casing.Lower, options.Keywords);
        Assert.Equal(IdentifierQuoting.WhenRequired, options.Identifiers);
        Assert.Equal(2, options.IndentSize);
        Assert.Equal(3, options.MaxBlankLines);
    }

    [Fact]
    public void WithEditorConfig_LetsTheOtherKeysTuneTheStyle()
    {
        // The preset is applied first no matter where the keys sit in the file, because they are a
        // dictionary by the time the mapping sees them.
        var editorConfig = """
            [*]
            nquery_select_columns = wrap_when_too_long
            nquery_style = stacked
            """;

        var options = Apply(editorConfig);

        Assert.Equal(LayoutStyle.Stacked, options.Layout);
        Assert.Equal(ListStyle.WrapWhenTooLong, options.SelectColumns);
    }

    [Fact]
    public void WithEditorConfig_IgnoresAStyleItDoesNotKnow()
    {
        var editorConfig = """
            [*]
            nquery_style = river
            """;

        var options = Apply(editorConfig, FormattingOptions.Stacked);

        AssertPreset(FormattingOptions.Stacked, options);
    }

    [Fact]
    public void WithEditorConfig_ReadsKeywordColumn()
    {
        var editorConfig = """
            [*]
            nquery_keyword_column = 10
            """;

        var options = Apply(editorConfig);

        Assert.Equal(10, options.KeywordColumn);
    }

    [Fact]
    public void WithEditorConfig_ReadsSelectColumns()
    {
        var onePerLine = """
            [*]
            nquery_select_columns = one_per_line
            """;

        var wrapWhenTooLong = """
            [*]
            nquery_select_columns = wrap_when_too_long
            """;

        Assert.Equal(ListStyle.OnePerLine, Apply(onePerLine, FormattingOptions.Compact).SelectColumns);
        Assert.Equal(ListStyle.WrapWhenTooLong, Apply(wrapWhenTooLong).SelectColumns);
    }

    [Fact]
    public void WithEditorConfig_ReadsJoinIndentation()
    {
        var fromLevel = """
            [*]
            nquery_join_indentation = from_level
            """;

        var indented = """
            [*]
            nquery_join_indentation = indented
            """;

        Assert.Equal(JoinIndentation.AtFromLevel, Apply(fromLevel).Joins);
        Assert.Equal(JoinIndentation.Indented, Apply(indented, FormattingOptions.Stacked).Joins);
    }

    [Fact]
    public void WithEditorConfig_ReadsOnPlacement()
    {
        var sameLine = """
            [*]
            nquery_on_placement = same_line
            """;

        var ownLine = """
            [*]
            nquery_on_placement = own_line
            """;

        var ownLineWhenMultiple = """
            [*]
            nquery_on_placement = own_line_when_multiple
            """;

        var baseline = FormattingOptions.Default with { On = OnPlacement.OwnLine };

        Assert.Equal(OnPlacement.SameLine, Apply(sameLine, baseline).On);
        Assert.Equal(OnPlacement.OwnLine, Apply(ownLine).On);
        Assert.Equal(OnPlacement.OwnLineWhenMultiple, Apply(ownLineWhenMultiple).On);
    }

    [Fact]
    public void WithEditorConfig_ReadsKeywordCase()
    {
        var upper = """
            [*]
            nquery_keyword_case = upper
            """;

        var lower = """
            [*]
            nquery_keyword_case = lower
            """;

        var preserve = """
            [*]
            nquery_keyword_case = preserve
            """;

        var baseline = FormattingOptions.Default with { Keywords = Casing.Lower };

        Assert.Equal(Casing.Upper, Apply(upper, baseline).Keywords);
        Assert.Equal(Casing.Lower, Apply(lower).Keywords);
        Assert.Equal(Casing.Preserve, Apply(preserve).Keywords);
    }

    [Fact]
    public void WithEditorConfig_ReadsIdentifierQuoting()
    {
        var preserve = """
            [*]
            nquery_identifier_quoting = preserve
            """;

        var whenRequired = """
            [*]
            nquery_identifier_quoting = when_required
            """;

        var baseline = FormattingOptions.Default with { Identifiers = IdentifierQuoting.WhenRequired };

        Assert.Equal(IdentifierQuoting.Preserve, Apply(preserve, baseline).Identifiers);
        Assert.Equal(IdentifierQuoting.WhenRequired, Apply(whenRequired).Identifiers);
    }

    [Fact]
    public void WithEditorConfig_ReadsMaxBlankLines()
    {
        var editorConfig = """
            [*]
            nquery_max_blank_lines = 0
            """;

        var options = Apply(editorConfig);

        Assert.Equal(0, options.MaxBlankLines);
    }

    [Fact]
    public void WithEditorConfig_IgnoresValuesItCannotRead()
    {
        var editorConfig = """
            [*]
            nquery_select_columns = sometimes
            nquery_join_indentation = a_bit
            nquery_on_placement = nearby
            nquery_keyword_case = shouty
            nquery_identifier_quoting = always
            nquery_keyword_column = wide
            nquery_max_blank_lines = -1
            """;

        var options = Apply(editorConfig);

        Assert.Equal(FormattingOptions.Default, options);
    }

    [Fact]
    public void WithEditorConfig_IgnoresKeysItDoesNotKnow()
    {
        // Unknown keys are somebody else's, and there is no nquery_layout on purpose.
        var editorConfig = """
            [*]
            nquery_layout = stacked
            nquery_river = true
            dotnet_sort_system_directives_first = true
            """;

        var options = Apply(editorConfig);

        Assert.Equal(FormattingOptions.Default, options);
    }

    [Fact]
    public void WithEditorConfig_ReadsEveryPropertyItKnows()
    {
        var editorConfig = """
            [*.nql]
            indent_style = tab
            indent_size = 2
            max_line_length = 120
            insert_final_newline = false
            end_of_line = lf
            nquery_style = stacked
            nquery_keyword_column = 10
            nquery_select_columns = wrap_when_too_long
            nquery_join_indentation = indented
            nquery_on_placement = own_line_when_multiple
            nquery_keyword_case = preserve
            nquery_identifier_quoting = when_required
            nquery_max_blank_lines = 2
            """;

        var expected = FormattingOptions.Default with
        {
            UseTabs = true,
            IndentSize = 2,
            MaxLineLength = 120,
            InsertFinalNewline = false,
            NewLine = "\n",
            Layout = LayoutStyle.Stacked,
            KeywordColumn = 10,
            SelectColumns = ListStyle.WrapWhenTooLong,
            Joins = JoinIndentation.Indented,
            On = OnPlacement.OwnLineWhenMultiple,
            Keywords = Casing.Preserve,
            Identifiers = IdentifierQuoting.WhenRequired,
            MaxBlankLines = 2
        };

        var options = Apply(editorConfig);

        Assert.Equal(expected, options);
    }

    // Parsing does no I/O, so these paths only have to be well-formed, not real.
    private static readonly string RootDirectory = Path.GetFullPath(Path.Combine(Path.GetTempPath(), @"nquery-formatting"));

    private static FormattingOptions Apply(string editorConfig, FormattingOptions? options = null)
    {
        var configPath = Path.Combine(RootDirectory, EditorConfig.FileName);
        var filePath = Path.Combine(RootDirectory, @"query.nql");
        var config = EditorConfig.Parse(editorConfig, configPath, filePath);

        return (options ?? FormattingOptions.Default).WithEditorConfig(config);
    }

    // The four properties a preset decides, which is all nquery_style is allowed to touch.
    private static void AssertPreset(FormattingOptions expected, FormattingOptions actual)
    {
        Assert.Equal(expected.Layout, actual.Layout);
        Assert.Equal(expected.KeywordColumn, actual.KeywordColumn);
        Assert.Equal(expected.SelectColumns, actual.SelectColumns);
        Assert.Equal(expected.Joins, actual.Joins);
    }
}
