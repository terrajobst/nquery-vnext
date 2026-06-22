using NQuery.CodeAnalysis;

namespace NQuery.Metadata;

public sealed class ParameterDefinition
{
    private ParameterDefinition(string name, Type type)
    {
        // Nullable<T> is erased to T at the metadata boundary; nullability is tracked separately
        // by the engine (see ColumnDefinition for the rationale).
        Name = name;
        Type = type.GetNonNullableType();
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
