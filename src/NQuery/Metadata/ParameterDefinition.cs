namespace NQuery.Metadata;

public sealed class ParameterDefinition
{
    private ParameterDefinition(string name, Type type)
    {
        Name = name;
        Type = type;
    }

    public static ParameterDefinition Create(string name, Type type)
    {
        ThrowIfNull(name);
        ThrowIfNull(type);

        return new ParameterDefinition(name, type);
    }

    public string Name { get; }

    public Type Type { get; }
}
