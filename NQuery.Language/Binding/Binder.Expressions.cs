using System;
using System.Linq;
using NQuery.Language.BoundNodes;
using NQuery.Language.Symbols;

namespace NQuery.Language.Binding
{
    internal sealed partial class Binder
    {
        private Type BindType(SyntaxToken typeName)
        {
            var type = LookupType(typeName);
            if (type != null)
                return type;

            _diagnostics.ReportUndeclaredType(typeName);
            return KnownTypes.Unknown;
        }

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

                case SyntaxKind.BitwiseAndExpression:
                case SyntaxKind.BitwiseOrExpression:
                case SyntaxKind.ExclusiveOrExpression:
                case SyntaxKind.AddExpression:
                case SyntaxKind.SubExpression:
                case SyntaxKind.MultiplyExpression:
                case SyntaxKind.DivideExpression:
                case SyntaxKind.ModuloExpression:
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
                    return BindCountAllExpression((CountAllExpressionSyntax)node);

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
            var operatorKind = node.Kind.ToUnaryOperatorKind();
            return BindUnaryExpression(node, operatorKind, node.Expression);
        }

        private BoundExpression BindUnaryExpression(SyntaxNode node, UnaryOperatorKind operatorKind, ExpressionSyntax expression)
        {
            var boundExpression = BindExpression(expression);
            return BindUnaryExpression(node, operatorKind, boundExpression);
        }

        private BoundExpression BindUnaryExpression(SyntaxNode node, UnaryOperatorKind operatorKind, BoundExpression expression)
        {
            // To avoid cascading errors, we'll return a unary expression that isn't bound to
            // an operator if the expression couldn't be resolved.

            if (expression.Type.IsError())
                return new BoundUnaryExpression(expression, OverloadResolutionResult<UnaryOperatorSignature>.None);

            var result = LookupUnaryOperator(operatorKind, expression);
            if (result.Best == null)
            {
                if (result.Selected == null)
                {
                    _diagnostics.ReportCannotApplyUnaryOperator(node.Span, operatorKind, expression.Type);
                }
                else
                {
                    _diagnostics.ReportAmbiguousUnaryOperator(node.Span, operatorKind, expression.Type);
                }
            }

            // TODO: We may want to insert conversion nodes right here.
            
            return new BoundUnaryExpression(expression, result);
        }

        private BoundExpression BindBinaryExpression(BinaryExpressionSyntax node)
        {
            var operatorKind = node.Kind.ToBinaryOperatorKind();
            return BindBinaryExpression(node, operatorKind, node.Left, node.Right);
        }

        private BoundExpression BindBinaryExpression(SyntaxNode node, BinaryOperatorKind operatorKind, ExpressionSyntax left, ExpressionSyntax right)
        {
            var boundLeft = BindExpression(left);
            var boundRight = BindExpression(right);
            return BindBinaryExpression(node, operatorKind, boundLeft, boundRight);
        }

        private BoundExpression BindBinaryExpression(SyntaxNode node, BinaryOperatorKind operatorKind, BoundExpression left, BoundExpression right)
        {
            // In order to avoid cascading errors, we'll return a binary expression without an operator
            // if either side couldn't be resolved.

            if (left.Type.IsError() || right.Type.IsError())
                return new BoundBinaryExpression(left, OverloadResolutionResult<BinaryOperatorSignature>.None, right);

            // TODO: We should consider supporting three-state-short-circuit evaluation.
            //
            // TODO: C# doesn't allow overloading && or ||. Instead, you need to overload & and | *and* you need to define operator true/operator false:
            //
            // The operation x && y is evaluated as T.false(x) ? x : T.&(x, y)
            // The operation x || y is evaluated as T.true(x) ? x : T.|(x, y)

            var result = LookupBinaryOperator(operatorKind, left, right);
            if (result.Best == null)
            {
                if (result.Selected == null)
                {
                    _diagnostics.ReportCannotApplyBinaryOperator(node.Span, operatorKind, left.Type, right.Type);
                }
                else
                {
                    _diagnostics.ReportAmbiguousBinaryOperator(node.Span, operatorKind, left.Type, right.Type);
                }
            }

            // TODO: We may want to insert conversion nodes right here.

            return new BoundBinaryExpression(left, result, right);
        }

        private BoundExpression BindConditionalExpression(SyntaxNode node, bool negated, BinaryOperatorKind operatorKind, ExpressionSyntax left, ExpressionSyntax right)
        {
            var result = BindBinaryExpression(node, operatorKind, left, right);
            return !negated
                       ? result
                       : BindUnaryExpression(node, UnaryOperatorKind.LogicalNot, result);
        }

        private BoundExpression BindLikeExpression(LikeExpressionSyntax node)
        {
            var negated = node.NotKeyword != null;
            return BindConditionalExpression(node, negated, BinaryOperatorKind.Like, node.Left, node.Right);
        }

        private BoundExpression BindSoundslikeExpression(SoundslikeExpressionSyntax node)
        {
            var negated = node.NotKeyword != null;
            return BindConditionalExpression(node, negated, BinaryOperatorKind.Soundslike, node.Left, node.Right);
        }

        private BoundExpression BindSimilarToExpression(SimilarToExpressionSyntax node)
        {
            var negated = node.NotKeyword != null;
            return BindConditionalExpression(node, negated, BinaryOperatorKind.SimilarTo, node.Left, node.Right);
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

            // TODO: Check errors
            var lowerCheckOperatorMethod = LookupBinaryOperator(BinaryOperatorKind.LessOrEqual, lowerBound, left);
            var upperCheckOperatorMethod = LookupBinaryOperator(BinaryOperatorKind.LessOrEqual, left, upperBound);
            var andOperatorMethod = LookupBinaryOperator(BinaryOperatorKind.LogicalAnd, typeof(bool), typeof(bool));

            var lowerExpression = new BoundBinaryExpression(lowerBound, lowerCheckOperatorMethod, left);
            var upperExpression = new BoundBinaryExpression(left, upperCheckOperatorMethod, upperBound);
            var andExpression = new BoundBinaryExpression(lowerExpression, andOperatorMethod, upperExpression);

            // TODO: Insert negation if node.NotKeyword != null
            return andExpression;
        }

        private BoundExpression BindIsNullExpression(IsNullExpressionSyntax node)
        {
            var boundExpression = BindExpression(node.Expression);
            return new BoundIsNullExpression(boundExpression);
        }

        private BoundExpression BindCastExpression(CastExpressionSyntax node)
        {
            var expression = BindExpression(node.Expression);
            var sourceType = expression.Type;
            var targetType = BindType(node.TypeName);
            var conversion = Conversion.Classify(sourceType, targetType);

            // To avoid cascading errors, we'll only validate the result
            // if we could resolve both, the expression as well as the
            // target type.

            if (!expression.Type.IsError() && !targetType.IsError())
            {
                if (!conversion.Exists)
                    _diagnostics.ReportCannotConvert(node, sourceType, targetType);
                else if (conversion.ConversionMethods.Count > 1)
                    _diagnostics.ReportAmbiguousConversion(node, sourceType, targetType);

                // TODO: We may want to insert conversion nodes right here.
            }

            return new BoundCastExpression(expression, targetType, conversion);
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
                                   let operatorMethod = LookupBinaryOperator(BinaryOperatorKind.Equal, boundInput, boundWhen)
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
            // TODO: We need to make sure that all argument types are identical or a conversion exists.
            var boundArguments = (from a in node.ArgumentList.Arguments
                                  select BindExpression(a)).ToArray();

            // TODO: Could we simply rewrite this syntax here?
            //
            // COALESCE(e1, e2, .. eN)
            //
            // ====>
            //
            // CASE
            //   WHEN e1 IS NOT NULL THEN e1
            //   ELSE
            //     CASE
            //       WHEN e2 IS NOT NULL THEN e2
            //       ELSE
            //         eN
            //     END
            // END

            return new BoundCoalesceExpression(boundArguments);
        }

        private BoundExpression BindNullIfExpression(NullIfExpressionSyntax node)
        {
            // TODO: We need to make sure that and right and left can be compared with each other.
            var boundLeft = BindExpression(node.LeftExpression);
            var boundRight = BindExpression(node.RightExpression);

            // TODO: Could we simply rewrite this syntax here?
            //
            // NULLIF(left, right)
            //
            // ===>
            //
            // CASE WHEN left != right THEN left END
            
            return new BoundNullIfExpression(boundLeft, boundRight);
        }

        private BoundExpression BindInExpression(InExpressionSyntax node)
        {
            // TODO: We need to make sure that every argument can be compared with expression.

            var boundExpression = BindExpression(node.Expression);
            var boundArguments = (from a in node.ArgumentList.Arguments
                                  select BindExpression(a)).ToArray();

            // TODO: We need to capture the operator being used -- it might not actully resolve to System.Boolean.

            // TODO: Could we simply rewrite this syntax here?
            //
            // expression IN (e1, e2..eN)
            //
            // ===>
            //
            // expression = e1 OR expression = e2 .. OR expression = eN

            return new BoundInExpression(boundExpression, boundArguments);
        }

        private static BoundExpression BindLiteralExpression(LiteralExpressionSyntax node)
        {
            return new BoundLiteralExpression(node.Value);
        }

        private BoundExpression BindVariableExpression(VariableExpressionSyntax node)
        {
            var symbols = LookupVariable(node.Name).ToArray();

            if (symbols.Length == 0)
            {
                _diagnostics.ReportUndeclaredVariable(node);

                var badVariableSymbol = new BadVariableSymbol(node.Name.ValueText);
                return new BoundVariableExpression(badVariableSymbol);
            }

            if (symbols.Length > 1)
                _diagnostics.ReportAmbiguousVariable(node.Name);

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

            var name = node.Name;
            var symbols = LookupColumnTableOrVariable(name).ToArray();

            if (symbols.Length == 0)
            {
                var isInvocable = LookupSymbols(name).OfType<FunctionSymbol>().Any() ||
                                  LookupSymbols(name).OfType<AggregateSymbol>().Any();
                if (isInvocable)
                    _diagnostics.ReportInvocationRequiresParenthesis(name);
                else
                    _diagnostics.ReportColumnTableOrVariableNotDeclared(name);
                var errorSymbol = new BadSymbol(name.ValueText);
                return new BoundNameExpression(errorSymbol, Enumerable.Empty<Symbol>());
            }

            if (symbols.Length > 1)
                _diagnostics.ReportAmbiguousName(name, symbols);

            var symbol = symbols[0];

            // If symbol refers to a table, we need to make that it's either not a derived table/CTE
            // or we are used in column access context (i.e. our parent is a a property access).
            //
            // For example, the following query is invalid
            //
            //    SELECT  D -- ERROR
            //    FROM    (
            //               SELECT  *
            //               FROM    Employees e
            //            ) AS D
            //
            // You cannot obtain a value for D itself.

            var tableInstance = symbol as TableInstanceSymbol;
            if (tableInstance != null)
            {
                var isColumnAccess = node.Parent is PropertyAccessExpressionSyntax;
                var refersToDerivedTableOrCte = tableInstance.Table.Kind == SymbolKind.DerivedTable ||
                                                tableInstance.Table.Kind == SymbolKind.CommonTableExpression;
                if (!isColumnAccess && refersToDerivedTableOrCte)
                    _diagnostics.ReportInvalidRowReference(name);
            }

            return new BoundNameExpression(symbol, symbols);
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

            var name = node.Name;
            if (target.Type.IsError())
            {
                // To avoid cascading errors, we'll give up early.
                var errorSymbol = new BadSymbol(name.ValueText);
                return new BoundNameExpression(errorSymbol, Enumerable.Empty<Symbol>());
            }
            
            var propertySymbols = LookupProperty(target.Type, name).ToArray();

            if (propertySymbols.Length == 0)
            {
                _diagnostics.ReportUndeclaredProperty(node, target.Type);
                var errorSymbol = new BadSymbol(name.ValueText);
                return new BoundNameExpression(errorSymbol, Enumerable.Empty<Symbol>());
            }

            if (propertySymbols.Length > 1)
                _diagnostics.ReportAmbiguousProperty(name);

            var propertySymbol = propertySymbols[0];
            return new BoundPropertyAccessExpression(target, propertySymbol);
        }

        private BoundExpression BindColumnInstance(PropertyAccessExpressionSyntax node, TableInstanceSymbol tableInstance)
        {
            var columnName = node.Name;
            var columnInstances = tableInstance.ColumnInstances.Where(c => columnName.Matches(c.Name)).ToArray();
            if (columnInstances.Length == 0)
            {
                _diagnostics.ReportUndeclaredColumn(node, tableInstance);
                var errorSymbol = new BadSymbol(columnName.ValueText);
                return new BoundNameExpression(errorSymbol, tableInstance.ColumnInstances);
            }

            if (columnInstances.Length > 1)
                _diagnostics.ReportAmbiguousColumnInstance(columnName, columnInstances);

            var columnInstance = columnInstances.First();
            return new BoundNameExpression(columnInstance, columnInstances);
        }

        private BoundExpression BindCountAllExpression(CountAllExpressionSyntax node)
        {
            var aggregates = LookupAggregate(node.Name).ToArray();
            if (aggregates.Length == 0)
            {
                _diagnostics.ReportUndeclaredAggregate(node.Name);

                var badSymbol = new BadSymbol(node.Name.ValueText);
                return new BoundNameExpression(badSymbol, Enumerable.Empty<Symbol>());
            }

            if (aggregates.Length > 1)
                _diagnostics.ReportAmbiguousAggregate(node.Name, aggregates);

            var aggregate = aggregates[0];
            var argument = new BoundLiteralExpression(1);
            return new BoundAggregateExpression(argument, aggregate);
        }

        private BoundExpression BindFunctionInvocationExpression(FunctionInvocationExpressionSyntax node)
        {
            if (node.ArgumentList.Arguments.Count == 1)
            {
                // Could be an aggregate or a function.

                var aggregates = LookupAggregate(node.Name).ToArray();
                var funcionCandidate = LookupFunctionWithSingleParameter(node.Name).FirstOrDefault();

                if (aggregates.Length > 0)
                {
                    if (funcionCandidate != null)
                    {
                        var symbols = new Symbol[] {aggregates[0], funcionCandidate};
                        _diagnostics.ReportAmbiguousName(node.Name, symbols);
                    }
                    else
                    {
                        if (aggregates.Length > 1)
                            _diagnostics.ReportAmbiguousAggregate(node.Name, aggregates);

                        var aggregate = aggregates[0];
                        var argument = BindExpression(node.ArgumentList.Arguments[0]);
                        return new BoundAggregateExpression(argument, aggregate);
                    }
                }
            }

            var name = node.Name;
            var arguments = node.ArgumentList.Arguments.Select(BindExpression).ToArray();
            var argumentTypes = arguments.Select(a => a.Type).ToArray();

            // To avoid cascading errors, we'll return a node that isn't bound to any function
            // if we couldn't resolve any of our arguments.

            var anyErrorsInArguments = argumentTypes.Any(a => a.IsError());
            if (anyErrorsInArguments)
                return new BoundFunctionInvocationExpression(arguments, OverloadResolutionResult<FunctionSymbolSignature>.None);

            var result = LookupFunction(name, argumentTypes);

            if (result.Best == null)
            {
                if (result.Selected == null)
                {
                    _diagnostics.ReportUndeclaredFunction(node, argumentTypes);
                    var errorSymbol = new BadSymbol(name.ValueText);
                    return new BoundNameExpression(errorSymbol, Enumerable.Empty<Symbol>());
                }
                
                var symbol1 = result.Selected.Signature.Symbol;
                var symbol2 = result.Candidates.First(c => c.IsSuitable && c.Signature.Symbol != symbol1).Signature.Symbol;
                _diagnostics.ReportAmbiguousInvocation(node.Span, symbol1, symbol2, argumentTypes);
            }

            // TODO: We need to convert all arguments

            return new BoundFunctionInvocationExpression(arguments, result);
        }

        private BoundExpression BindMethodInvocationExpression(MethodInvocationExpressionSyntax node)
        {
            var target = BindExpression(node.Target);
            var name = node.Name;
            var arguments = node.ArgumentList.Arguments.Select(BindExpression).ToArray();
            var argumentTypes = arguments.Select(a => a.Type).ToArray();

            // To avoid cascading errors, we'll return a node that insn't bound to
            // any method if we couldn't resolve our target or any of our arguments.

            var anyErrors = target.Type.IsError() || argumentTypes.Any(a => a.IsError());
            if (anyErrors)
                return new BoundMethodInvocationExpression(target, arguments, OverloadResolutionResult<MethodSymbolSignature>.None);

            var result = LookupMethod(target.Type, name, argumentTypes);

            if (result.Best == null)
            {
                if (result.Selected == null)
                {
                    _diagnostics.ReportUndeclaredMethod(node, target.Type, argumentTypes);
                    var errorSymbol = new BadSymbol(name.ValueText);
                    return new BoundNameExpression(errorSymbol, Enumerable.Empty<Symbol>());
                }
                
                var symbol1 = result.Selected.Signature.Symbol;
                var symbol2 = result.Candidates.First(c => c.IsSuitable && c.Signature.Symbol != symbol1).Signature.Symbol;
                _diagnostics.ReportAmbiguousInvocation(node.Span, symbol1, symbol2, argumentTypes);
            }

            // TODO: We need to convert all arguments

            return new BoundMethodInvocationExpression(target, arguments, result);
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
            var left = BindExpression(node.Left);
            var boundQuery = BindQuery(node.Query);

            if (boundQuery.SelectColumns.Count == 0)
            {
                // TODO: Error
            }
            else
            {
                if (boundQuery.SelectColumns.Count > 1)
                {
                    // TODO: Error
                }

                var right = boundQuery.SelectColumns[0].Expression;

                // To avoid cascading errors, we'll only validate the operator
                // if we could both sides.

                var argumentErrors = left.Type.IsError() || right.Type.IsError();
                if (!argumentErrors)
                {
                    var expressionKind = SyntaxFacts.GetBinaryOperatorExpression(node.OperatorToken.Kind);
                    var operatorKind = expressionKind.ToBinaryOperatorKind();

                    var result = LookupBinaryOperator(operatorKind, left.Type, right.Type);
                    if (result.Best == null)
                    {
                        if (result.Selected == null)
                            _diagnostics.ReportCannotApplyBinaryOperator(node.Span, operatorKind, left.Type, right.Type);
                        else
                            _diagnostics.ReportAmbiguousBinaryOperator(node.Span, operatorKind, left.Type, right.Type);
                    }
                }
            }

            // TODO: We may need to add a conversion for it
            return new BoundAllAnySubselect(left, boundQuery);
        }
    }
}