namespace NQuery.CodeAnalysis.Algebra;

// The identity connecting a LogicalRecursiveUnion (the recursion driver) to its
// LogicalRecursiveReference leaves (the working-table scans). The reference is a
// back-edge: it lives inside the union's recursive-member subtrees but denotes the
// union's own row set, so the association cannot be structural -- both sides carry
// the same token instead, and every layer (logical, physical, emit) resolves the
// pairing by token identity.
//
// The token is pure identity (reference equality); the name is the CTE's name and
// exists only for show plans and diagnostics.
internal sealed class RecursionToken
{
    public RecursionToken(string name)
    {
        ThrowIfNull(name);

        Name = name;
    }

    public string Name { get; }

    public override string ToString()
    {
        return Name;
    }
}
