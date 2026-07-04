namespace NQuery.CodeAnalysis.Algebra;

internal sealed class ValueSlotFactory
{
    private const string TemporaryFormatString = @"Expr{0}";
    private readonly Dictionary<string, int> _usedNames = new();

    public ValueSlot Create(string formatString, Type type)
    {
        var number = _usedNames.AddOrUpdate(formatString, 1, (_, v) => v + 1);
        return new ValueSlot(this, formatString, number, type);
    }

    public ValueSlot CreateTemporary(Type type)
    {
        return Create(TemporaryFormatString, type);
    }

    public ValueSlot CreateNamed(string name, Type type)
    {
        var formatString = $"{name}:{{0}}";
        return Create(formatString, type);
    }
}
