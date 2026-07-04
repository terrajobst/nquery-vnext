using System.Collections;
using System.Reflection;

using NQuery.CodeAnalysis;
using NQuery.CodeAnalysis.Syntax;

namespace NQuery.Tests.CodeAnalysis.Syntax;

// A data model of NQuery's grammar, reflected from the syntax-tree classes -- the
// single source consumed by the random generator, the EBNF doc emitter, and the
// coverage-driven corpus seeder. Because it is derived from the same node classes
// the parser produces, it can't drift from the language.
//
// A concrete node type is a Production whose right-hand side is the ordered list
// of its constructor's children (a token, a child node, or a list). An abstract
// node type is a Choice over its concrete subtypes.
internal abstract record GrammarSymbol
{
    public bool IsOptional { get; init; }

    public GrammarSymbol AsOptional() => this with { IsOptional = true };

    // Terminal. Kinds holds one kind for a fixed token, or several for a
    // "vocabulary" slot (e.g. binaryOperatorToken). Name is the constructor
    // parameter name, used to name the token-set rule in EBNF.
    public sealed record Token(string Name, IReadOnlyList<SyntaxKind> Kinds) : GrammarSymbol;

    // Reference to another production, or to an abstract type (a choice).
    public sealed record Node(Type NodeType) : GrammarSymbol;

    // A repeated element with a separator (SeparatedSyntaxList or a node list).
    public sealed record List(GrammarSymbol Element, string Separator) : GrammarSymbol;
}

internal sealed record Production(Type NodeType, IReadOnlyList<GrammarSymbol> Symbols)
{
    public string Name => RuleName(NodeType);

    public static string RuleName(Type nodeType)
    {
        var name = nodeType.Name;
        return name.EndsWith("Syntax", StringComparison.Ordinal) ? name[..^"Syntax".Length] : name;
    }
}

internal sealed class Grammar
{
    // Token slots whose parameter name denotes a *set* of kinds rather than a
    // single one. Same set => same parameter name; an unmapped name that also
    // fails to resolve to a single SyntaxKind throws when the grammar is built.
    public static readonly IReadOnlyDictionary<string, SyntaxKind[]> Vocabulary = new Dictionary<string, SyntaxKind[]>
    {
        ["binaryOperatorToken"] = [.. SyntaxFacts.GetBinaryExpressionTokenKinds()],
        ["unaryOperatorToken"] = [.. SyntaxFacts.GetUnaryExpressionTokenKinds()],
        ["comparisonOperatorToken"] =
        [
            SyntaxKind.EqualsToken, SyntaxKind.LessGreaterToken, SyntaxKind.ExclamationEqualsToken,
            SyntaxKind.LessToken, SyntaxKind.LessEqualToken, SyntaxKind.GreaterToken,
            SyntaxKind.GreaterEqualToken, SyntaxKind.ExclamationLessToken, SyntaxKind.ExclamationGreaterToken,
        ],
        ["quantifierKeyword"] = [SyntaxKind.AllKeyword, SyntaxKind.AnyKeyword, SyntaxKind.SomeKeyword],
        ["distinctAllKeyword"] = [SyntaxKind.DistinctKeyword, SyntaxKind.AllKeyword],
        ["joinTypeKeyword"] = [SyntaxKind.LeftKeyword, SyntaxKind.RightKeyword, SyntaxKind.FullKeyword],
        ["sortDirectionKeyword"] = [SyntaxKind.AscKeyword, SyntaxKind.DescKeyword],
        ["literalToken"] =
        [
            SyntaxKind.NumericLiteralToken, SyntaxKind.StringLiteralToken, SyntaxKind.DateLiteralToken,
            SyntaxKind.NullKeyword, SyntaxKind.TrueKeyword, SyntaxKind.FalseKeyword,
        ],
    };

    private Grammar(IReadOnlyDictionary<Type, Production> productions, IReadOnlyDictionary<Type, IReadOnlyList<Type>> choices)
    {
        Productions = productions;
        Choices = choices;
    }

    public IReadOnlyDictionary<Type, Production> Productions { get; }

    // Abstract node type -> its concrete alternatives.
    public IReadOnlyDictionary<Type, IReadOnlyList<Type>> Choices { get; }

    public static Type QueryRoot => typeof(QuerySyntax);

    public static Type ExpressionRoot => typeof(ExpressionSyntax);

    public static Grammar FromSyntaxTree()
    {
        var nullability = new NullabilityInfoContext();
        var nodeTypes = typeof(SyntaxNode).Assembly.GetTypes()
            .Where(t => typeof(SyntaxNode).IsAssignableFrom(t) && !t.IsAbstract)
            .ToArray();

        var productions = nodeTypes.ToDictionary(t => t, t => new Production(t, BuildSymbols(t, nullability)));

        var choices = new Dictionary<Type, IReadOnlyList<Type>>();
        foreach (var abstractBase in nodeTypes.SelectMany(AbstractBases).Distinct())
            choices[abstractBase] = nodeTypes.Where(abstractBase.IsAssignableFrom).ToArray();

        return new Grammar(productions, choices);
    }

    private static IReadOnlyList<GrammarSymbol> BuildSymbols(Type nodeType, NullabilityInfoContext nullability)
    {
        var symbols = new List<GrammarSymbol>();
        foreach (var p in ChildParameters(nodeType))
        {
            var symbol = BuildSymbol(nodeType, p);
            if (symbol is null)
                continue; // a non-syntax ctor param (e.g. LiteralExpression's object value)

            var optional = nullability.Create(p).WriteState == NullabilityState.Nullable;
            symbols.Add(optional ? symbol.AsOptional() : symbol);
        }

        return symbols;
    }

    private static GrammarSymbol? BuildSymbol(Type declaring, ParameterInfo p)
    {
        var type = p.ParameterType;

        if (type == typeof(SyntaxToken))
            return new GrammarSymbol.Token(p.Name!, ResolveKinds(declaring, p.Name!));

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(SeparatedSyntaxList<>))
            return new GrammarSymbol.List(new GrammarSymbol.Node(type.GetGenericArguments()[0]), ", ");

        if (typeof(IEnumerable).IsAssignableFrom(type) && type.IsGenericType &&
            typeof(SyntaxNode).IsAssignableFrom(type.GetGenericArguments()[0]))
            return new GrammarSymbol.List(new GrammarSymbol.Node(type.GetGenericArguments()[0]), " ");

        return typeof(SyntaxNode).IsAssignableFrom(type) ? new GrammarSymbol.Node(type) : null;
    }

    private static IReadOnlyList<SyntaxKind> ResolveKinds(Type declaring, string parameterName)
    {
        var pascal = char.ToUpperInvariant(parameterName[0]) + parameterName.Substring(1);
        if (Enum.TryParse<SyntaxKind>(pascal, out var kind) && kind.ToString() == pascal)
            return [kind];

        if (Vocabulary.TryGetValue(parameterName, out var kinds))
            return kinds;

        throw new InvalidOperationException(
            $"Token parameter '{Production.RuleName(declaring)}.{parameterName}' neither names a SyntaxKind " +
            $"nor is a known vocabulary slot. Rename it to '<kind>Token'/'<kind>Keyword', or add it to Grammar.Vocabulary.");
    }

    public static IEnumerable<ParameterInfo> ChildParameters(Type nodeType)
    {
        var ctor = nodeType.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
            .OrderByDescending(c => c.GetParameters().Length)
            .First();
        return ctor.GetParameters().Skip(1); // skip the leading SyntaxTree parameter
    }

    private static IEnumerable<Type> AbstractBases(Type t)
    {
        for (var b = t.BaseType; b is not null && b != typeof(SyntaxNode) && b.IsAbstract; b = b.BaseType)
            yield return b;
    }
}
