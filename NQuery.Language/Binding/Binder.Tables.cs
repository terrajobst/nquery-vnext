using System;
using System.Linq;
using NQuery.Language.BoundNodes;
using NQuery.Language.Symbols;

namespace NQuery.Language.Binding
{
    internal sealed partial class Binder
    {
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
            var symbols = LookupTable(node.TableName.ValueText).ToArray();

            if (symbols.Length == 0)
            {
                _diagnostics.Add(DiagnosticFactory.UndeclaredTable(node));

                var badTableSymbol = new BadTableSymbol(node.TableName.ValueText);
                var badAlias = node.Alias == null
                                   ? badTableSymbol.Name
                                   : node.Alias.Identifier.ValueText;
                var errorInstance = new TableInstanceSymbol(badAlias, badTableSymbol);
                return new BoundNamedTableReference(errorInstance);
            }

            if (symbols.Length > 1)
            {
                // TODO: Report ambiguous match
            }

            var table = symbols[0];
            var alias = node.Alias == null
                            ? table.Name
                            : node.Alias.Identifier.ValueText;

            var tableInstance = new TableInstanceSymbol(alias, table);
            var boundNamedTableReference = new BoundNamedTableReference(tableInstance);

            var parent = _bindingContextStack.Pop();
            var bindingContext = new TableBindingContext(parent, boundNamedTableReference);
            _bindingContextStack.Push(bindingContext);

            return boundNamedTableReference;
        }

        private BoundTableReference BindCrossJoinedTableReference(CrossJoinedTableReferenceSyntax node)
        {
            var left = BindTableReference(node.Left);
            var right = BindTableReference(node.Right);

            var parent = _bindingContextStack.Pop();
            var bindingContext = new JoinConditionBindingContext(parent, left, right);
            _bindingContextStack.Push(bindingContext);

            return new BoundJoinedTableReference(BoundJoinType.InnerJoin, left, right, null);
        }

        private BoundTableReference BindInnerJoinedTableReference(InnerJoinedTableReferenceSyntax node)
        {
            var parent = Context;

            var left = BindTableReference(node.Left);
            var right = BindTableReference(node.Right);
            _bindingContextStack.Pop();

            var bindingContext = new JoinConditionBindingContext(parent, left, right);
            _bindingContextStack.Push(bindingContext);

            var expressionBindingContext = new ExpressionBindingContext(bindingContext);
            _bindingContextStack.Push(expressionBindingContext);

            var condition = BindExpression(node.Condition);

            _bindingContextStack.Pop();

            // TODO: Ensure condition evaluates to boolean

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

            var parent = _bindingContextStack.Pop();
            var bindingContext = new JoinConditionBindingContext(parent, left, right);
            _bindingContextStack.Push(bindingContext);

            var condition = BindExpression(node.Condition);

            // TODO: Ensure condition evaluates to boolean

            return new BoundJoinedTableReference(joinType, left, right, condition);
        }

        private BoundTableReference BindDerivedTableReference(DerivedTableReferenceSyntax node)
        {
            // TODO: We need to make sure the nested query doesn't get access to our variables.
            //
            // More precisely, the following query is expected to cause a resolution error
            // for "t.RegionId" in the WHERE clause. However, right now it succeeds and binds
            // agains the "outer row".
            //
            //    SELECT e.LastName + ', ' + e.FirstName Employee,
            //		     r.RegionDescription Regions,
            //		     t.TerritoryDescription Territories
            //
            //    FROM	 Employees e
            //			    INNER JOIN EmployeeTerritories et ON et.EmployeeID = e.EmployeeID
            //			    INNER JOIN Territories t ON t.TerritoryID = et.TerritoryID
            //			    CROSS JOIN (SELECT * FROM Region x WHERE x.RegionID = t.RegionID) AS r
            //
            // One way to achieve this is by introducing a new "outermost" binding context. This is
            // either the SchemaBindingContext (in case of non-CTE tables) or the CteBindingContext
            // which contains the CTEs and has the SchemaBindingContext as its parent.
            //
            // When resolving a CTE, the current context is set to this outermost binding context.

            var previousBindingContext = _bindingContextStack.Pop();
            _bindingContextStack.Push(_rootBindingContext);

            var query = BindQuery(node.Query);

            _bindingContextStack.Pop();
            _bindingContextStack.Push(previousBindingContext);

            var columns = (from c in query.SelectColumns
                           select new ColumnSymbol(c.Name, c.Expression.Type)).ToArray();

            var derivedTable = new DerivedTableSymbol(columns);
            var derivedTableInstance = new TableInstanceSymbol(node.Name.ValueText, derivedTable);
            var boundTableReference = new BoundDerivedTableReference(derivedTableInstance, query);

            var parent = _bindingContextStack.Pop();
            var bindingContext = new TableBindingContext(parent, boundTableReference);
            _bindingContextStack.Push(bindingContext);

            return boundTableReference;
        }
    }
}