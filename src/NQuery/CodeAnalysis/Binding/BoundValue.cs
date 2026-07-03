namespace NQuery.CodeAnalysis.Binding;

// The value identity for an anonymous value the binder materializes -- a computed expression,
// an aggregate output, a set-op unified value, a subquery result/probe, an ORDER BY selector.
// Columns do not use this: a column symbol is its own identity.
//
// Reference identity IS the identity; Name is for diagnostics only and need not be unique.
// Unlike the algebra's ValueSlot, this has no factory back-reference and no Duplicate() --
// binder identities are never cloned (syntax isn't duplicated), so there is nothing to clone.
internal sealed class BoundValue : IBoundValue
{
    public BoundValue(string name, Type type)
    {
        ThrowIfNull(name);
        ThrowIfNull(type);

        Name = name;
        Type = type;
    }

    public string Name { get; }

    public Type Type { get; }

    public override string ToString()
    {
        return Name;
    }
}
