# Lexing and Parsing

The lexing and parsing phases produce a **syntax tree** (`SyntaxTree`) from raw source text.

## Lexer

The **Lexer** (`NQuery.Syntax.Lexer`) tokenizes `SourceText` into a sequence of `SyntaxToken` values. It is a standard hand-written lexer that recognizes:

- **Keywords**: `SELECT`, `FROM`, `WHERE`, `JOIN`, `GROUP BY`, `HAVING`, `ORDER BY`, `UNION`, `INTERSECT`, `EXCEPT`, `DISTINCT`, `TOP`, `AS`, `ON`, `AND`, `OR`, `NOT`, `IN`, `BETWEEN`, `LIKE`, `IS NULL`, `EXISTS`, `ALL`, `ANY`, `CAST`, `CASE`, `WHEN`, `THEN`, `ELSE`, `END`, `NULL`, `TRUE`, `FALSE`, `SOME`, and others.
- **Identifiers**: Regular and delimited (`[bracketed]`).
- **Literals**: Integer, floating-point, string (single-quoted), binary, date/time.
- **Operators**: Arithmetic (`+`, `-`, `*`, `/`, `%`), comparison (`=`, `<>`, `<`, `>`, `<=`, `>=`), logical (`AND`, `OR`, `NOT`), and others (`()`, `,`, `.`, `*`).
- **Comments**: Single-line (`--`) and multi-line (`/* */`).
- **Whitespace**: Spaces, tabs, newlines.

The **CharReader** (`NQuery.Syntax.CharReader`) is a character-by-character reader over `SourceText` that handles line break normalization.

## Parser

The **Parser** (`NQuery.Syntax.Parser`) is a hand-written recursive-descent parser that consumes tokens from the `Lexer` and produces a `SyntaxTree`. The full grammar it accepts is documented in [Grammar](../grammar.md). It parses the following grammar structure:

- **Query**: `SELECT` (optional `DISTINCT`/`TOP`) select-list `FROM` table-sources `WHERE` condition `GROUP BY` columns `HAVING` condition `ORDER BY` columns
- **Set operations**: `UNION`, `INTERSECT`, `EXCEPT` (with `ALL` variant)
- **Joins**: `INNER JOIN`, `LEFT/RIGHT/FULL OUTER JOIN`, `CROSS JOIN`, comma-separated
- **Subqueries**: Scalar, `EXISTS`, `IN`, `ANY`/`ALL` (correlated and uncorrelated)
- **Expressions**: Literals, column references, function calls, method invocations, property access, `CASE`, `CAST`, `COALESCE`, `NULLIF`, `BETWEEN`, `LIKE`, `IS NULL`, `IN`, binary/unary operators
- **Common Table Expressions (CTEs)**: `WITH` clause

## Type Hierarchy

All syntax nodes inherit from `SyntaxNode`:

```
SyntaxNode
├── CompilationUnitSyntax
├── CommonTableExpressionSyntax
├── CommonTableExpressionColumnNameSyntax
├── CommonTableExpressionColumnNameListSyntax
├── QuerySyntax
│   ├── CommonTableExpressionQuerySyntax
│   ├── SelectQuerySyntax
│   ├── OrderedQuerySyntax
│   ├── UnionQuerySyntax
│   ├── IntersectQuerySyntax
│   ├── ExceptQuerySyntax
│   └── ParenthesizedQuerySyntax
├── SelectClauseSyntax
├── SelectColumnSyntax
│   ├── ExpressionSelectColumnSyntax
│   └── WildcardSelectColumnSyntax
├── TopClauseSyntax
├── FromClauseSyntax
├── TableReferenceSyntax
│   ├── NamedTableReferenceSyntax
│   ├── DerivedTableReferenceSyntax
│   ├── ParenthesizedTableReferenceSyntax
│   └── JoinedTableReferenceSyntax
│       ├── CrossJoinedTableReferenceSyntax
│       └── ConditionedJoinedTableReferenceSyntax
│           ├── InnerJoinedTableReferenceSyntax
│           └── OuterJoinedTableReferenceSyntax
├── AliasSyntax
├── WhereClauseSyntax
├── GroupByClauseSyntax
├── GroupByColumnSyntax
├── HavingClauseSyntax
├── OrderByColumnSyntax
├── ArgumentListSyntax
├── ExpressionSyntax
│   ├── LiteralExpressionSyntax
│   ├── NameExpressionSyntax
│   ├── VariableExpressionSyntax
│   ├── UnaryExpressionSyntax
│   ├── BinaryExpressionSyntax
│   ├── ParenthesizedExpressionSyntax
│   ├── CastExpressionSyntax
│   ├── CoalesceExpressionSyntax
│   ├── NullIfExpressionSyntax
│   ├── CountAllExpressionSyntax
│   ├── FunctionInvocationExpressionSyntax
│   ├── MethodInvocationExpressionSyntax
│   ├── PropertyAccessExpressionSyntax
│   ├── InExpressionSyntax
│   ├── InQueryExpressionSyntax
│   ├── IsNullExpressionSyntax
│   ├── LikeExpressionSyntax
│   ├── SimilarToExpressionSyntax
│   ├── SoundsLikeExpressionSyntax
│   ├── CaseExpressionSyntax
│   └── SubselectExpressionSyntax
│       ├── AllAnySubselectSyntax
│       ├── ExistsSubselectSyntax
│       └── SingleRowSubselectSyntax
├── CaseLabelSyntax
├── CaseElseLabelSyntax
└── StructuredTriviaSyntax
    └── SkippedTokensTriviaSyntax
```

## Syntax Tree

The `SyntaxTree` (`NQuery.Syntax.SyntaxTree`) is the output of parsing. It is an immutable tree of `SyntaxNode` instances (approximately 55 node types). Key characteristics:

- **Syntax tokens** (`SyntaxToken`): Leaf nodes with `SyntaxKind`, positional span, and optional value.
- **Syntax trivia** (`SyntaxTrivia`): Whitespace, comments, and skipped tokens attached to tokens.
- **Diagnostics**: The syntax tree carries syntax errors as `Diagnostic` values accessible via `SyntaxTree.GetDiagnostics()`.

```csharp
public sealed class SyntaxTree
{
    public static SyntaxTree Parse(string text);
    public static SyntaxTree Parse(SourceText text);

    public SyntaxNode Root { get; }
    public SourceText Text { get; }
    public ImmutableArray<Diagnostic> GetDiagnostics();
    public T MatchBraces<T>(int position, ImmutableArray<BraceMatcher> matchers);
    public TextSpan ExtendSelection(TextSpan span);
    public CommentResult ToggleSingleLineComment(TextSpan span);
    public CommentResult ToggleMultiLineComment(TextSpan span);
}
```

`SyntaxTree` also exposes navigation, equivalence, and commenting APIs used by the authoring layer.
