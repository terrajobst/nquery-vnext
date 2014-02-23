using System;
using System.Collections.Generic;

using NQuery.Symbols;

namespace NQuery.Binding
{
    internal static class BoundTableReferenceExtensions
    {
        public static IReadOnlyCollection<TableInstanceSymbol> GetDeclaredTableInstances(this BoundTableReference node)
        {
            var result = new List<TableInstanceSymbol>();
            GetDeclaredTableInstances(result, node);
            return result;
        }

        private static void GetDeclaredTableInstances(List<TableInstanceSymbol> receiver, BoundTableReference node)
        {
            switch (node.Kind)
            {
                case BoundNodeKind.NamedTableReference:
                    GetDeclaredTableInstances(receiver, (BoundNamedTableReference)node);
                    break;
                case BoundNodeKind.DerivedTableReference:
                    GetDeclaredTableInstances(receiver, (BoundDerivedTableReference)node);
                    break;
                case BoundNodeKind.JoinedTableReference:
                    GetDeclaredTableInstances(receiver, (BoundJoinedTableReference)node);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static void GetDeclaredTableInstances(List<TableInstanceSymbol> receiver, BoundNamedTableReference node)
        {
            var symbol = node.TableInstance;
            receiver.Add(symbol);
        }

        private static void GetDeclaredTableInstances(List<TableInstanceSymbol> receiver, BoundJoinedTableReference node)
        {
            GetDeclaredTableInstances(receiver, node.Left);
            GetDeclaredTableInstances(receiver, node.Right);
        }

        private static void GetDeclaredTableInstances(List<TableInstanceSymbol> receiver, BoundDerivedTableReference node)
        {
            var symbol = node.TableInstance;
            receiver.Add(symbol);
        }
    }
}