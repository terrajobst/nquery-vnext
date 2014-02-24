using System;
using System.Collections.Generic;

using NQuery.Symbols;

namespace NQuery.Binding
{
    internal static class BoundTableReferenceExtensions
    {
        public static IReadOnlyCollection<TableInstanceSymbol> GetDeclaredTableInstances(this BoundRelation node)
        {
            var result = new List<TableInstanceSymbol>();
            GetDeclaredTableInstances(result, node);
            return result;
        }

        private static void GetDeclaredTableInstances(List<TableInstanceSymbol> receiver, BoundRelation node)
        {
            switch (node.Kind)
            {
                case BoundNodeKind.TableRelation:
                    GetDeclaredTableInstances(receiver, (BoundTableRelation)node);
                    break;
                case BoundNodeKind.DerivedTableRelation:
                    GetDeclaredTableInstances(receiver, (BoundDerivedTableRelation)node);
                    break;
                case BoundNodeKind.JoinRelation:
                    GetDeclaredTableInstances(receiver, (BoundJoinRelation)node);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static void GetDeclaredTableInstances(List<TableInstanceSymbol> receiver, BoundTableRelation node)
        {
            var symbol = node.TableInstance;
            receiver.Add(symbol);
        }

        private static void GetDeclaredTableInstances(List<TableInstanceSymbol> receiver, BoundJoinRelation node)
        {
            GetDeclaredTableInstances(receiver, node.Left);
            GetDeclaredTableInstances(receiver, node.Right);
        }

        private static void GetDeclaredTableInstances(List<TableInstanceSymbol> receiver, BoundDerivedTableRelation node)
        {
            var symbol = node.TableInstance;
            receiver.Add(symbol);
        }
    }
}