using System;
using System.Collections.Generic;
using System.Linq;
using NQuery.Language.BoundNodes;
using NQuery.Language.Symbols;

namespace NQuery.Language.Binding
{
    internal sealed partial class Binder
    {
        private BoundExpression BindExpression(ExpressionSyntax node)
        {
            return Bind(node, BindExpressionInternal);
        }

        private BoundExpression BindExpressionInternal(ExpressionSyntax node)
        {
            switch (node.Kind)
            {
                case SyntaxKind.ComplementExpression:
                case SyntaxKind.IdentityExpression:
                case SyntaxKind.NegationExpression:
                case SyntaxKind.LogicalNotExpression:
                    return BindUnaryExpression((UnaryExpressionSyntax) node);

                case SyntaxKind.BitAndExpression:
                case SyntaxKind.BitOrExpression:
                case SyntaxKind.BitXorExpression:
                case SyntaxKind.AddExpression:
                case SyntaxKind.SubExpression:
                case SyntaxKind.MultiplyExpression:
                case SyntaxKind.DivideExpression:
                case SyntaxKind.ModulusExpression:
                case SyntaxKind.PowerExpression:
                case SyntaxKind.EqualExpression:
                case SyntaxKind.NotEqualExpression:
                case SyntaxKind.LessExpression:
                case SyntaxKind.LessOrEqualExpression:
                case SyntaxKind.GreaterExpression:
                case SyntaxKind.GreaterOrEqualExpression:
                case SyntaxKind.NotLessExpression:
                case SyntaxKind.NotGreaterExpression:
                case SyntaxKind.LeftShiftExpression:
                case SyntaxKind.RightShiftExpression:
                case SyntaxKind.LogicalAndExpression:
                case SyntaxKind.LogicalOrExpression:
                    return BindBinaryExpression((BinaryExpressionSyntax) node);

                case SyntaxKind.LikeExpression:
                    return BindLikeExpression((LikeExpressionSyntax) node);

                case SyntaxKind.SoundslikeExpression:
                    return BindSoundslikeExpression((SoundslikeExpressionSyntax) node);

                case SyntaxKind.SimilarToExpression:
                    return BindSimilarToExpression((SimilarToExpressionSyntax) node);

                case SyntaxKind.ParenthesizedExpression:
                    return BindParenthesizedExpression((ParenthesizedExpressionSyntax) node);

                case SyntaxKind.BetweenExpression:
                    return BindBetweenExpression((BetweenExpressionSyntax) node);

                case SyntaxKind.IsNullExpression:
                    return BindIsNullExpression((IsNullExpressionSyntax) node);

                case SyntaxKind.CastExpression:
                    return BindCastExpression((CastExpressionSyntax) node);

                case SyntaxKind.CaseExpression:
                    return BindCaseExpression((CaseExpressionSyntax) node);

                case SyntaxKind.CoalesceExpression:
                    return BindCoalesceExpression((CoalesceExpressionSyntax) node);

                case SyntaxKind.NullIfExpression:
                    return BindNullIfExpression((NullIfExpressionSyntax) node);

                case SyntaxKind.InExpression:
                    return BindInExpression((InExpressionSyntax) node);

                case SyntaxKind.LiteralExpression:
                    return BindLiteralExpression((LiteralExpressionSyntax) node);

                case SyntaxKind.VariableExpression:
                    return BindVariableExpression((VariableExpressionSyntax) node);

                case SyntaxKind.NameExpression:
                    return BindNameExpression((NameExpressionSyntax) node);

                case SyntaxKind.PropertyAccessExpression:
                    return BindPropertyAccessExpression((PropertyAccessExpressionSyntax) node);

                case SyntaxKind.CountAllExpression:
                    return BindCountAllExpression((CountAllExpressionSyntax) node);

                case SyntaxKind.FunctionInvocationExpression:
                    return BindFunctionInvocationExpression((FunctionInvocationExpressionSyntax) node);

                case SyntaxKind.MethodInvocationExpression:
                    return BindMethodInvocationExpression((MethodInvocationExpressionSyntax) node);

                case SyntaxKind.SingleRowSubselect:
                    return BindSingleRowSubselect((SingleRowSubselectSyntax) node);

                case SyntaxKind.ExistsSubselect:
                    return BindExistsSubselect((ExistsSubselectSyntax) node);

                case SyntaxKind.AllAnySubselect:
                    return BindAllAnySubselect((AllAnySubselectSyntax) node);

                default:
                    throw new ArgumentException(string.Format("Unknown node kind: {0}", node.Kind), "node");
            }
        }

        private BoundExpression BindUnaryExpression(UnaryExpressionSyntax node)
        {
            var expression = BindExpression(node.Expression);
            var operatorKind = GetBoundUnaryOperator(node.Kind);
            var operatorMethod = BindUnaryOperator(expression.Type, operatorKind);
            return new BoundUnaryExpression(expression, operatorMethod);
        }

        private BoundExpression BindBinaryExpression(BinaryExpressionSyntax node)
        {
            var left = BindExpression(node.Left);
            var right = BindExpression(node.Right);
            var operatorKind = GetBoundBinaryOperator(node.Kind);
            var operatorMethod = BindBinaryOperator(left.Type, right.Type, operatorKind);
            return new BoundBinaryExpression(left, operatorMethod, right);
        }

        private BoundExpression BindLikeExpression(LikeExpressionSyntax node)
        {
            var left = BindExpression(node.Left);
            var right = BindExpression(node.Right);
            var operatorMethod = BindBinaryOperator(left.Type, right.Type, BoundBinaryOperatorKind.Like);
            return new BoundBinaryExpression(left, operatorMethod, right);
        }

        private BoundExpression BindSoundslikeExpression(SoundslikeExpressionSyntax node)
        {
            var left = BindExpression(node.Left);
            var right = BindExpression(node.Right);
            var operatorMethod = BindBinaryOperator(left.Type, right.Type, BoundBinaryOperatorKind.Soundex);
            return new BoundBinaryExpression(left, operatorMethod, right);
        }

        private BoundExpression BindSimilarToExpression(SimilarToExpressionSyntax node)
        {
            var left = BindExpression(node.Left);
            var right = BindExpression(node.Right);
            var operatorMethod = BindBinaryOperator(left.Type, right.Type, BoundBinaryOperatorKind.SimilarTo);
            return new BoundBinaryExpression(left, operatorMethod, right);
        }

        private BoundExpression BindParenthesizedExpression(ParenthesizedExpressionSyntax node)
        {
            return BindExpression(node.Expression);
        }

        private BoundExpression BindBetweenExpression(BetweenExpressionSyntax node)
        {
            // left BETWEEN lowerBound AND upperBound
            // 
            // ===>
            //
            // lowerBound <= Left AND left <= upperBound

            var left = BindExpression(node.Left);
            var lowerBound = BindExpression(node.LowerBound);
            var upperBound = BindExpression(node.UpperBound);

            var lowerCheckOperatorMethod = BindBinaryOperator(lowerBound.Type, left.Type, BoundBinaryOperatorKind.LessOrEqual);
            var upperCheckOperatorMethod = BindBinaryOperator(left.Type, upperBound.Type, BoundBinaryOperatorKind.LessOrEqual);
            var andOperatorMethod = BindBinaryOperator(typeof(bool), typeof(bool), BoundBinaryOperatorKind.LogicalAnd);

            var lowerExpression = new BoundBinaryExpression(lowerBound, lowerCheckOperatorMethod, left);
            var upperExpression = new BoundBinaryExpression(left, upperCheckOperatorMethod, upperBound);
            var andExpression = new BoundBinaryExpression(lowerExpression, andOperatorMethod, upperExpression);

            return andExpression;
        }

        private BoundExpression BindIsNullExpression(IsNullExpressionSyntax node)
        {
            throw new NotImplementedException();
        }

        private BoundExpression BindCastExpression(CastExpressionSyntax node)
        {
            throw new NotImplementedException();
        }

        private BoundExpression BindCaseExpression(CaseExpressionSyntax node)
        {
            return node.InputExpression == null
                       ? BindRegularCase(node)
                       : BindSearchedCase(node);
        }

        private BoundExpression BindRegularCase(CaseExpressionSyntax node)
        {
            // CASE
            //   WHEN e1 THEN r1
            //   WHEN e2 THEN r2
            //   ...
            //   ELSE re
            // END CASE

            // TODO: We need to ensure that there is a common type across r1..re.
            // TODO: We need to ensure that e1..eN are all of type bool

            var boundCaseLabels = (from caseLabel in node.CaseLabels
                                   let boundCondition = BindExpression(caseLabel.WhenExpression)
                                   let boundThen = BindExpression(caseLabel.ThenExpression)
                                   select new BoundCaseLabel(boundCondition, boundThen)).ToList();

            var boundElse = node.ElseExpression == null
                                ? null
                                : BindExpression(node.ElseExpression);

            return new BoundCaseExpression(boundCaseLabels, boundElse);
        }

        private BoundExpression BindSearchedCase(CaseExpressionSyntax node)
        {
            // CASE x
            //   WHEN e1 THEN r1
            //   WHEN e2 THEN r2
            //   ...
            //   ELSE re
            // END CASE
            //
            // ==>
            //
            // CASE
            //   WHEN x = e1 THEN r1
            //   WHEN x = e2 THEN r2
            //   ...
            //   ELSE re
            // END CASE

            // TODO: We need to ensure that there is a common type across r1...rn and re.
            // TODO: We need to ensure that all conditions x = eN have valid operators and evaluate to bool.

            var boundInput = BindExpression(node.InputExpression);

            var boundCaseLabels = (from caseLabel in node.CaseLabels
                                   let boundWhen = BindExpression(caseLabel.WhenExpression)
                                   let operatorMethod = BindBinaryOperator(boundInput.Type, boundWhen.Type, BoundBinaryOperatorKind.Equal)
                                   let boundCondition = new BoundBinaryExpression(boundInput, operatorMethod, boundWhen)
                                   let boundThen = BindExpression(caseLabel.ThenExpression)
                                   select new BoundCaseLabel(boundCondition, boundThen)).ToList();

            var boundElse = node.ElseExpression == null
                                ? null
                                : BindExpression(node.ElseExpression);

            return new BoundCaseExpression(boundCaseLabels, boundElse);
        }

        private BoundExpression BindCoalesceExpression(CoalesceExpressionSyntax node)
        {
            throw new NotImplementedException();
        }

        private BoundExpression BindNullIfExpression(NullIfExpressionSyntax node)
        {
            throw new NotImplementedException();
        }

        private BoundExpression BindInExpression(InExpressionSyntax node)
        {
            // expression IN (e1, e2..eN)
            //
            // ===>
            //
            // expression = e1 OR expression = e2 .. OR expression = eN

            throw new NotImplementedException();
        }

        private BoundExpression BindLiteralExpression(LiteralExpressionSyntax node)
        {
            return new BoundLiteralExpression(node.Value);
        }

        private BoundExpression BindVariableExpression(VariableExpressionSyntax node)
        {
            var symbols = LookupVariable(node.Name.ValueText).ToArray();

            if (symbols.Length == 0)
            {
                _diagnostics.ReportUndeclaredVariable(node);

                var badVariableSymbol = new BadVariableSymbol(node.Name.ValueText);
                return new BoundVariableExpression(badVariableSymbol);
            }

            if (symbols.Length > 1)
            {
                // TODO: Report ambiguous match
            }

            var variableSymbol = symbols[0];
            return new BoundVariableExpression(variableSymbol);
        }

        private BoundExpression BindNameExpression(NameExpressionSyntax node)
        {
            if (node.Name.IsMissing)
            {
                // If this token was inserted by the parser, there is no point in
                // trying to resolve this guy.
                var errorSymbol = new BadSymbol(string.Empty);
                return new BoundNameExpression(errorSymbol, Enumerable.Empty<Symbol>());
            }

            var name = node.Name.ValueText;
            var symbols = LookupName(name).ToArray();

            if (symbols.Length == 0)
            {
                _diagnostics.ReportUndeclaredEntity(node);
                var errorSymbol = new BadSymbol(name);
                return new BoundNameExpression(errorSymbol, Enumerable.Empty<Symbol>());
            }

            if (symbols.Length > 1)
            {
                // TODO: Report ambiguous match                   
            }

            // TODO: Check that symbol resolves to a table instance, column, or variable
            // TODO: If it's a function or aggregate report that parenthesis are required

            var first = symbols[0];
            return new BoundNameExpression(first, symbols);
        }

        private BoundExpression BindPropertyAccessExpression(PropertyAccessExpressionSyntax node)
        {
            var target = BindExpression(node.Target);

            // For cases like Foo.Bar we check whether 'Foo' was resolved to a table instance.
            // If that's the case we bind a column otherwise we bind a normal expression.

            var boundName = target as BoundNameExpression;
            if (boundName != null && boundName.Symbol.Kind == SymbolKind.TableInstance)
            {
                // In Foo.Bar, Foo was resolved to a table. Bind Bar as column.
                var tableInstance = (TableInstanceSymbol) boundName.Symbol;
                return BindColumnInstance(node, tableInstance);
            }

            // node.Target either wasn't a name expression or didn't resolve to a
            // table instance. Resolve node.Name as a property.

            var name = node.Name.ValueText;
            if (target.Type.IsUnknown())
            {
                var errorSymbol = new BadSymbol(name);
                return new BoundNameExpression(errorSymbol, Enumerable.Empty<Symbol>());
            }
            
            var propertySymbols = LookupProperty(target.Type, name).ToArray();

            if (propertySymbols.Length == 0)
            {
                _diagnostics.ReportUndeclaredProperty(node, target.Type);
                var errorSymbol = new BadSymbol(name);
                return new BoundNameExpression(errorSymbol, Enumerable.Empty<Symbol>());
            }

            if (propertySymbols.Length > 1)
            {
                // TODO: Report ambigious match
            }

            var propertySymbol = propertySymbols[0];
            return new BoundPropertyAccessExpression(target, propertySymbol);
        }

        private BoundExpression BindColumnInstance(PropertyAccessExpressionSyntax node, TableInstanceSymbol tableInstance)
        {
            var columnName = node.Name.ValueText;
            var columnInstances = tableInstance.ColumnInstances.Where(c => c.Name.Equals(columnName, StringComparison.OrdinalIgnoreCase)).ToArray();
            if (columnInstances.Length == 0)
            {
                _diagnostics.ReportUndeclaredColumn(node, tableInstance);
                var errorSymbol = new BadSymbol(columnName);
                return new BoundNameExpression(errorSymbol, tableInstance.ColumnInstances);
            }
            
            if (columnInstances.Length > 1)
            {
                // TODO: Return ambiguous match
            }

            var columnInstance = columnInstances.First();
            return new BoundNameExpression(columnInstance, columnInstances);
        }

        private BoundExpression BindCountAllExpression(CountAllExpressionSyntax node)
        {
            throw new NotImplementedException();
        }

        private BoundExpression BindFunctionInvocationExpression(FunctionInvocationExpressionSyntax node)
        {
            // TODO: Resolve and bind to aggregates

            var name = node.Name.ValueText;
            var arguments = (from a in node.ArgumentList.Arguments
                             select BindExpression(a.Expression)).ToList();

            var functionSymbols = LookupFunction(name, arguments.Count).ToArray();

            if (functionSymbols.Length == 0)
            {
                var argumentTypes = from a in arguments
                                    select a.Type;
                _diagnostics.ReportUndeclaredFunction(node, argumentTypes);
                var errorSymbol = new BadSymbol(name);
                return new BoundNameExpression(errorSymbol, Enumerable.Empty<Symbol>());
            }

            if (functionSymbols.Length > 1)
            {
                // TODO: Overload resolution
                // TODO: Report ambigious match
            }

            var functionSymbol = functionSymbols[0];
            return new BoundFunctionInvocationExpression(arguments, functionSymbol);
        }

        private BoundExpression BindMethodInvocationExpression(MethodInvocationExpressionSyntax node)
        {
            var target = BindExpression(node.Target);

            var name = node.Name.ValueText;
            if (target.Type.IsUnknown())
            {
                var errorSymbol = new BadSymbol(name);
                return new BoundNameExpression(errorSymbol, Enumerable.Empty<Symbol>());
            }

            var arguments = (from a in node.ArgumentList.Arguments
                             select BindExpression(a.Expression)).ToList();

            var methodSymbols = LookupMethod(target.Type, name, arguments.Count).ToArray();

            if (methodSymbols.Length == 0)
            {
                var argumentTypes = from a in arguments
                                    select a.Type;
                _diagnostics.ReportUndeclaredMethod(node, target.Type, argumentTypes);
                var errorSymbol = new BadSymbol(name);
                return new BoundNameExpression(errorSymbol, Enumerable.Empty<Symbol>());
            }

            if (methodSymbols.Length > 1)
            {
                // TODO: Overload resolution
                // TODO: Report ambigious match
            }

            var methodSymbol = methodSymbols[0];
            return new BoundMethodInvocationExpression(target, arguments, methodSymbol);
        }

        private BoundExpression BindSingleRowSubselect(SingleRowSubselectSyntax node)
        {
            var boundQuery = BindQuery(node.Query);
      
            // TODO: Ensure query has exactly one column

            return new BoundSingleRowSubselect(boundQuery);
        }

        private BoundExpression BindExistsSubselect(ExistsSubselectSyntax node)
        {
            var boundQuery = BindQuery(node.Query);

            // NOTE: Number of columns doesn't matter here

            return new BoundExistsSubselect(boundQuery);
        }

        private BoundExpression BindAllAnySubselect(AllAnySubselectSyntax node)
        {
            var boundQuery = BindQuery(node.Query);

            // TODO: Ensure query has exactly one column

            return new BoundAllAnySubselect(boundQuery);
        }
    }
}