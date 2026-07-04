using System.Linq.Expressions;
using System.Reflection;

using NQuery.CodeAnalysis;

namespace NQuery.Metadata;

public abstract class VariableDefinition
{
    private protected VariableDefinition(string name, Type type)
    {
        // Nullable<T> is erased to T at the metadata boundary; nullability is tracked separately
        // by the engine (see ColumnDefinition for the rationale). Capture whether the caller's type
        // was a nullable value type before erasing it -- otherwise the erased Type can no longer tell
        // VariableDefinition<int?> (accepts null) from VariableDefinition<int> (does not).
        Name = name;
        IsNullableValueType = type.IsNullableOfT();
        Type = type.GetNonNullableType();
    }

    public string Name { get; }

    public Type Type { get; }

    // Whether the type the variable was created from was a nullable value type (Nullable<T>), captured
    // before Type was erased to its non-nullable form.
    private protected bool IsNullableValueType { get; }

    // Non-virtual entry point for reading/writing the value as object. It validates the value
    // against the variable's type and delegates the actual storage to ValueCore; the strongly-typed
    // VariableDefinition<T> owns its value and skips the (redundant) validation.
    public object? Value
    {
        get { return ValueCore; }
        set
        {
            if (value is null)
            {
                // Type is erased to its non-nullable form, so it can't tell VariableDefinition<int>
                // from VariableDefinition<int?>; whether null fits is a per-definition capability.
                if (!CanValueBeNull)
                    throw new ArgumentNullException(nameof(value), string.Format(Resources.VariableValueCannotBeNull, Type));
            }
            else if (!Type.IsInstanceOfType(value))
            {
                throw new ArgumentException(string.Format(Resources.VariableValueTypeMismatch, value, Type), nameof(value));
            }

            ValueCore = value;
        }
    }

    internal abstract object? ValueCore { get; set; }

    // Whether a null value can be stored. The object-typed variable always can (its slot is object);
    // the strongly-typed VariableDefinition<T> can only when T itself admits null -- a reference type
    // or Nullable<T>, but not a bare value type such as int.
    private protected abstract bool CanValueBeNull { get; }

    // Produces an expression tree that reads the current value. VariableDefinition<T> reads its
    // typed value directly (no boxing); the object-typed variable reads its object slot. The
    // emitter converts the result to the variable's nullable shape.
    internal abstract Expression CreateInvocation();

    public static VariableDefinition Create(string name, Type type)
    {
        return Create(name, type, null);
    }

    public static VariableDefinition Create(string name, Type type, object? value)
    {
        ThrowIfNull(name);
        ThrowIfNull(type);

        return new ObjectVariableDefinition(name, type) { Value = value };
    }

    public static VariableDefinition<T> Create<T>(string name)
    {
        return Create<T>(name, default);
    }

    public static VariableDefinition<T> Create<T>(string name, T? value)
    {
        ThrowIfNull(name);

        return new VariableDefinition<T>(name) { Value = value };
    }

    private sealed class ObjectVariableDefinition : VariableDefinition
    {
        private static readonly PropertyInfo ValueProperty = typeof(VariableDefinition).GetProperty(nameof(Value), typeof(object))!;

        public ObjectVariableDefinition(string name, Type type)
            : base(name, type)
        {
        }

        internal override object? ValueCore { get; set; }

        // The object slot always holds null; erasure means we can't know whether the caller
        // intended a nullable type, so the loosely-typed path stays permissive.
        private protected override bool CanValueBeNull
        {
            get { return true; }
        }

        internal override Expression CreateInvocation()
        {
            return Expression.Property(Expression.Constant(this), ValueProperty);
        }
    }
}

public sealed class VariableDefinition<T> : VariableDefinition
{
    private static readonly PropertyInfo ValueProperty = typeof(VariableDefinition<T>).GetProperty(nameof(Value), typeof(T))!;

    public VariableDefinition(string name)
        : base(GetName(name), typeof(T))
    {
    }

    private static string GetName(string name)
    {
        ThrowIfNull(name);

        return name;
    }

    // Owns the value in its own CLR type, so a strongly-typed variable never boxes. The base
    // Value property validates and delegates here through ValueCore.
    public new T? Value { get; set; }

    internal override object? ValueCore
    {
        get { return Value; }
        set { Value = (T?)value; }
    }

    // A bare value type (e.g. int) can't hold null; a reference type or Nullable<T> can. Type is
    // erased to non-nullable, so combine its runtime kind with the nullability captured at construction.
    private protected override bool CanValueBeNull
    {
        get { return !Type.IsValueType || IsNullableValueType; }
    }

    internal override Expression CreateInvocation()
    {
        return Expression.Property(Expression.Constant(this), ValueProperty);
    }
}
