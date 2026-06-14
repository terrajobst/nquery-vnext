using System.Linq.Expressions;

using NQuery.Metadata;

namespace NQuery.Symbols;

public sealed class MethodSymbol : InvocableSymbol
{
    public MethodSymbol(MethodDefinition definition)
        : base(GetName(definition), definition.ReturnType, definition.Parameters.Select(p => new ParameterSymbol(p.Name, p.Type)))
    {
        Definition = definition;
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

    internal Expression CreateInvocation(Expression instance, IEnumerable<Expression> arguments)
    {
        return Definition.CreateInvocation(instance, arguments);
    }
}
