namespace NQuery.CodeAnalysis.Symbols;

public sealed class ParameterSymbol : Symbol
{
    internal ParameterSymbol(string name, Type type)
        : base(name)
    {
        Type = type;
    }

    public override SymbolKind Kind
    {
        get { return SymbolKind.Parameter; }
    }

    public Type Type { get; }
}
