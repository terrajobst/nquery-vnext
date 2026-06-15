using System.Collections.Immutable;
using System.Linq.Expressions;

using NQuery.Metadata;

namespace NQuery.Symbols;

public sealed class MethodSymbol : Symbol, IInvocableSymbol
{
    public MethodSymbol(MethodDefinition definition)
        : base(GetName(definition))
    {
        Definition = definition;
        ReturnType = definition.ReturnType;
        Parameters = definition.Parameters.Select(p => new ParameterSymbol(p.Name, p.Type)).ToImmutableArray();
    }

    private static string GetName(MethodDefinition definition)
    {
        ThrowIfNull(definition);

        return definition.Name;
    }

    public MethodDefinition Definition { get; }

    public override SymbolKind Kind
    {
        get { return SymbolKind.Method; }
    }

    public ImmutableArray<ParameterSymbol> Parameters { get; }

    public Type ReturnType { get; }

    public override Type Type
    {
        get { return ReturnType; }
    }

    internal Expression CreateInvocation(Expression instance, IEnumerable<Expression> arguments)
    {
        return Definition.CreateInvocation(instance, arguments);
    }
}
