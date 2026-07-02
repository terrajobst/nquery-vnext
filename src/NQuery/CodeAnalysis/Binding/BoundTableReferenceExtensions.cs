using System.Collections.Immutable;

using NQuery.CodeAnalysis.Symbols;

namespace NQuery.CodeAnalysis.Binding;

internal static class BoundTableReferenceExtensions
{
    extension(BoundTableReference node)
    {
        public ImmutableArray<TableInstanceSymbol> GetDeclaredTableInstances()
        {
            var result = new List<TableInstanceSymbol>();
            GetDeclaredTableInstances(result, node);
            return [.. result];
        }
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
            case BoundNodeKind.ApplyTableReference:
                var apply = (BoundApplyTableReference)node;
                GetDeclaredTableInstances(receiver, apply.Left);
                GetDeclaredTableInstances(receiver, apply.Right);
                break;
            case BoundNodeKind.ErrorTable:
                break;
            default:
                throw ExceptionBuilder.UnexpectedValue(node.Kind);
        }
    }
}
