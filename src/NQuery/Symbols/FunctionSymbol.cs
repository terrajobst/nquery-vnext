using System.Linq.Expressions;
using NQuery.Metadata;

namespace NQuery.Symbols;

public sealed class FunctionSymbol : InvocableSymbol
{
    public FunctionSymbol(FunctionDefinition definition)
        : base(GetName(definition), definition.ReturnType, definition.Parameters.Select(p => new ParameterSymbol(p.Name, p.Type)))
    {
        Definition = definition;
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

    internal Expression CreateInvocation(IEnumerable<Expression> arguments)
    {
        return Definition.CreateInvocation(arguments);
    }
}
