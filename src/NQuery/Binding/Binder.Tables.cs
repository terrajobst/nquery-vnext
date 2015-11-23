using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using NQuery.Symbols;
using NQuery.Syntax;

namespace NQuery.Binding
{
    partial class Binder
    {
        private BoundRelation BindTableReference(TableReferenceSyntax node)
        {
            return Bind(node, BindTableReferenceInternal);
        }

        private BoundRelation BindTableReferenceInternal(TableReferenceSyntax node)
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
                    throw ExceptionBuilder.UnexpectedValue(node.Kind);
            }
        }

        private BoundRelation BindParenthesizedTableReference(ParenthesizedTableReferenceSyntax node)
        {
            return BindTableReference(node.TableReference);
        }

        private BoundRelation BindNamedTableReference(NamedTableReferenceSyntax node)
        {
            var symbols = LookupTable(node.TableName).ToImmutableArray();

            if (symbols.Length == 0)
            {
                Diagnostics.ReportUndeclaredTable(node);

                var errorTable = new ErrorTableSymbol(node.TableName.ValueText);
                var errorAlias = node.Alias == null
                                   ? errorTable.Name
                                   : node.Alias.Identifier.ValueText;
                var errorInstance = new TableInstanceSymbol(errorAlias, errorTable, ValueSlotFactory);
                return new BoundTableRelation(errorInstance);
            }

            if (symbols.Length > 1)
                Diagnostics.ReportAmbiguousTable(node.TableName, symbols);

            var table = symbols[0];
            var aliasIdentifier = node.Alias != null
                ? node.Alias.Identifier
                : node.TableName;

            var alias = aliasIdentifier.ValueText;

            var tableInstance = new TableInstanceSymbol(alias, table, ValueSlotFactory);

            QueryState.IntroducedTables.Add(tableInstance, aliasIdentifier);

            return new BoundTableRelation(tableInstance);
        }

        private BoundRelation BindCrossJoinedTableReference(CrossJoinedTableReferenceSyntax node)
        {
            var left = BindTableReference(node.Left);
            var right = BindTableReference(node.Right);
            return new BoundJoinRelation(BoundJoinType.Inner, left, right, null, null, null);
        }

        private BoundRelation BindInnerJoinedTableReference(InnerJoinedTableReferenceSyntax node)
        {
            var left = BindTableReference(node.Left);
            var right = BindTableReference(node.Right);

            var binder = CreateJoinConditionBinder(left, right);
            var condition = binder.BindExpression(node.Condition);

            if (condition.Type.IsNonBoolean())
                Diagnostics.ReportOnClauseMustEvaluateToBool(node.Condition.Span);

            return new BoundJoinRelation(BoundJoinType.Inner, left, right, condition, null, null);
        }

        private BoundRelation BindOuterJoinedTableReference(OuterJoinedTableReferenceSyntax node)
        {
            var joinType = node.TypeKeyword.Kind == SyntaxKind.LeftKeyword
                               ? BoundJoinType.LeftOuter
                               : node.TypeKeyword.Kind == SyntaxKind.RightKeyword
                                     ? BoundJoinType.RightOuter
                                     : BoundJoinType.FullOuter;

            var left = BindTableReference(node.Left);
            var right = BindTableReference(node.Right);

            var binder = CreateJoinConditionBinder(left, right);
            var condition = binder.BindExpression(node.Condition);

            if (condition.Type.IsNonBoolean())
                Diagnostics.ReportOnClauseMustEvaluateToBool(node.Condition.Span);

            return new BoundJoinRelation(joinType, left, right, condition, null, null);
        }

        private BoundRelation BindDerivedTableReference(DerivedTableReferenceSyntax node)
        {
            // TODO: Ensure query has no ORDER BY unless TOP is also specified

            var query = BindQuery(node.Query);

            var namedQueryColumns = query.OutputColumns.Where(c => !string.IsNullOrEmpty(c.Name));
            var columns = new List<ColumnSymbol>();
            var valueSlotFromColumn = new Dictionary<ColumnSymbol, ValueSlot>();

            foreach (var queryColumn in namedQueryColumns)
            {
                var columnSymbol = new ColumnSymbol(queryColumn.Name, queryColumn.Type);
                columns.Add(columnSymbol);
                valueSlotFromColumn.Add(columnSymbol, queryColumn.ValueSlot);
            }

            var derivedTable = new DerivedTableSymbol(columns);
            var valueSlotFactory = new Func<TableInstanceSymbol, ColumnSymbol, ValueSlot>((ti, c) => valueSlotFromColumn[c]);
            var derivedTableInstance = new TableInstanceSymbol(node.Name.ValueText, derivedTable, valueSlotFactory);
            var boundTableReference = new BoundDerivedTableRelation(derivedTableInstance, query.Relation);
            return boundTableReference;
        }
    }
}