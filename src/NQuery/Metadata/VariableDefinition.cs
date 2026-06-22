using NQuery.CodeAnalysis;

namespace NQuery.Metadata;

public sealed class VariableDefinition
{
    private readonly Type _type;

    private object? _value;

    private VariableDefinition(string name, Type type, object? value)
    {
        // Nullable<T> is erased to T at the metadata boundary; nullability is tracked separately
        // by the engine (see ColumnDefinition for the rationale).
        Name = name;
        _type = type.GetNonNullableType();
        Value = value;
    }

    public static VariableDefinition Create(string name, Type type)
    {
        return Create(name, type, null);
    }

    public static VariableDefinition Create(string name, Type type, object? value)
    {
        ThrowIfNull(name);
        ThrowIfNull(type);

        return new VariableDefinition(name, type, value);
    }

    public string Name { get; }

    public Type Type
    {
        get { return _type; }
    }

    public object? Value
    {
        get { return _value; }
        set
        {
            if (value is not null && !_type.IsInstanceOfType(value))
                throw new ArgumentException(string.Format(Resources.VariableValueTypeMismatch, value, _type), nameof(value));

            _value = value;
        }
    }
}
