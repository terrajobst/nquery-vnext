using System.Text;

using NQuery.CodeAnalysis;
using NQuery.CodeAnalysis.Syntax;

namespace NQuery.Tests.CodeAnalysis.Syntax;

// Renders the shared Grammar model as an EBNF grammar file for documentation.
// Because the model is reflected from the syntax-tree classes, the emitted file
// always matches the parser -- a doc that can't go stale.
//
// Conventions: productions are snake_case, terminals are UPPER-CASE (keywords and
// value tokens) or 'quoted' (punctuation).
internal static class GrammarEbnfWriter
{
    public static string Write(Grammar grammar)
    {
        var sb = new StringBuilder();
        sb.AppendLine("// NQuery grammar (EBNF), generated from the syntax-tree classes.");
        sb.AppendLine("//   snake_case   production          'SELECT' '('     literal terminal (keyword / punctuation)");
        sb.AppendLine("//   a = b c ;     sequence            IDENTIFIER       lexical terminal (defined at end)");
        sb.AppendLine("//   a = b | c ;   choice              [ x ] optional   { x } zero or more");
        sb.AppendLine();

        // Abstract choices first (query, expression, table_reference, ...), roots on top.
        foreach (var (abstractType, alternatives) in grammar.Choices.OrderBy(c => Order(c.Key)).ThenBy(c => Rule(c.Key)))
            WriteRule(sb, Rule(abstractType), alternatives.Select(Rule).OrderBy(n => n).ToArray());

        // Concrete productions.
        foreach (var (_, production) in grammar.Productions.OrderBy(p => Rule(p.Key)))
        {
            var rhs = production.Symbols.Count == 0
                ? "(empty)"
                : string.Join(" ", production.Symbols.Select(Render));
            WriteRule(sb, Rule(production.NodeType), [rhs]);
        }

        // Token-set (vocabulary) rules referenced above.
        foreach (var (name, kinds) in Grammar.Vocabulary.OrderBy(v => v.Key))
            WriteRule(sb, Snake(name), kinds.Select(RenderKind).ToArray());

        WriteLexicalTerminals(sb);

        return sb.ToString();
    }

    // The value tokens (IDENTIFIER/NUMBER/STRING/DATE) come from the lexer, not the
    // syntax-tree classes, so they can't be reflected. Hard-code their definitions
    // here (kept in sync with Lexer.cs by hand) to keep the grammar self-contained.
    private static void WriteLexicalTerminals(StringBuilder sb)
    {
        sb.AppendLine("// Lexical terminals -- produced by the lexer.");
        sb.AppendLine("//   ? x ?   a character class described in prose");
        sb.AppendLine();

        WriteRule(sb, "IDENTIFIER",
        [
            "( LETTER | '_' ) { LETTER | DIGIT | '_' | '$' }",
            "'\"' { ? any char except '\"' ? | '\"\"' } '\"'",
            "'[' { ? any char except ']' ? | ']]' } ']'",
        ]);
        WriteRule(sb, "NUMBER",
        [
            "DIGIT { DIGIT } [ '.' DIGIT { DIGIT } ] [ EXPONENT ]",
            "'.' DIGIT { DIGIT } [ EXPONENT ]",
        ]);
        WriteRule(sb, "EXPONENT", ["( 'e' | 'E' ) [ '+' | '-' ] DIGIT { DIGIT }"]);
        WriteRule(sb, "STRING", ["\"'\" { ? any char except \"'\" ? | \"''\" } \"'\""]);
        WriteRule(sb, "DATE", ["'#' { ? any char except '#' or newline ? } '#'"]);
        WriteRule(sb, "LETTER", ["? any Unicode letter ?"]);
        WriteRule(sb, "DIGIT", ["'0' | '1' | '2' | '3' | '4' | '5' | '6' | '7' | '8' | '9'"]);
    }

    // name
    //     = firstAlternative
    //     | nextAlternative
    //     ;
    private static void WriteRule(StringBuilder sb, string name, IReadOnlyList<string> alternatives)
    {
        sb.AppendLine(name);
        for (var i = 0; i < alternatives.Count; i++)
            sb.AppendLine($"    {(i == 0 ? '=' : '|')} {alternatives[i]}");
        sb.AppendLine("    ;");
        sb.AppendLine();
    }

    private static string Render(GrammarSymbol symbol)
    {
        var text = symbol switch
        {
            GrammarSymbol.Token token => token.Kinds.Count > 1 ? Snake(token.Name) : RenderKind(token.Kinds[0]),
            GrammarSymbol.Node node => Rule(node.NodeType),
            GrammarSymbol.List list => $"{{ {Render(list.Element)} }}",
            _ => "?",
        };

        return symbol.IsOptional ? $"[ {text} ]" : text;
    }

    private static string RenderKind(SyntaxKind kind) => kind switch
    {
        SyntaxKind.IdentifierToken => "IDENTIFIER",
        SyntaxKind.NumericLiteralToken => "NUMBER",
        SyntaxKind.StringLiteralToken => "STRING",
        SyntaxKind.DateLiteralToken => "DATE",
        _ => $"'{kind.GetText()}'", // keywords and punctuation as quoted literals
    };

    private static string Rule(Type nodeType) => Snake(Production.RuleName(nodeType));

    private static string Snake(string name)
    {
        var sb = new StringBuilder(name.Length + 8);
        for (var i = 0; i < name.Length; i++)
        {
            var c = name[i];
            if (char.IsUpper(c))
            {
                if (i > 0)
                    sb.Append('_');
                sb.Append(char.ToLowerInvariant(c));
            }
            else
            {
                sb.Append(c);
            }
        }

        return sb.ToString();
    }

    // Roots (query, expression) sort first, then everything else.
    private static int Order(Type t) =>
        t == Grammar.QueryRoot ? 0 : t == Grammar.ExpressionRoot ? 1 : 2;
}
