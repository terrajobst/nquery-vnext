using System;
using System.Collections.Generic;
using System.Linq;

using NQuery.Symbols;
using NQuery.Syntax;

namespace NQuery.Binding
{
    partial class Binder
    {
        private void EnsureCaseLabelsEvaluateToBool(IList<CaseLabelSyntax> caseLabels, IList<BoundCaseLabel> boundCaseLabels)
        {
            for (var i = 0; i < caseLabels.Count; i++)
            {
                var type = boundCaseLabels[i].Condition.Type;
                if (!type.IsError() && type != typeof(bool))
                    Diagnostics.ReportWhenMustEvaluateToBool(caseLabels[i].WhenExpression.Span);
            }
        }

        private Type BindType(SyntaxToken typeName)
        {
            var type = LookupType(typeName);
            if (type != null)
                return type;

            Diagnostics.ReportUndeclaredType(typeName);
            return TypeFacts.Unknown;
        }

        private static BoundExpression BindArgument<T>(BoundExpression expression, OverloadResolutionResult<T> result, int argumentIndex) where T : Signature
        {
            var selected = result.Selected;
            if (selected == null)
                return expression;

            var targetType = selected.Signature.GetParameterType(argumentIndex);
            var conversion = selected.ArgumentConversions[argumentIndex];
            return conversion.IsIdentity
                       ? expression
                       : new BoundConversionExpression(expression, targetType, conversion);
        }

        private IList<BoundExpression> BindToCommonType(IList<ExpressionSyntax> expressions)
        {
            var boundExpressions = expressions.Select(BindExpression).ToArray();

            // To avoid cascading errors as let's first see whether we couldn't resolve
            // any of the expressions. If that's the case, we'll simply return them as-is.

            var hasAnyErrors = boundExpressions.Any(e => e.Type.IsError());
            if (hasAnyErrors)
                return boundExpressions;

            // Now let's see whether all expression already have the same type.
            // In that case we can simply return the input as-is.

            var firstType = boundExpressions.Select(e => e.Type).First();
            var allAreSameType = boundExpressions.All(e => e.Type == firstType);
            if (allAreSameType)
                return boundExpressions;

            // Not all expressions have the same type. Let's try to find a common type.
            //
            // The common type C among a type set T1..TN is defined as follows:
            //
            // (1) C has to be in the set T1..TN. In other words we don't consider
            //     types not already present.
            //
            // (2) All types T1..T2 have to have an identity conversion or an implicit
            //     conversion to C.
            //
            // (3) C has to be the only type for which (1) and (2) hold.

            Type commonType = null;

            foreach (var target in boundExpressions)
            {
                if (target.Type.IsError() || target.Type == commonType)
                    continue;

                var allOthersCanConvertToTarget = true;

                foreach (var source in boundExpressions)
                {
                    if (source.Type.IsError() || source.Type == target.Type)
                        continue;

                    var conversion = Conversion.Classify(source.Type, target.Type);
                    if (!conversion.IsImplicit)
                    {
                        allOthersCanConvertToTarget = false;
                        break;
                    }
                }

                if (allOthersCanConvertToTarget)
                {
                    if (commonType == null)
                    {
                        commonType = target.Type;
                    }
                    else
                    {
                        // TODO: We may want to report an ambiguity error here.
                        commonType = null;
                        break;
                    }
                }
            }

            if (commonType == null)
            {
                // If we couldn't find a common type, we'll just use the first expression's
                // type -- this will cause BindConversion below to to report errors.
                commonType = boundExpressions.First().Type;
            }

            return boundExpressions.Select((e, i) => BindConversion(expressions[i].Span, e, commonType)).ToArray();
        }

        private BoundExpression BindConversion(TextSpan errorSpan, BoundExpression expression, Type targetType)
        {
            var sourceType = expression.Type;
            var conversion = Conversion.Classify(sourceType, targetType);
            if (conversion.IsIdentity)
                return expression;

            // To avoid cascading errors, we'll only validate the result
            // if we could resolve both, the expression as well as the
            // target type.

            if (!sourceType.IsError() && !targetType.IsError())
            {
                if (!conversion.Exists)
                    Diagnostics.ReportCannotConvert(errorSpan, sourceType, targetType);
                else if (conversion.ConversionMethods.Count > 1)
                    Diagnostics.ReportAmbiguousConversion(errorSpan, sourceType, targetType);
            }

            return new BoundConversionExpression(expression, targetType, conversion);
        }

        private BoundExpression BindExpression(ExpressionSyntax node)
        {
            var result = Bind(node, BindExpressionInternal);

            // If we've already allocated a value slot for the given expression,
            // we want our caller to refer to this value slot.

            ValueSlot valueSlot;
            if (TryReplaceExpression(node, result, out valueSlot))
                return new BoundValueSlotExpression(valueSlot);

            return result;
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

                case SyntaxKind.InQueryExpression:
                    return BindInQueryExpression((InQueryExpressionSyntax)node);

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
            return BindUnaryExpression(node.Span, operatorKind, node.Expression);
        }

        private BoundExpression BindUnaryExpression(TextSpan errorSpan, UnaryOperatorKind operatorKind, ExpressionSyntax expression)
        {
            var boundExpression = BindExpression(expression);
            return BindUnaryExpression(errorSpan, operatorKind, boundExpression);
        }

        private BoundExpression BindUnaryExpression(TextSpan errorSpan, UnaryOperatorKind operatorKind, BoundExpression expression)
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
                    Diagnostics.ReportCannotApplyUnaryOperator(errorSpan, operatorKind, expression.Type);
                }
                else
                {
                    Diagnostics.ReportAmbiguousUnaryOperator(errorSpan, operatorKind, expression.Type);
                }
            }

            // Convert argument (if necessary)

            var convertedArgument = BindArgument(expression, result, 0);

            return new BoundUnaryExpression(convertedArgument, result);
        }

        private BoundExpression BindOptionalNegation(TextSpan errorSpan, SyntaxToken notKeyword, BoundExpression expression)
        {
            return notKeyword == null
                       ? expression
                       : BindUnaryExpression(errorSpan, UnaryOperatorKind.LogicalNot, expression);
        }

        private BoundExpression BindBinaryExpression(BinaryExpressionSyntax node)
        {
            var operatorKind = node.Kind.ToBinaryOperatorKind();
            return BindBinaryExpression(node.Span, operatorKind, node.Left, node.Right);
        }

        private BoundExpression BindBinaryExpression(TextSpan errorSpan, BinaryOperatorKind operatorKind, ExpressionSyntax left, ExpressionSyntax right)
        {
            var boundLeft = BindExpression(left);
            var boundRight = BindExpression(right);
            return BindBinaryExpression(errorSpan, operatorKind, boundLeft, boundRight);
        }

        private BoundExpression BindBinaryExpression(TextSpan errorSpan, BinaryOperatorKind operatorKind, BoundExpression left, BoundExpression right)
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
                    Diagnostics.ReportCannotApplyBinaryOperator(errorSpan, operatorKind, left.Type, right.Type);
                }
                else
                {
                    Diagnostics.ReportAmbiguousBinaryOperator(errorSpan, operatorKind, left.Type, right.Type);
                }
            }

            // Convert arguments (if necessary)

            var convertedLeft = BindArgument(left, result, 0);
            var convertedRight = BindArgument(right, result, 1);

            return new BoundBinaryExpression(convertedLeft, result, convertedRight);
        }

        private BoundExpression BindBinaryExpression(TextSpan errorSpan, SyntaxToken notKeyword, BinaryOperatorKind operatorKind, ExpressionSyntax left, ExpressionSyntax right)
        {
            var expression = BindBinaryExpression(errorSpan, operatorKind, left, right);
            return BindOptionalNegation(errorSpan, notKeyword, expression);
        }

        private BoundExpression BindLikeExpression(LikeExpressionSyntax node)
        {
            return BindBinaryExpression(node.Span, node.NotKeyword, BinaryOperatorKind.Like, node.Left, node.Right);
        }

        private BoundExpression BindSoundslikeExpression(SoundslikeExpressionSyntax node)
        {
            return BindBinaryExpression(node.Span, node.NotKeyword, BinaryOperatorKind.Soundslike, node.Left, node.Right);
        }

        private BoundExpression BindSimilarToExpression(SimilarToExpressionSyntax node)
        {
            return BindBinaryExpression(node.Span, node.NotKeyword, BinaryOperatorKind.SimilarTo, node.Left, node.Right);
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
            // left >= lowerBound AND left <= upperBound

            var left = BindExpression(node.Left);
            var lowerBound = BindExpression(node.LowerBound);
            var upperBound = BindExpression(node.UpperBound);

            var lowerCheck = BindBinaryExpression(node.Span, BinaryOperatorKind.GreaterOrEqual, left, lowerBound);
            var upperCheck = BindBinaryExpression(node.Span, BinaryOperatorKind.LessOrEqual, left, upperBound);
            var boundsCheck = BindBinaryExpression(node.Span, BinaryOperatorKind.LogicalAnd, lowerCheck, upperCheck);

            return BindOptionalNegation(node.Span, node.NotKeyword, boundsCheck);
        }

        private BoundExpression BindIsNullExpression(IsNullExpressionSyntax node)
        {
            var expression = BindExpression(node.Expression);
            var isNull = new BoundIsNullExpression(expression);
            return BindOptionalNegation(node.Span, node.NotKeyword, isNull);
        }

        private BoundExpression BindCastExpression(CastExpressionSyntax node)
        {
            var expression = BindExpression(node.Expression);
            var targetType = BindType(node.TypeName);
            return BindConversion(node.Span, expression, targetType);
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

            var boundResults = BindCaseResultExpressions(node);
            var boundCaseLabels = node.CaseLabels.Select((l, i) => new BoundCaseLabel(BindExpression(l.WhenExpression), boundResults[i])).ToArray();
            var boundElse = node.ElseLabel == null ? null : boundResults.Last();

            EnsureCaseLabelsEvaluateToBool(node.CaseLabels, boundCaseLabels);

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

            var boundInput = BindExpression(node.InputExpression);
            var boundResults = BindCaseResultExpressions(node);

            var boundCaseLabels = (from t in node.CaseLabels.Select((c, i) => Tuple.Create(c, i))
                                   let caseLabel = t.Item1
                                   let i = t.Item2
                                   let boundWhen = BindExpression(caseLabel.WhenExpression)
                                   let boundCondition = BindBinaryExpression(caseLabel.WhenExpression.Span, BinaryOperatorKind.Equal, boundInput, boundWhen)
                                   let boundThen = boundResults[i]
                                   select new BoundCaseLabel(boundCondition, boundThen)).ToArray();

            var boundElse = node.ElseLabel == null
                                ? null
                                : boundResults.Last();

            EnsureCaseLabelsEvaluateToBool(node.CaseLabels, boundCaseLabels);

            return new BoundCaseExpression(boundCaseLabels, boundElse);
        }

        private IList<BoundExpression> BindCaseResultExpressions(CaseExpressionSyntax node)
        {
            var elseExpression = node.ElseLabel == null
                                     ? Enumerable.Empty<ExpressionSyntax>()
                                     : new[] {node.ElseLabel.Expression};
            var expressions = node.CaseLabels.Select(l => l.ThenExpression).Concat(elseExpression).ToArray();
            return BindToCommonType(expressions);
        }

        private BoundExpression BindCoalesceExpression(CoalesceExpressionSyntax node)
        {
            // COALESCE(e1, e2, .. eN)
            //
            // ====>
            //
            // CASE
            //   WHEN e1 IS NOT NULL THEN e1
            //   ..
            //   WHEN e2 IS NOT NULL THEN e2
            //   ELSE
            //     eN
            // END

            var boundArguments = BindToCommonType(node.ArgumentList.Arguments);
            var caseLabelCount = node.ArgumentList.Arguments.Count - 1;
            var caseLabels = boundArguments.Take(caseLabelCount).Select(a => new BoundCaseLabel(new BoundIsNullExpression(a), a)).ToArray();
            var elseExpresion = boundArguments.Last();
            return new BoundCaseExpression(caseLabels, elseExpresion);
        }

        private BoundExpression BindNullIfExpression(NullIfExpressionSyntax node)
        {
            // NULLIF(left, right)
            //
            // ===>
            //
            // CASE WHEN left != right THEN left END

            var expressions = BindToCommonType(new[] {node.LeftExpression, node.RightExpression});
            var boundLeft = expressions[0];
            var boundRight = expressions[1];
            var boundComparison = BindBinaryExpression(node.Span, BinaryOperatorKind.NotEqual, boundLeft, boundRight);
            var boundCaseLabel = new BoundCaseLabel(boundComparison, boundLeft);
            return new BoundCaseExpression(new[] { boundCaseLabel }, null);
        }

        private BoundExpression BindInExpression(InExpressionSyntax node)
        {
            // e IN (e1, e2..eN)
            //
            // ===>
            //
            // e = e1 OR e = e2 .. OR e = eN

            var boundExpression = BindExpression(node.Expression);
            var boundComparisons = from a in node.ArgumentList.Arguments
                                   let boundArgument = BindExpression(a)
                                   let boundComparision = BindBinaryExpression(a.Span, BinaryOperatorKind.Equal, boundExpression, boundArgument)
                                   select boundComparision;

            return boundComparisons.Aggregate<BoundExpression, BoundExpression>(null, (c, b) => c == null ? b : BindBinaryExpression(node.Span, BinaryOperatorKind.LogicalOr, c, b));
        }

        private BoundExpression BindInQueryExpression(InQueryExpressionSyntax node)
        {
            // left IN (SELECT right FROM ...)
            //
            // ==>
            //
            // left = ANY (SELECT right FROM ...)

            return BindAllAnySubselect(node.Span, node.Expression, node.Query, BinaryOperatorKind.Equal);
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
                Diagnostics.ReportUndeclaredVariable(node);

                var badVariableSymbol = new BadVariableSymbol(node.Name.ValueText);
                return new BoundVariableExpression(badVariableSymbol);
            }

            if (symbols.Length > 1)
                Diagnostics.ReportAmbiguousVariable(node.Name);

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
                return new BoundNameExpression(errorSymbol);
            }

            var name = node.Name;
            var symbols = LookupColumnTableOrVariable(name).ToArray();

            if (symbols.Length == 0)
            {
                var isInvocable = LookupSymbols<FunctionSymbol>(name).Any() ||
                                  LookupSymbols<AggregateSymbol>(name).Any();
                if (isInvocable)
                    Diagnostics.ReportInvocationRequiresParenthesis(name);
                else
                    Diagnostics.ReportColumnTableOrVariableNotDeclared(name);
                var errorSymbol = new BadSymbol(name.ValueText);
                return new BoundNameExpression(errorSymbol);
            }

            if (symbols.Length > 1)
                Diagnostics.ReportAmbiguousName(name, symbols);

            var symbol = symbols[0];

            // If symbol refers to a table, we need to make sure that it's either not a derived table/CTE
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
                var hasNoType = tableInstance.Table.Type.IsMissing();
                if (!isColumnAccess && hasNoType)
                    Diagnostics.ReportInvalidRowReference(name);
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
                return new BoundNameExpression(errorSymbol);
            }

            var propertySymbols = LookupProperty(target.Type, name).ToArray();

            if (propertySymbols.Length == 0)
            {
                var hasMethods = LookupMethod(target.Type, name).Any();
                if (hasMethods)
                    Diagnostics.ReportInvocationRequiresParenthesis(name);
                else
                    Diagnostics.ReportUndeclaredProperty(node, target.Type);
                var errorSymbol = new BadSymbol(name.ValueText);
                return new BoundNameExpression(errorSymbol);
            }

            if (propertySymbols.Length > 1)
                Diagnostics.ReportAmbiguousProperty(name);

            var propertySymbol = propertySymbols[0];
            return new BoundPropertyAccessExpression(target, propertySymbol);
        }

        private BoundExpression BindColumnInstance(PropertyAccessExpressionSyntax node, TableInstanceSymbol tableInstance)
        {
            var columnName = node.Name;
            var columnInstances = tableInstance.ColumnInstances.Where(c => columnName.Matches(c.Name)).ToArray();
            if (columnInstances.Length == 0)
            {
                // TODO: Check whether we can resolve to a method and if so give error ReportInvocationRequiresParenthesis
                Diagnostics.ReportUndeclaredColumn(node, tableInstance);
                var errorSymbol = new BadSymbol(columnName.ValueText);
                return new BoundNameExpression(errorSymbol, tableInstance.ColumnInstances);
            }

            if (columnInstances.Length > 1)
                Diagnostics.ReportAmbiguousColumnInstance(columnName, columnInstances);

            var columnInstance = columnInstances.First();
            return new BoundNameExpression(columnInstance, columnInstances);
        }

        private BoundExpression BindCountAllExpression(CountAllExpressionSyntax node)
        {
            var aggregates = LookupAggregate(node.Name).ToArray();
            if (aggregates.Length == 0)
            {
                Diagnostics.ReportUndeclaredAggregate(node.Name);

                var badSymbol = new BadSymbol(node.Name.ValueText);
                return new BoundNameExpression(badSymbol);
            }

            if (aggregates.Length > 1)
                Diagnostics.ReportAmbiguousAggregate(node.Name, aggregates);

            var aggregate = aggregates[0];
            var argument = new BoundLiteralExpression(0);
            var boundAggregate = new BoundAggregateExpression(aggregate, argument);
            return BindAggregate(node, boundAggregate);
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
                        Diagnostics.ReportAmbiguousName(node.Name, symbols);
                    }
                    else
                    {
                        if (aggregates.Length > 1)
                            Diagnostics.ReportAmbiguousAggregate(node.Name, aggregates);

                        var aggregate = aggregates[0];
                        return BindAggregateInvocationExpression(node, aggregate);
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
                    Diagnostics.ReportUndeclaredFunction(node, argumentTypes);
                    var errorSymbol = new BadSymbol(name.ValueText);
                    return new BoundNameExpression(errorSymbol);
                }

                var symbol1 = result.Selected.Signature.Symbol;
                var symbol2 = result.Candidates.First(c => c.IsSuitable && c.Signature.Symbol != symbol1).Signature.Symbol;
                Diagnostics.ReportAmbiguousInvocation(node.Span, symbol1, symbol2, argumentTypes);
            }

            // Convert all arguments (if necessary)

            var convertedArguments = arguments.Select((a, i) => BindArgument(a, result, i)).ToArray();

            return new BoundFunctionInvocationExpression(convertedArguments, result);
        }

        private BoundExpression BindAggregateInvocationExpression(FunctionInvocationExpressionSyntax node, AggregateSymbol aggregate)
        {
            var argument = node.ArgumentList.Arguments[0];
            var argumentBinder = CreateAggregateArgumentBinder();
            var boundArgument = argumentBinder.BindExpression(argument);

            var boundAggregate = new BoundAggregateExpression(aggregate, boundArgument);
            return BindAggregate(node, boundAggregate);
        }

        private BoundExpression BindAggregate(ExpressionSyntax aggregate, BoundAggregateExpression boundAggregate)
        {
            var affectedQueryScopes = aggregate.DescendantNodes()
                                               .Select(GetBoundNode<BoundNameExpression>)
                                               .Where(n => n != null)
                                               .Select(b => b.Symbol)
                                               .OfType<TableColumnInstanceSymbol>()
                                               .Select(c => FindQueryState(c.TableInstance))
                                               .Distinct()
                                               .Take(2)
                                               .ToArray();

            if (affectedQueryScopes.Length > 1)
                Diagnostics.Report(aggregate.Span, DiagnosticId.AggregateContainsColumnsFromDifferentQueries);

            var queryState = affectedQueryScopes.DefaultIfEmpty(QueryState)
                                                .First();

            if (queryState == null)
            {
                Diagnostics.Report(aggregate.Span, DiagnosticId.AggregateInvalidInCurrentContext);
            }
            else
            {
                var existingSlot = FindComputedValue(aggregate, queryState.ComputedAggregates);
                if (existingSlot == null)
                {
                    var slot = ValueSlotFactory.CreateTemporaryValueSlot(boundAggregate.Type);
                    queryState.ComputedAggregates.Add(new ComputedValue(aggregate, boundAggregate, slot));
                }
            }

            var aggregateBelongsToCurrentQuery = QueryState == queryState;

            if (InOnClause && aggregateBelongsToCurrentQuery)
            {
                Diagnostics.ReportAggregateInOn(aggregate.Span);
            }
            else if (InWhereClause && aggregateBelongsToCurrentQuery)
            {
                Diagnostics.ReportAggregateInWhere(aggregate.Span);
            }
            else if (InGroupByClause && aggregateBelongsToCurrentQuery)
            {
                Diagnostics.ReportAggregateInGroupBy(aggregate.Span);
            }
            else if (InAggregateArgument)
            {
                Diagnostics.ReportAggregateInAggregateArgument(aggregate.Span);
            }

            return boundAggregate;
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
                    Diagnostics.ReportUndeclaredMethod(node, target.Type, argumentTypes);
                    var errorSymbol = new BadSymbol(name.ValueText);
                    return new BoundNameExpression(errorSymbol);
                }

                var symbol1 = result.Selected.Signature.Symbol;
                var symbol2 = result.Candidates.First(c => c.IsSuitable && c.Signature.Symbol != symbol1).Signature.Symbol;
                Diagnostics.ReportAmbiguousInvocation(node.Span, symbol1, symbol2, argumentTypes);
            }

            // Convert all arguments (if necessary)

            var convertedArguments = arguments.Select((a, i) => BindArgument(a, result, i)).ToArray();

            return new BoundMethodInvocationExpression(target, convertedArguments, result);
        }

        private BoundExpression BindSingleRowSubselect(SingleRowSubselectSyntax node)
        {
            // TODO: Ensure query has no ORDER BY unless TOP is also specified

            var boundQuery = BindSubquery(node.Query);

            if (boundQuery.OutputColumns.Count > 1)
            {
                // TODO: Error
            }

            return new BoundSingleRowSubselect(boundQuery);
        }

        private BoundExpression BindExistsSubselect(ExistsSubselectSyntax node)
        {
            // TODO: Ensure query has no ORDER BY unless TOP is also specified

            var boundQuery = BindSubquery(node.Query);

            // NOTE: Number of columns doesn't matter here

            return new BoundExistsSubselect(boundQuery);
        }

        private BoundExpression BindAllAnySubselect(AllAnySubselectSyntax node)
        {
            var expressionKind = SyntaxFacts.GetBinaryOperatorExpression(node.OperatorToken.Kind);
            var operatorKind = expressionKind.ToBinaryOperatorKind();
            return BindAllAnySubselect(node.Span, node.Left, node.Query, operatorKind);
        }

        private BoundExpression BindAllAnySubselect(TextSpan span, ExpressionSyntax leftNode, QuerySyntax queryNode, BinaryOperatorKind operatorKind)
        {
            // TODO: Ensure query has no ORDER BY unless TOP is also specified

            var left = BindExpression(leftNode);
            var boundQuery = BindSubquery(queryNode);

            if (boundQuery.OutputColumns.Count > 1)
            {
                // TODO: Error
            }

            var rightColumn = boundQuery.OutputColumns[0];
            var right = new BoundNameExpression(rightColumn);

            // To avoid cascading errors, we'll only validate the operator
            // if we could resolve both sides.

            if (left.Type.IsError() || right.Type.IsError())
                return new BoundAllAnySubselect(left, boundQuery, OverloadResolutionResult<BinaryOperatorSignature>.None);

            var result = LookupBinaryOperator(operatorKind, left.Type, right.Type);
            if (result.Best == null)
            {
                if (result.Selected == null)
                    Diagnostics.ReportCannotApplyBinaryOperator(span, operatorKind, left.Type, right.Type);
                else
                    Diagnostics.ReportAmbiguousBinaryOperator(span, operatorKind, left.Type, right.Type);
            }

            var convertedLeft = BindArgument(left, result, 0);
            var convertedRight = BindArgument(right, result, 1);

            if (convertedRight != right)
            {
                // TODO: We need a BoundNode that can convert the output of a query.
                throw new NotImplementedException("Converting right side of ALL/ANY/SOME not implemented yet.");
            }

            return new BoundAllAnySubselect(convertedLeft, boundQuery, result);
        }
    }
}