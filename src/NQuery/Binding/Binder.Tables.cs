using System;
using System.Linq;

using NQuery.BoundNodes;
using NQuery.Symbols;

namespace NQuery.Binding
{
    partial class Binder
    {
        private Binder GetJoinConditionBinder(BoundTableReference left, BoundTableReference right)
        {
            var leftTables = left.GetDeclaredTableInstances();
            var rightTables = right.GetDeclaredTableInstances();
            var tables = leftTables.Concat(rightTables);
            return CreateLocalBinder(tables);
        }

        private BoundTableReference BindTableReference(TableReferenceSyntax node)
        {
            return Bind(node, BindTableReferenceInternal);
        }

        private BoundTableReference BindTableReferenceInternal(TableReferenceSyntax node)
        {
            switch (node.Kind)
            {
                case SyntaxKind.ParenthesizedTableReference:
                    return BindParenthesizedTableReference((ParenthesizedTableReferenceSyntax)node);

                case SyntaxKind.NamedTableReference:
                    return BindNamedTableReference((NamedTableReferenceSyntax)node);

                case SyntaxKind.CrossJoinedTableReference:
                    return BindCrossJoinedTableReference((CrossJoinedTableReferenceSyntax)node);

                case SyntaxKind.InnerJoinedTableReference:
                    return BindInnerJoinedTableReference((InnerJoinedTableReferenceSyntax)node);

                case SyntaxKind.OuterJoinedTableReference:
                    return BindOuterJoinedTableReference((OuterJoinedTableReferenceSyntax)node);

                case SyntaxKind.DerivedTableReference:
                    return BindDerivedTableReference((DerivedTableReferenceSyntax)node);

                default:
                    throw new ArgumentException(string.Format("Unknown node kind: {0}", node.Kind), "node");
            }
        }

        private BoundTableReference BindParenthesizedTableReference(ParenthesizedTableReferenceSyntax node)
        {
            return BindTableReference(node.TableReference);
        }

        private BoundTableReference BindNamedTableReference(NamedTableReferenceSyntax node)
        {
            var symbols = LookupTable(node.TableName).ToArray();

            if (symbols.Length == 0)
            {
                _diagnostics.ReportUndeclaredTable(node);

                var badTableSymbol = new BadTableSymbol(node.TableName.ValueText);
                var badAlias = node.Alias == null
                                   ? badTableSymbol.Name
                                   : node.Alias.Identifier.ValueText;
                var errorInstance = new TableInstanceSymbol(badAlias, badTableSymbol);
                return new BoundNamedTableReference(errorInstance);
            }

            if (symbols.Length > 1)
                _diagnostics.ReportAmbiguousTable(node.TableName, symbols);

            var table = symbols[0];
            var alias = node.Alias == null
                            ? table.Name
                            : node.Alias.Identifier.ValueText;

            var tableInstance = new TableInstanceSymbol(alias, table);
            return new BoundNamedTableReference(tableInstance);
        }

        private BoundTableReference BindCrossJoinedTableReference(CrossJoinedTableReferenceSyntax node)
        {
            var left = BindTableReference(node.Left);
            var right = BindTableReference(node.Right);
            return new BoundJoinedTableReference(BoundJoinType.InnerJoin, left, right, null);
        }

        private BoundTableReference BindInnerJoinedTableReference(InnerJoinedTableReferenceSyntax node)
        {
            var left = BindTableReference(node.Left);
            var right = BindTableReference(node.Right);

            var binder = GetJoinConditionBinder(left, right);
            var condition = binder.BindExpression(node.Condition);

            if (condition.Type.IsNonBoolean())
                _diagnostics.ReportOnClauseMustEvaluateToBool(node.Condition.Span);

            return new BoundJoinedTableReference(BoundJoinType.InnerJoin, left, right, condition);
        }

        private BoundTableReference BindOuterJoinedTableReference(OuterJoinedTableReferenceSyntax node)
        {
            var joinType = node.TypeKeyword.Kind == SyntaxKind.LeftKeyword
                               ? BoundJoinType.LeftOuterJoin
                               : node.TypeKeyword.Kind == SyntaxKind.RightKeyword
                                     ? BoundJoinType.RightOuterJoin
                                     : BoundJoinType.FullOuterJoin;

            var left = BindTableReference(node.Left);
            var right = BindTableReference(node.Right);

            var binder = GetJoinConditionBinder(left, right);
            var condition = binder.BindExpression(node.Condition);

            if (condition.Type.IsNonBoolean())
                _diagnostics.ReportOnClauseMustEvaluateToBool(node.Condition.Span);

            return new BoundJoinedTableReference(joinType, left, right, condition);
        }

        private BoundTableReference BindDerivedTableReference(DerivedTableReferenceSyntax node)
        {
            var query = BindQuery(node.Query);

            var columns = (from c in query.SelectColumns
                           where !string.IsNullOrEmpty(c.Name)
                           select new ColumnSymbol(c.Name, c.Expression.Type)).ToArray();

            var derivedTable = new DerivedTableSymbol(columns);
            var derivedTableInstance = new TableInstanceSymbol(node.Name.ValueText, derivedTable);
            var boundTableReference = new BoundDerivedTableReference(derivedTableInstance, query);

            return boundTableReference;
        }
    }
}