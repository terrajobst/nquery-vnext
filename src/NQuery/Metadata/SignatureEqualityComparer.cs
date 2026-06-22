using System.Runtime.CompilerServices;

namespace NQuery.Metadata;

// Compares functions by their signature: name (case-insensitive) plus parameter types compared by
// reference. Parameter types are already erased (Nullable<T> -> T), so two functions whose
// parameters differed only by nullability share a signature here. The hash is precomputed once per
// function (FunctionDefinition.SignatureHashCode) and used both as the comparer's hash and as a fast
// reject in Equals.
internal sealed class SignatureEqualityComparer : IEqualityComparer<FunctionDefinition>
{
    public static SignatureEqualityComparer Instance { get; } = new();

    public static int GetHashCode(FunctionDefinition function)
    {
        var hash = new HashCode();
        hash.Add(function.Name, StringComparer.OrdinalIgnoreCase);

        foreach (var parameter in function.Parameters)
            hash.Add(RuntimeHelpers.GetHashCode(parameter.Type));

        return hash.ToHashCode();
    }

    public bool Equals(FunctionDefinition? x, FunctionDefinition? y)
    {
        if (ReferenceEquals(x, y))
            return true;

        if (x is null || y is null)
            return false;

        if (x.SignatureHashCode != y.SignatureHashCode)
            return false;

        if (!string.Equals(x.Name, y.Name, StringComparison.OrdinalIgnoreCase))
            return false;

        var xParameters = x.Parameters;
        var yParameters = y.Parameters;
        if (xParameters.Length != yParameters.Length)
            return false;

        for (var i = 0; i < xParameters.Length; i++)
        {
            if (!ReferenceEquals(xParameters[i].Type, yParameters[i].Type))
                return false;
        }

        return true;
    }

    int IEqualityComparer<FunctionDefinition>.GetHashCode(FunctionDefinition obj)
    {
        return obj.SignatureHashCode;
    }
}
