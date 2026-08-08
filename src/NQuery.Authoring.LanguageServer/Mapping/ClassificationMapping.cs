using System.Collections.Immutable;

using NQuery.Authoring.Classifications;
using NQuery.Authoring.LanguageServer.Protocol;
using NQuery.CodeAnalysis.Text;

namespace NQuery.Authoring.LanguageServer.Mapping;

internal static class ClassificationMapping
{
    // Index into this list is what goes on the wire, so order is part of the protocol contract
    // with the client -- only ever append.
    private static readonly ImmutableArray<string> TokenTypes =
    [
        @"keyword",     // 0
        @"comment",     // 1
        @"string",      // 2
        @"number",      // 3
        @"operator",    // 4
        @"variable",    // 5
        @"property",    // 6
        @"function",    // 7
        @"method",      // 8
        @"class",       // 9
        @"struct"       // 10
    ];

    private const int TypeKeyword = 0;
    private const int TypeComment = 1;
    private const int TypeString = 2;
    private const int TypeNumber = 3;
    private const int TypeOperator = 4;
    private const int TypeVariable = 5;
    private const int TypeProperty = 6;
    private const int TypeFunction = 7;
    private const int TypeMethod = 8;
    private const int TypeClass = 9;
    private const int TypeStruct = 10;

    public static SemanticTokensLegend Legend { get; } = new()
    {
        TokenTypes = TokenTypes,
        TokenModifiers = []
    };

    // Syntax classification handles the non-identifier tokens; identifiers are deliberately left
    // to the semantic pass, so an identifier that binds to nothing emits no token at all and
    // falls through to the TextMate grammar rather than being mis-colored as a generic name.
    private static int? ToTokenType(SyntaxClassification classification)
    {
        return classification switch
        {
            SyntaxClassification.Comment => TypeComment,
            SyntaxClassification.Keyword => TypeKeyword,
            SyntaxClassification.Punctuation => TypeOperator,
            SyntaxClassification.StringLiteral => TypeString,
            SyntaxClassification.NumberLiteral => TypeNumber,
            _ => null
        };
    }

    private static int? ToTokenType(SemanticClassification classification)
    {
        return classification switch
        {
            SemanticClassification.SchemaTable => TypeClass,
            SemanticClassification.DerivedTable => TypeStruct,
            SemanticClassification.CommonTableExpression => TypeStruct,
            SemanticClassification.Column => TypeProperty,
            SemanticClassification.Function => TypeFunction,
            SemanticClassification.Aggregate => TypeFunction,
            SemanticClassification.Method => TypeMethod,
            SemanticClassification.Property => TypeProperty,
            SemanticClassification.Variable => TypeVariable,
            _ => null
        };
    }

    public static int[] Encode(SourceText text,
                               IReadOnlyList<SyntaxClassificationSpan> syntaxSpans,
                               IReadOnlyList<SemanticClassificationSpan> semanticSpans)
    {
        ThrowIfNull(text);
        ThrowIfNull(syntaxSpans);
        ThrowIfNull(semanticSpans);

        var tokens = new List<(int Line, int Start, int Length, int Type)>();

        foreach (var span in syntaxSpans)
        {
            var type = ToTokenType(span.Classification);
            if (type is not null)
                AddSplitByLine(tokens, text, span.Span, type.Value);
        }

        foreach (var span in semanticSpans)
        {
            var type = ToTokenType(span.Classification);
            if (type is not null)
                AddSplitByLine(tokens, text, span.Span, type.Value);
        }

        tokens.Sort(static (x, y) => x.Line != y.Line
            ? x.Line.CompareTo(y.Line)
            : x.Start.CompareTo(y.Start));

        var result = new int[tokens.Count * 5];
        var previousLine = 0;
        var previousStart = 0;
        var index = 0;

        foreach (var token in tokens)
        {
            var deltaLine = token.Line - previousLine;
            var deltaStart = deltaLine == 0 ? token.Start - previousStart : token.Start;

            result[index++] = deltaLine;
            result[index++] = deltaStart;
            result[index++] = token.Length;
            result[index++] = token.Type;
            result[index++] = 0;

            previousLine = token.Line;
            previousStart = token.Start;
        }

        return result;
    }

    // LSP forbids a semantic token from spanning lines, but block comments routinely do, so any
    // multi-line span is emitted as one token per line it covers.
    private static void AddSplitByLine(List<(int Line, int Start, int Length, int Type)> tokens,
                                       SourceText text,
                                       TextSpan span,
                                       int type)
    {
        if (span.Length == 0)
            return;

        var startLine = text.GetLineNumberFromPosition(span.Start);
        var endLine = text.GetLineNumberFromPosition(Math.Min(span.End, text.Length));

        for (var lineNumber = startLine; lineNumber <= endLine; lineNumber++)
        {
            var line = text.Lines[lineNumber];
            var start = Math.Max(span.Start, line.Span.Start);
            var end = Math.Min(span.End, line.Span.End);
            var length = end - start;

            if (length > 0)
                tokens.Add((lineNumber, start - line.Span.Start, length, type));
        }
    }
}
