using System.Collections.Immutable;
using System.Linq.Expressions;
using NQuery.Metadata;

namespace NQuery.Symbols;

public sealed class FunctionSymbol : Symbol, IInvocableSymbol
{
    internal FunctionSymbol(FunctionDefinition definition)
        : base(GetName(definition))
    {
        Definition = definition;
        ReturnType = definition.ReturnType;
        Parameters = definition.Parameters.Select(p => new ParameterSymbol(p.Name, p.Type)).ToImmutableArray();
    }

    private static string GetName(FunctionDefinition definition)
    {
        ThrowIfNull(definition);

        return definition.Name;
    }

    public FunctionDefinition Definition { get; }

    public override SymbolKind Kind
    {
        get { return SymbolKind.Function; }
    }

    public ImmutableArray<ParameterSymbol> Parameters { get; }

    public Type ReturnType { get; }

    public override Type Type
    {
        get { return ReturnType; }
    }

    internal Expression CreateInvocation(IEnumerable<Expression> arguments)
    {
        return Definition.CreateInvocation(arguments);
    }
}
