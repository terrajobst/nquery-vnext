using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using NQuery.Symbols;
using NQuery.Symbols.Aggregation;
using NQuery.Syntax;
using NQuery.Text;

namespace NQuery.Binding
{
    partial class Binder
    {
        private void EnsureCaseLabelsEvaluateToBool(ImmutableArray<CaseLabelSyntax> caseLabels, ImmutableArray<BoundCaseLabel> boundCaseLabels)
        {
            for (var i = 0; i < caseLabels.Length; i++)
            {
                var type = boundCaseLabels[i].Condition.Type;
                if (!type.IsError() && type != typeof(bool))
                    Diagnostics.ReportWhenMustEvaluateToBool(caseLabels[i].WhenExpression.Span);
            }
        }

        private static Type FindCommonType(ImmutableArray<BoundExpression> boundExpressions)
        {
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

            return commonType;
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

            // TODO: We need check for ambiguous conversions here as well.

            return conversion.IsIdentity
                       ? expression
                       : new BoundConversionExpression(expression, targetType, conversion);
        }

        private ImmutableArray<BoundExpression> BindToCommonType(IReadOnlyList<ExpressionSyntax> expressions)
        {
            var boundExpressions = expressions.Select(BindExpression).ToImmutableArray();
            return BindToCommonType(boundExpressions, i => expressions[i].Span);
        }

        private ImmutableArray<BoundExpression> BindToCommonType(ImmutableArray<BoundExpression> boundExpressions, TextSpan dianosticSpan)
        {
            return BindToCommonType(boundExpressions, i => dianosticSpan);
        }

        private ImmutableArray<BoundExpression> BindToCommonType(ImmutableArray<BoundExpression> boundExpressions, Func<int, TextSpan> dianosticSpanProvider)
        {
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

            var commonType = FindCommonType(boundExpressions);

            // If we couldn't find a common type, we'll just use the first expression's
            // type -- this will cause BindConversion below to to report errors.

            if (commonType == null)
                commonType = boundExpressions.First().Type;

            return boundExpressions.Select((e, i) => BindConversion(dianosticSpanProvider(i), e, commonType)).ToImmutableArray();
        }

        private void BindToCommonType(TextSpan diagnosticSpan, ValueSlot left, ValueSlot right, out BoundExpression newLeft, out BoundExpression newRight)
        {
            newLeft = null;
            newRight = null;

            if (left.Type == right.Type || left.Type.IsError() || right.Type.IsError())
                return;

            var conversionLeftToRight = Conversion.Classify(left.Type, right.Type);
            var conversionRightToLeft = Conversion.Classify(right.Type, left.Type);

            if (conversionLeftToRight.IsImplicit && conversionRightToLeft.IsImplicit)
            {
                // TODO: We may want to report an ambiguity error here.
            }

            if (conversionLeftToRight.IsImplicit)
            {
                newLeft = BindConversion(diagnosticSpan, new BoundValueSlotExpression(left), right.Type);
            }
            else
            {
                newRight = BindConversion(diagnosticSpan, new BoundValueSlotExpression(right), left.Type);
            }
        }

        private BoundExpression BindConversion(TextSpan diagnosticSpan, BoundExpression expression, Type targetType)
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
                    Diagnostics.ReportCannotConvert(diagnosticSpan, sourceType, targetType);
                else if (conversion.ConversionMethods.Length > 1)
                    Diagnostics.ReportAmbiguousConversion(diagnosticSpan, sourceType, targetType);
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

                case SyntaxKind.SoundsLikeExpression:
                    return BindSoundsLikeExpression((SoundsLikeExpressionSyntax) node);

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
                    throw ExceptionBuilder.UnexpectedValue(node.Kind);
            }
        }

        private BoundExpression BindUnaryExpression(UnaryExpressionSyntax node)
        {
            var operatorKind = node.Kind.ToUnaryOperatorKind();
            return BindUnaryExpression(node.Span, operatorKind, node.Expression);
        }

        private BoundExpression BindUnaryExpression(TextSpan diagnosticSpan, UnaryOperatorKind operatorKind, ExpressionSyntax expression)
        {
            var boundExpression = BindExpression(expression);
            return BindUnaryExpression(diagnosticSpan, operatorKind, boundExpression);
        }

        private BoundExpression BindUnaryExpression(TextSpan diagnosticSpan, UnaryOperatorKind operatorKind, BoundExpression expression)
        {
            // To avoid cascading errors, we'll return a unary expression that isn't bound to
            // an operator if the expression couldn't be resolved.

            if (expression.Type.IsError())
                return new BoundUnaryExpression(operatorKind, OverloadResolutionResult<UnaryOperatorSignature>.None, expression);

            var result = LookupUnaryOperator(operatorKind, expression);
            if (result.Best == null)
            {
                if (result.Selected == null)
                {
                    Diagnostics.ReportCannotApplyUnaryOperator(diagnosticSpan, operatorKind, expression.Type);
                }
                else
                {
                    Diagnostics.ReportAmbiguousUnaryOperator(diagnosticSpan, operatorKind, expression.Type);
                }
            }

            // Convert argument (if necessary)

            var convertedArgument = BindArgument(expression, result, 0);

            return new BoundUnaryExpression(operatorKind, result, convertedArgument);
        }

        private BoundExpression BindOptionalNegation(TextSpan diagnosticSpan, SyntaxToken notKeyword, BoundExpression expression)
        {
            return notKeyword == null
                       ? expression
                       : BindUnaryExpression(diagnosticSpan, UnaryOperatorKind.LogicalNot, expression);
        }

        private BoundExpression BindBinaryExpression(BinaryExpressionSyntax node)
        {
            var operatorKind = node.Kind.ToBinaryOperatorKind();
            return BindBinaryExpression(node.Span, operatorKind, node.Left, node.Right);
        }

        private BoundExpression BindBinaryExpression(TextSpan diagnosticSpan, BinaryOperatorKind operatorKind, ExpressionSyntax left, ExpressionSyntax right)
        {
            var boundLeft = BindExpression(left);
            var boundRight = BindExpression(right);
            return BindBinaryExpression(diagnosticSpan, operatorKind, boundLeft, boundRight);
        }

        private BoundExpression BindBinaryExpression(TextSpan diagnosticSpan, BinaryOperatorKind operatorKind, BoundExpression left, BoundExpression right)
        {
            // In order to avoid cascading errors, we'll return a binary expression without an operator
            // if either side couldn't be resolved.

            if (left.Type.IsError() || right.Type.IsError())
                return new BoundBinaryExpression(left, operatorKind, OverloadResolutionResult<BinaryOperatorSignature>.None, right);

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
                    Diagnostics.ReportCannotApplyBinaryOperator(diagnosticSpan, operatorKind, left.Type, right.Type);
                }
                else
                {
                    Diagnostics.ReportAmbiguousBinaryOperator(diagnosticSpan, operatorKind, left.Type, right.Type);
                }
            }

            // Convert arguments (if necessary)

            var convertedLeft = BindArgument(left, result, 0);
            var convertedRight = BindArgument(right, result, 1);

            return new BoundBinaryExpression(convertedLeft, operatorKind, result, convertedRight);
        }

        private BoundExpression BindBinaryExpression(TextSpan diagnosticSpan, SyntaxToken notKeyword, BinaryOperatorKind operatorKind, ExpressionSyntax left, ExpressionSyntax right)
        {
            var expression = BindBinaryExpression(diagnosticSpan, operatorKind, left, right);
            return BindOptionalNegation(diagnosticSpan, notKeyword, expression);
        }

        private BoundExpression BindLikeExpression(LikeExpressionSyntax node)
        {
            return BindBinaryExpression(node.Span, node.NotKeyword, BinaryOperatorKind.Like, node.Left, node.Right);
        }

        private BoundExpression BindSoundsLikeExpression(SoundsLikeExpressionSyntax node)
        {
            return BindBinaryExpression(node.Span, node.NotKeyword, BinaryOperatorKind.SoundsLike, node.Left, node.Right);
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
            var boundCaseLabels = node.CaseLabels.Select((l, i) => new BoundCaseLabel(BindExpression(l.WhenExpression), boundResults[i])).ToImmutableArray();
            var boundElse = node.ElseLabel == null
                                ? null
                                : boundResults.Last();

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
                                   select new BoundCaseLabel(boundCondition, boundThen)).ToImmutableArray();

            var boundElse = node.ElseLabel == null
                                ? null
                                : boundResults.Last();

            EnsureCaseLabelsEvaluateToBool(node.CaseLabels, boundCaseLabels);

            return new BoundCaseExpression(boundCaseLabels, boundElse);
        }

        private ImmutableArray<BoundExpression> BindCaseResultExpressions(CaseExpressionSyntax node)
        {
            var elseExpression = node.ElseLabel == null
                                     ? Enumerable.Empty<ExpressionSyntax>()
                                     : new[] {node.ElseLabel.Expression};
            var expressions = node.CaseLabels.Select(l => l.ThenExpression).Concat(elseExpression).ToImmutableArray();
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
            var caseLabels = new BoundCaseLabel[caseLabelCount];
            for (var i = 0; i < caseLabelCount; i++)
            {
                var argument = node.ArgumentList.Arguments[i];
                var boundArgument = boundArguments[i];
                var boundIsNullExpression = new BoundIsNullExpression(boundArgument);
                var boundIsNullNegation = BindUnaryExpression(argument.Span, UnaryOperatorKind.LogicalNot, boundIsNullExpression);
                var caseLabel = new BoundCaseLabel(boundIsNullNegation, boundArgument);
                caseLabels[i] = caseLabel;
            }

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

            var allAnySubselect = BindAllAnySubselect(node.Span, node.Expression, false, node.Query, BinaryOperatorKind.Equal);
            return BindOptionalNegation(node.Span, node.NotKeyword, allAnySubselect);
        }

        private static BoundExpression BindLiteralExpression(LiteralExpressionSyntax node)
        {
            return new BoundLiteralExpression(node.Value);
        }

        private BoundExpression BindVariableExpression(VariableExpressionSyntax node)
        {
            var symbols = LookupVariable(node.Name).ToImmutableArray();

            if (symbols.Length == 0)
            {
                Diagnostics.ReportUndeclaredVariable(node);

                return new BoundErrorExpression();
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
                return new BoundErrorExpression();
            }

            var name = node.Name;
            var symbols = LookupColumnTableOrVariable(name).ToImmutableArray();

            if (symbols.Length == 0)
            {
                var isInvocable = LookupSymbols<FunctionSymbol>(name).Any() ||
                                  LookupSymbols<AggregateSymbol>(name).Any();
                if (isInvocable)
                    Diagnostics.ReportInvocationRequiresParenthesis(name);
                else
                    Diagnostics.ReportColumnTableOrVariableNotDeclared(name);

                return new BoundErrorExpression();
            }

            if (symbols.Length > 1)
                Diagnostics.ReportAmbiguousName(name, symbols);

            var symbol = symbols.First();

            switch (symbol.Kind)
            {
                case SymbolKind.TableColumnInstance:
                    return new BoundColumnExpression((ColumnInstanceSymbol)symbol);
                case SymbolKind.Variable:
                    return new BoundVariableExpression((VariableSymbol) symbol);
                case SymbolKind.TableInstance:
                {
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
                        // TODO: Fully support row access
                        //var isColumnAccess = node.Parent is PropertyAccessExpressionSyntax;
                        //var hasNoType = tableInstance.Table.Type.IsMissing();
                        //if (!isColumnAccess && hasNoType)
                        //    Diagnostics.ReportInvalidRowReference(name);

                        var isColumnAccess = node.Parent is PropertyAccessExpressionSyntax;
                        if (!isColumnAccess)
                        {
                            Diagnostics.ReportInvalidRowReference(name);
                            return new BoundErrorExpression();
                        }
                    }

                    return new BoundTableExpression(tableInstance);
                }
                default:
                    throw ExceptionBuilder.UnexpectedValue(symbol.Kind);
            }
        }

        private BoundExpression BindPropertyAccessExpression(PropertyAccessExpressionSyntax node)
        {
            var target = BindExpression(node.Target);

            // For cases like Foo.Bar we check whether 'Foo' was resolved to a table instance.
            // If that's the case we bind a column otherwise we bind a normal expression.

            var boundTable = target as BoundTableExpression;
            if (boundTable != null)
            {
                // In Foo.Bar, Foo was resolved to a table. Bind Bar as column.
                var tableInstance = boundTable.Symbol;
                return BindColumnInstance(node, tableInstance);
            }

            // node.Target either wasn't a name expression or didn't resolve to a
            // table instance. Resolve node.Name as a property.

            var name = node.Name;
            if (target.Type.IsError())
            {
                // To avoid cascading errors, we'll give up early.
                return new BoundErrorExpression();
            }

            var propertySymbols = LookupProperty(target.Type, name).ToImmutableArray();

            if (propertySymbols.Length == 0)
            {
                var hasMethods = LookupMethod(target.Type, name).Any();
                if (hasMethods)
                    Diagnostics.ReportInvocationRequiresParenthesis(name);
                else
                    Diagnostics.ReportUndeclaredProperty(node, target.Type);

                return new BoundErrorExpression();
            }

            if (propertySymbols.Length > 1)
                Diagnostics.ReportAmbiguousProperty(name);

            var propertySymbol = propertySymbols[0];
            return new BoundPropertyAccessExpression(target, propertySymbol);
        }

        private BoundExpression BindColumnInstance(PropertyAccessExpressionSyntax node, TableInstanceSymbol tableInstance)
        {
            var columnName = node.Name;
            var columnInstances = tableInstance.ColumnInstances.Where(c => columnName.Matches(c.Name)).ToImmutableArray();
            if (columnInstances.Length == 0)
            {
                Diagnostics.ReportUndeclaredColumn(node, tableInstance);
                return new BoundErrorExpression();
            }

            if (columnInstances.Length > 1)
                Diagnostics.ReportAmbiguousColumnInstance(columnName, columnInstances);

            var columnInstance = columnInstances.First();
            return new BoundColumnExpression(columnInstance);
        }

        private BoundExpression BindCountAllExpression(CountAllExpressionSyntax node)
        {
            var aggregates = LookupAggregate(node.Name).ToImmutableArray();
            if (aggregates.Length == 0)
            {
                Diagnostics.ReportUndeclaredAggregate(node.Name);
                return new BoundErrorExpression();
            }

            if (aggregates.Length > 1)
                Diagnostics.ReportAmbiguousAggregate(node.Name, aggregates);

            var aggregate = aggregates[0];
            var boundArgument = new BoundLiteralExpression(0);
            var boundAggregatable = BindAggregatable(node.Span, aggregate, boundArgument);
            var boundAggregate = new BoundAggregateExpression(aggregate, boundAggregatable, boundArgument);
            return BindAggregate(node, boundAggregate);
        }

        private BoundExpression BindFunctionInvocationExpression(FunctionInvocationExpressionSyntax node)
        {
            if (node.ArgumentList.Arguments.Count == 1)
            {
                // Could be an aggregate or a function.

                var aggregates = LookupAggregate(node.Name).ToImmutableArray();
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
            var arguments = node.ArgumentList.Arguments.Select(BindExpression).ToImmutableArray();
            var argumentTypes = arguments.Select(a => a.Type).ToImmutableArray();

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
                    return new BoundErrorExpression();
                }

                var symbol1 = result.Selected.Signature.Symbol;
                var symbol2 = result.Candidates.First(c => c.IsSuitable && c.Signature.Symbol != symbol1).Signature.Symbol;
                Diagnostics.ReportAmbiguousInvocation(node.Span, symbol1, symbol2, argumentTypes);
            }

            // Convert all arguments (if necessary)

            var convertedArguments = arguments.Select((a, i) => BindArgument(a, result, i)).ToImmutableArray();

            return new BoundFunctionInvocationExpression(convertedArguments, result);
        }

        private BoundExpression BindAggregateInvocationExpression(FunctionInvocationExpressionSyntax node, AggregateSymbol aggregate)
        {
            var argument = node.ArgumentList.Arguments[0];
            var argumentBinder = CreateAggregateArgumentBinder();
            var boundArgument = argumentBinder.BindExpression(argument);
            var boundAggregatable = BindAggregatable(node.Span, aggregate, boundArgument);
            var boundAggregate = new BoundAggregateExpression(aggregate, boundAggregatable, boundArgument);
            return BindAggregate(node, boundAggregate);
        }

        private IAggregatable BindAggregatable(TextSpan errorSpan, AggregateSymbol aggregate, BoundExpression boundArgument)
        {
            var aggregatable = boundArgument.Type.IsError()
                ? null
                : aggregate.Definition.CreateAggregatable(boundArgument.Type);

            if (!boundArgument.Type.IsError() && aggregatable == null)
                Diagnostics.ReportAggregateDoesNotSupportType(errorSpan, aggregate, boundArgument.Type);

            return aggregatable;
        }

        private BoundExpression BindAggregate(ExpressionSyntax aggregate, BoundAggregateExpression boundAggregate)
        {
            var affectedQueryScopes = aggregate.DescendantNodes()
                                               .Select(GetBoundNode<BoundColumnExpression>)
                                               .Where(n => n != null)
                                               .Select(b => b.Symbol)
                                               .OfType<TableColumnInstanceSymbol>()
                                               .Select(c => FindQueryState(c.TableInstance))
                                               .Distinct()
                                               .Take(2)
                                               .ToImmutableArray();

            if (affectedQueryScopes.Length > 1)
                Diagnostics.ReportAggregateContainsColumnsFromDifferentQueries(aggregate.Span);

            var queryState = affectedQueryScopes.DefaultIfEmpty(QueryState)
                                                .First();

            if (queryState == null)
            {
                Diagnostics.ReportAggregateInvalidInCurrentContext(aggregate.Span);
            }
            else
            {
                var existingSlot = FindComputedValue(aggregate, queryState.ComputedAggregates);
                if (existingSlot == null)
                {
                    var slot = ValueSlotFactory.CreateTemporary(boundAggregate.Type);
                    queryState.ComputedAggregates.Add(new BoundComputedValueWithSyntax(aggregate, boundAggregate, slot));
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
            var arguments = node.ArgumentList.Arguments.Select(BindExpression).ToImmutableArray();
            var argumentTypes = arguments.Select(a => a.Type).ToImmutableArray();

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
                    return new BoundErrorExpression();
                }

                var symbol1 = result.Selected.Signature.Symbol;
                var symbol2 = result.Candidates.First(c => c.IsSuitable && c.Signature.Symbol != symbol1).Signature.Symbol;
                Diagnostics.ReportAmbiguousInvocation(node.Span, symbol1, symbol2, argumentTypes);
            }

            // Convert all arguments (if necessary)

            var convertedArguments = arguments.Select((a, i) => BindArgument(a, result, i)).ToImmutableArray();

            return new BoundMethodInvocationExpression(target, convertedArguments, result);
        }

        private BoundExpression BindSingleRowSubselect(SingleRowSubselectSyntax node)
        {
            // TODO: Ensure query has no ORDER BY unless TOP is also specified

            var boundQuery = BindSubquery(node.Query);

            if (boundQuery.OutputColumns.Length == 0)
            {
                // This can happen in cases like this:
                //
                //    SELECT  (SELECT * FROM
                //    FROM EmployeeTerritories et
                //
                // We shouldn't report an error but we can't return bound
                // single row subselect either.
                return new BoundErrorExpression();
            }

            if (boundQuery.OutputColumns.Length > 1)
                Diagnostics.ReportTooManyExpressionsInSelectListOfSubquery(node.Span);

            var value = boundQuery.OutputColumns.First().ValueSlot;

            return new BoundSingleRowSubselect(value, boundQuery.Relation);
        }

        private BoundExpression BindExistsSubselect(ExistsSubselectSyntax node)
        {
            // TODO: Ensure query has no ORDER BY unless TOP is also specified

            var boundQuery = BindSubquery(node.Query);

            // NOTE: Number of columns doesn't matter here

            return new BoundExistsSubselect(boundQuery.Relation);
        }

        private BoundExpression BindAllAnySubselect(AllAnySubselectSyntax node)
        {
            var expressionKind = SyntaxFacts.GetBinaryOperatorExpression(node.OperatorToken.Kind);
            var operatorKind = expressionKind.ToBinaryOperatorKind();
            var isAll = node.Keyword.Kind == SyntaxKind.AllKeyword;
            return BindAllAnySubselect(node.Span, node.Left, isAll, node.Query, operatorKind);
        }

        private BoundExpression BindAllAnySubselect(TextSpan diagnosticSpan, ExpressionSyntax leftNode, bool isAll, QuerySyntax queryNode, BinaryOperatorKind operatorKind)
        {
            // TODO: Ensure query has no ORDER BY unless TOP is also specified

            // First, let's bind the expression and the query

            var left = BindExpression(leftNode);
            var boundQuery = BindSubquery(queryNode);

            // The right hand side of the binary expression is the first column of the query.

            if (boundQuery.OutputColumns.Length == 0)
            {
                var outputValue = ValueSlotFactory.CreateTemporary(typeof(bool));
                return new BoundValueSlotExpression(outputValue);
            }

            if (boundQuery.OutputColumns.Length > 1)
                Diagnostics.ReportTooManyExpressionsInSelectListOfSubquery(queryNode.Span);

            var rightColumn = boundQuery.OutputColumns[0];
            var right = new BoundValueSlotExpression(rightColumn.ValueSlot);

            // Now we need to bind the binary operator.
            //
            // To avoid cascading errors, we'll only validate the operator
            // if we could resolve both sides.

            if (left.Type.IsError() || right.Type.IsError())
                return new BoundErrorExpression();

            var result = LookupBinaryOperator(operatorKind, left.Type, right.Type);
            if (result.Best == null)
            {
                if (result.Selected == null)
                    Diagnostics.ReportCannotApplyBinaryOperator(diagnosticSpan, operatorKind, left.Type, right.Type);
                else
                    Diagnostics.ReportAmbiguousBinaryOperator(diagnosticSpan, operatorKind, left.Type, right.Type);
            }

            // We may need to convert the arguments to the binary operator, so let's
            // bind them as arguments to the resolved operator.

            var convertedLeft = BindArgument(left, result, 0);
            var convertedRight = BindArgument(right, result, 1);

            // If we need to convert the right side, then we musy insert a BoundComputeRelation
            // that produces a derived value.

            BoundRelation relation;

            if (convertedRight == right)
            {
                relation = boundQuery.Relation;
            }
            else
            {
                var outputValue = ValueSlotFactory.CreateTemporary(convertedRight.Type);
                var outputValues = new[] {outputValue};
                var computedValue = new BoundComputedValue(convertedRight, outputValue);
                var computedValues = new[] {computedValue};
                var computeRelation = new BoundComputeRelation(boundQuery.Relation, computedValues);
                relation = new BoundProjectRelation(computeRelation, outputValues);
                convertedRight = new BoundValueSlotExpression(outputValue);
            }

            // In order to simplify later phases, we'll rewrite the the ALL/ANY subselect into
            // a regular EXISTS subselect. ANY is fairly straight forward:
            //
            //      left op ANY (SELECT right FROM ...)
            //
            //      ===>
            //
            //      EXISTS (SELECT * FROM ... WHERE left op right)
            //
            // ALL requires a bit more trickery as we need to handle NULL values in the negated
            // predicate correctly:
            //
            //      left op ALL (SELECT Column FROM ...)
            //
            //      ===>
            //
            //      NOT EXISTS (SELECT * FROM ... WHERE NOT (left op right) OR (left IS NULL) OR (right IS NULL))

            if (!isAll)
            {
                var condition = new BoundBinaryExpression(convertedLeft, operatorKind, result, convertedRight);
                var filter = new BoundFilterRelation(relation, condition);
                return new BoundExistsSubselect(filter);
            }
            else
            {
                var comparison = new BoundBinaryExpression(convertedLeft, operatorKind, result, right);
                var negatedComparison = BindUnaryExpression(diagnosticSpan, UnaryOperatorKind.LogicalNot, comparison);
                var leftIsNull = new BoundIsNullExpression(convertedLeft);
                var rightisNull = new BoundIsNullExpression(right);
                var eitherSideIsNull = BindBinaryExpression(diagnosticSpan, BinaryOperatorKind.LogicalOr, leftIsNull, rightisNull);
                var condition = BindBinaryExpression(diagnosticSpan, BinaryOperatorKind.LogicalOr, negatedComparison, eitherSideIsNull);
                var filter = new BoundFilterRelation(relation, condition);
                var existsSubselect = new BoundExistsSubselect(filter);
                return BindUnaryExpression(diagnosticSpan, UnaryOperatorKind.LogicalNot, existsSubselect);
            }
        }
    }
}