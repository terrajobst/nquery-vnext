using System.Collections.Immutable;

using NQuery.Symbols;

namespace NQuery.Binding;

internal static class BoundTableReferenceExtensions
{
    public static ImmutableArray<TableInstanceSymbol> GetDeclaredTableInstances(this BoundTableReference node)
    {
        var result = new List<TableInstanceSymbol>();
        GetDeclaredTableInstances(result, node);
        return result.ToImmutableArray();
    }

    private static void GetDeclaredTableInstances(List<TableInstanceSymbol> receiver, BoundTableReference node)
    {
        switch (node.Kind)
        {
            case BoundNodeKind.NamedTableReference:
                receiver.Add(((BoundNamedTableReference)node).TableInstance);
                break;
            case BoundNodeKind.DerivedTableReference:
                receiver.Add(((BoundDerivedTableReference)node).TableInstance);
                break;
            case BoundNodeKind.JoinTableReference:
                var join = (BoundJoinTableReference)node;
                GetDeclaredTableInstances(receiver, join.Left);
                GetDeclaredTableInstances(receiver, join.Right);
                break;
            default:
                throw ExceptionBuilder.UnexpectedValue(node.Kind);
        }
    }
}
