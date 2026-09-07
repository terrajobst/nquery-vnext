using NQuery.Authoring.Configuration;

namespace NQuery.Authoring.Tests.Configuration;

public class EditorConfigTests
{
    [Fact]
    public void EditorConfig_Empty_HasNoProperties()
    {
        Assert.Empty(EditorConfig.Empty.Properties);
        Assert.False(EditorConfig.Empty.TryGetString(@"indent_size", out _));
    }

    [Fact]
    public void EditorConfig_Parse_LowercasesKeys()
    {
        var config = Parse("""
            [*]
            Indent_Size = 4
            """);

        Assert.Equal(@"indent_size", config.Properties.Keys.Single());
    }

    [Fact]
    public void EditorConfig_Parse_LooksUpKeysCaseInsensitively()
    {
        var config = Parse("""
            [*]
            indent_size = 4
            """);

        Assert.True(config.TryGetString(@"INDENT_SIZE", out var value));
        Assert.Equal(@"4", value);
    }

    [Fact]
    public void EditorConfig_Parse_PreservesValueCase()
    {
        // Plenty of properties are free-form text, so lowercasing values -- which the spec doesn't
        // ask for -- would corrupt them.
        var config = Parse("""
            [*]
            file_header_template = Copyright (c) Immo Landwerth
            """);

        Assert.True(config.TryGetString(@"file_header_template", out var value));
        Assert.Equal(@"Copyright (c) Immo Landwerth", value);
    }

    [Fact]
    public void EditorConfig_Parse_TrimsAroundKeysAndValues()
    {
        var config = Parse("[*]\n   indent_size   =   4   \n");

        Assert.True(config.TryGetString(@"indent_size", out var value));
        Assert.Equal(@"4", value);
    }

    [Fact]
    public void EditorConfig_Parse_KeepsWhitespaceInsideValues()
    {
        var config = Parse("""
            [*]
            csharp_new_line_before_open_brace = methods, lambdas, types
            """);

        Assert.True(config.TryGetString(@"csharp_new_line_before_open_brace", out var value));
        Assert.Equal(@"methods, lambdas, types", value);
    }

    [Fact]
    public void EditorConfig_Parse_IgnoresComments()
    {
        var config = Parse("""
            # a hash comment
            [*]
            ; a semicolon comment
            indent_size = 4
            """);

        Assert.Equal(1, config.Properties.Count);
        Assert.True(config.TryGetString(@"indent_size", out var value));
        Assert.Equal(@"4", value);
    }

    [Fact]
    public void EditorConfig_Parse_KeepsCommentCharactersInsideValues()
    {
        // A ; or # only starts a comment at the beginning of a line.
        var config = Parse("""
            [*]
            comment_prefix = # not a comment
            """);

        Assert.True(config.TryGetString(@"comment_prefix", out var value));
        Assert.Equal(@"# not a comment", value);
    }

    [Fact]
    public void EditorConfig_Parse_IgnoresPreamblePairs()
    {
        // Only root has any effect outside a section, and root isn't a property of the file.
        var config = Parse("""
            root = true
            indent_size = 4
            """);

        Assert.Empty(config.Properties);
    }

    [Fact]
    public void EditorConfig_Parse_IgnoresLinesThatArentPairs()
    {
        var config = Parse("""
            [*]
            this line has no separator
            indent_size = 4
            """);

        Assert.Equal(@"indent_size", config.Properties.Keys.Single());
    }

    [Fact]
    public void EditorConfig_Parse_SplitsAtTheFirstSeparator()
    {
        var config = Parse("""
            [*]
            key = a = b
            """);

        Assert.True(config.TryGetString(@"key", out var value));
        Assert.Equal(@"a = b", value);
    }

    [Fact]
    public void EditorConfig_Parse_LetsTheLaterSectionWin()
    {
        var config = Parse("""
            [*]
            indent_size = 2

            [*.nql]
            indent_size = 4
            """);

        Assert.True(config.TryGetString(@"indent_size", out var value));
        Assert.Equal(@"4", value);
    }

    [Fact]
    public void EditorConfig_Parse_LetsTheLaterPairWin()
    {
        var config = Parse("""
            [*]
            indent_size = 2
            indent_size = 4
            """);

        Assert.True(config.TryGetString(@"indent_size", out var value));
        Assert.Equal(@"4", value);
    }

    [Fact]
    public void EditorConfig_Parse_IgnoresSectionsThatDontMatch()
    {
        var config = Parse("""
            [*.nqe]
            indent_size = 2
            """);

        Assert.Empty(config.Properties);
    }

    [Fact]
    public void EditorConfig_Parse_TakesBackUnsetProperties()
    {
        var config = Parse("""
            [*]
            indent_size = 2

            [*.nql]
            indent_size = UNSET
            """);

        Assert.False(config.TryGetString(@"indent_size", out _));
    }

    [Fact]
    public void EditorConfig_TryGetString_ReturnsFalseForMissingKey()
    {
        var config = Parse("""
            [*]
            indent_size = 4
            """);

        Assert.False(config.TryGetString(@"max_line_length", out _));
    }

    [Fact]
    public void EditorConfig_TryGetInt32_ParsesIntegers()
    {
        var config = Parse("""
            [*]
            indent_size = 4
            max_line_length = -1
            """);

        Assert.True(config.TryGetInt32(@"indent_size", out var indentSize));
        Assert.Equal(4, indentSize);

        Assert.True(config.TryGetInt32(@"max_line_length", out var maxLineLength));
        Assert.Equal(-1, maxLineLength);
    }

    [Fact]
    public void EditorConfig_TryGetInt32_ReturnsFalseForValuesThatArentIntegers()
    {
        // indent_size = tab and max_line_length = off are the standard properties that aren't
        // numbers. Saying no here is what lets a caller fall back to tab_width.
        var config = Parse("""
            [*]
            indent_size = tab
            max_line_length = off
            """);

        Assert.False(config.TryGetInt32(@"indent_size", out _));
        Assert.False(config.TryGetInt32(@"max_line_length", out _));
    }

    [Fact]
    public void EditorConfig_TryGetBoolean_ParsesTrueAndFalse()
    {
        var config = Parse("""
            [*]
            insert_final_newline = TRUE
            trim_trailing_whitespace = False
            """);

        Assert.True(config.TryGetBoolean(@"insert_final_newline", out var insertFinalNewline));
        Assert.True(insertFinalNewline);

        Assert.True(config.TryGetBoolean(@"trim_trailing_whitespace", out var trimTrailingWhitespace));
        Assert.False(trimTrailingWhitespace);
    }

    [Fact]
    public void EditorConfig_TryGetBoolean_ReturnsFalseForOtherSpellings()
    {
        var config = Parse("""
            [*]
            insert_final_newline = yes
            trim_trailing_whitespace = 1
            """);

        Assert.False(config.TryGetBoolean(@"insert_final_newline", out _));
        Assert.False(config.TryGetBoolean(@"trim_trailing_whitespace", out _));
    }

    [Fact]
    public void EditorConfig_Glob_StarMatchesAnyName()
    {
        Assert.True(Matches(@"*", @"query.nql"));
        Assert.True(Matches(@"*", @"src/query.nql"));
    }

    [Fact]
    public void EditorConfig_Glob_MatchesExtensions()
    {
        Assert.True(Matches(@"*.nql", @"query.nql"));
        Assert.False(Matches(@"*.nql", @"query.nqe"));
    }

    [Fact]
    public void EditorConfig_Glob_MatchesAtAnyDepthWithoutASeparator()
    {
        Assert.True(Matches(@"*.nql", @"src/nested/query.nql"));
        Assert.True(Matches(@"query.nql", @"src/query.nql"));
    }

    [Fact]
    public void EditorConfig_Glob_AnchorsPatternsWithASeparator()
    {
        Assert.True(Matches(@"src/*.nql", @"src/query.nql"));
        Assert.False(Matches(@"src/*.nql", @"query.nql"));
        Assert.False(Matches(@"src/*.nql", @"other/src/query.nql"));
    }

    [Fact]
    public void EditorConfig_Glob_StarDoesNotCrossDirectories()
    {
        Assert.False(Matches(@"src/*.nql", @"src/nested/query.nql"));
    }

    [Fact]
    public void EditorConfig_Glob_DoubleStarCrossesDirectories()
    {
        Assert.True(Matches(@"src/**.nql", @"src/nested/query.nql"));
    }

    [Fact]
    public void EditorConfig_Glob_DoubleStarSwallowsItsSeparator()
    {
        // [**/*.nql] is how everyone writes "every query", and it has to cover the file sitting
        // next to the config as well as the ones below it.
        Assert.True(Matches(@"**/*.nql", @"query.nql"));
        Assert.True(Matches(@"**/*.nql", @"src/nested/query.nql"));
    }

    [Fact]
    public void EditorConfig_Glob_LeadingSlashAnchorsToTheConfigDirectory()
    {
        Assert.True(Matches(@"/query.nql", @"query.nql"));
        Assert.False(Matches(@"/query.nql", @"src/query.nql"));
    }

    [Fact]
    public void EditorConfig_Glob_QuestionMarkMatchesOneCharacter()
    {
        Assert.True(Matches(@"?uery.nql", @"query.nql"));
        Assert.False(Matches(@"?.nql", @"query.nql"));
        Assert.False(Matches(@"src?query.nql", @"src/query.nql"));
    }

    [Fact]
    public void EditorConfig_Glob_MatchesCharacterClasses()
    {
        Assert.True(Matches(@"[qr]uery.nql", @"query.nql"));
        Assert.False(Matches(@"[rs]uery.nql", @"query.nql"));
    }

    [Fact]
    public void EditorConfig_Glob_MatchesNegatedCharacterClasses()
    {
        Assert.True(Matches(@"[!rs]uery.nql", @"query.nql"));
        Assert.False(Matches(@"[!qr]uery.nql", @"query.nql"));
    }

    [Fact]
    public void EditorConfig_Glob_MatchesCharacterRanges()
    {
        Assert.True(Matches(@"query[0-9].nql", @"query7.nql"));
        Assert.False(Matches(@"query[0-9].nql", @"queryx.nql"));
    }

    [Fact]
    public void EditorConfig_Glob_MatchesAlternations()
    {
        Assert.True(Matches(@"*.{nql,nqe}", @"query.nql"));
        Assert.True(Matches(@"*.{nql,nqe}", @"query.nqe"));
        Assert.False(Matches(@"*.{nql,nqe}", @"query.txt"));
    }

    [Fact]
    public void EditorConfig_Glob_MatchesNestedAlternations()
    {
        Assert.True(Matches(@"{query,{a,b}}.nql", @"b.nql"));
        Assert.False(Matches(@"{query,{a,b}}.nql", @"c.nql"));
    }

    [Fact]
    public void EditorConfig_Glob_MatchesAlternationsThatSpanDirectories()
    {
        Assert.True(Matches(@"{src/*.nql,*.nqe}", @"src/query.nql"));
    }

    [Fact]
    public void EditorConfig_Glob_TreatsBracesWithoutACommaAsLiteral()
    {
        // Which is also how the unimplemented numeric range ends up: matching nothing rather than
        // matching the wrong thing.
        Assert.False(Matches(@"query{1..9}.nql", @"query7.nql"));
        Assert.True(Matches(@"query{1..9}.nql", @"query{1..9}.nql"));
    }

    [Fact]
    public void EditorConfig_Glob_MatchesCaseInsensitively()
    {
        // The spec doesn't say, and a .NQL file that missed [*.nql] would only read as a bug.
        Assert.True(Matches(@"*.NQL", @"query.nql"));
    }

    [Fact]
    public void EditorConfig_Glob_HonorsEscapes()
    {
        // Not a * or a ?, deliberately: .NET Framework's Path.GetFullPath rejects those outright,
        // so a file can't be named after one.
        Assert.True(Matches(@"query\[1].nql", @"query[1].nql"));
        Assert.True(Matches(@"query[1].nql", @"query1.nql"));
        Assert.False(Matches(@"query[1].nql", @"query[1].nql"));
    }

    [Fact]
    public void EditorConfig_Glob_TreatsUnterminatedClassesAsLiteral()
    {
        Assert.True(Matches(@"[.nql", @"[.nql"));
    }

    [Fact]
    public void EditorConfig_Glob_DoesNotAnchorPatternsWhoseSlashIsInAClass()
    {
        Assert.True(Matches(@"query[/-]1.nql", @"src/query-1.nql"));
    }

    [Fact]
    public void EditorConfig_Glob_OnlyMatchesNamesForFilesOutsideTheConfigDirectory()
    {
        var configPath = Path.Combine(RootDirectory, EditorConfig.FileName);
        var filePath = Path.GetFullPath(Path.Combine(Path.GetTempPath(), @"elsewhere", @"query.nql"));

        Assert.True(EditorConfig.Parse("[*.nql]\nindent_size = 4\n", configPath, filePath).TryGetString(@"indent_size", out _));
        Assert.False(EditorConfig.Parse("[src/*.nql]\nindent_size = 4\n", configPath, filePath).TryGetString(@"indent_size", out _));
    }

    [Fact]
    public void EditorConfig_Load_ReadsTheGivenFile()
    {
        using var directory = new TempDirectory();

        var configPath = directory.CreateFile(EditorConfig.FileName, """
            [*.nql]
            nquery_test_key = value
            """);

        var config = EditorConfig.Load(configPath, directory.GetPath(@"query.nql"));

        Assert.True(config.TryGetString(@"nquery_test_key", out var value));
        Assert.Equal(@"value", value);
    }

    [Fact]
    public void EditorConfig_Load_IgnoresRootAndDoesNotWalk()
    {
        using var directory = new TempDirectory();

        directory.CreateFile(EditorConfig.FileName, """
            root = true

            [*.nql]
            nquery_test_key = outer
            """);

        var configPath = directory.CreateFile(@"src/.editorconfig", """
            [*.nql]
            nquery_test_other = inner
            """);

        var config = EditorConfig.Load(configPath, directory.GetPath(@"src/query.nql"));

        Assert.True(config.TryGetString(@"nquery_test_other", out _));
        Assert.False(config.TryGetString(@"nquery_test_key", out _));
    }

    [Fact]
    public void EditorConfig_LoadForFile_LetsTheNearerConfigWin()
    {
        using var directory = new TempDirectory();

        directory.CreateFile(EditorConfig.FileName, """
            root = true

            [*.nql]
            nquery_test_key = outer
            nquery_test_outer_only = outer
            """);

        directory.CreateFile(@"src/.editorconfig", """
            [*.nql]
            nquery_test_key = inner
            """);

        var config = EditorConfig.LoadForFile(directory.GetPath(@"src/query.nql"));

        Assert.True(config.TryGetString(@"nquery_test_key", out var key));
        Assert.Equal(@"inner", key);

        // The farther config still contributes everything the nearer one is silent about.
        Assert.True(config.TryGetString(@"nquery_test_outer_only", out var outerOnly));
        Assert.Equal(@"outer", outerOnly);
    }

    [Fact]
    public void EditorConfig_LoadForFile_SkipsDirectoriesWithoutAConfig()
    {
        using var directory = new TempDirectory();

        directory.CreateFile(EditorConfig.FileName, """
            root = true

            [*.nql]
            nquery_test_key = outer
            """);

        var config = EditorConfig.LoadForFile(directory.GetPath(@"src/nested/query.nql"));

        Assert.True(config.TryGetString(@"nquery_test_key", out var value));
        Assert.Equal(@"outer", value);
    }

    [Fact]
    public void EditorConfig_LoadForFile_StopsAtRoot()
    {
        using var directory = new TempDirectory();

        directory.CreateFile(EditorConfig.FileName, """
            [*.nql]
            nquery_test_outer_only = outer
            """);

        directory.CreateFile(@"src/.editorconfig", """
            root = true

            [*.nql]
            nquery_test_key = inner
            """);

        var config = EditorConfig.LoadForFile(directory.GetPath(@"src/query.nql"));

        Assert.True(config.TryGetString(@"nquery_test_key", out _));
        Assert.False(config.TryGetString(@"nquery_test_outer_only", out _));
    }

    [Fact]
    public void EditorConfig_LoadForFile_MatchesSectionsAgainstThePathBelowEachConfig()
    {
        // The same pattern is anchored at a different directory in each file, which is the whole
        // reason a config can't be resolved without knowing where it lives.
        using var directory = new TempDirectory();

        directory.CreateFile(EditorConfig.FileName, """
            root = true

            [src/*.nql]
            nquery_test_outer = yes
            """);

        directory.CreateFile(@"src/.editorconfig", """
            [src/*.nql]
            nquery_test_inner = yes
            """);

        var config = EditorConfig.LoadForFile(directory.GetPath(@"src/query.nql"));

        Assert.True(config.TryGetString(@"nquery_test_outer", out _));
        Assert.False(config.TryGetString(@"nquery_test_inner", out _));
    }

    [Fact]
    public void EditorConfig_LoadForFile_DoesNotRequireTheFileToExist()
    {
        // An editor asks about a document, and a document that was never saved still has a name.
        using var directory = new TempDirectory();

        directory.CreateFile(EditorConfig.FileName, """
            root = true

            [*.nql]
            nquery_test_key = value
            """);

        var config = EditorConfig.LoadForFile(directory.GetPath(@"never-saved.nql"));

        Assert.True(config.TryGetString(@"nquery_test_key", out _));
    }

    [Fact]
    public void EditorConfig_LoadForFile_FindsNothingWithoutAConfig()
    {
        using var directory = new TempDirectory();

        var config = EditorConfig.LoadForFile(directory.GetPath(@"query.nql"));

        // Not asserting emptiness: the temp directory has real parents, and one of them may well
        // carry an .editorconfig of its own.
        Assert.False(config.TryGetString(@"nquery_test_key", out _));
    }

    // Parsing does no I/O, so this only has to be a well-formed rooted path, not a real one.
    private static readonly string RootDirectory = Path.GetFullPath(Path.Combine(Path.GetTempPath(), @"nquery-editorconfig"));

    private static EditorConfig Parse(string text, string relativeFilePath = @"query.nql")
    {
        var configPath = Path.Combine(RootDirectory, EditorConfig.FileName);
        var filePath = Path.Combine(RootDirectory, relativeFilePath.Replace('/', Path.DirectorySeparatorChar));

        return EditorConfig.Parse(text, configPath, filePath);
    }

    private static bool Matches(string pattern, string relativeFilePath)
    {
        var text = $"[{pattern}]{Environment.NewLine}nquery_test_key = value";

        return Parse(text, relativeFilePath).TryGetString(@"nquery_test_key", out _);
    }

    private sealed class TempDirectory : IDisposable
    {
        public TempDirectory()
        {
            Root = Path.Combine(Path.GetTempPath(), $"nquery-{Guid.NewGuid():N}");
            Directory.CreateDirectory(Root);
        }

        public string Root { get; }

        public string CreateFile(string relativePath, string content)
        {
            var path = GetPath(relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, content);
            return path;
        }

        public string GetPath(string relativePath)
        {
            return Path.Combine(Root, relativePath.Replace('/', Path.DirectorySeparatorChar));
        }

        public void Dispose()
        {
            Directory.Delete(Root, recursive: true);
        }
    }
}
