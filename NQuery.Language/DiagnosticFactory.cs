using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NQuery.Language.Symbols;

namespace NQuery.Language
{
    internal static class DiagnosticFactory
    {
        #region Lexer CompilationErrors

        public static Diagnostic IllegalInputCharacter(TextSpan textSpan, char character)
        {
            var message = String.Format(CultureInfo.CurrentCulture, Resources.IllegalInputCharacter, character);
            return new Diagnostic(textSpan, DiagnosticId.IllegalInputCharacter, message);
        }

        public static Diagnostic UnterminatedComment(TextSpan textSpan)
        {
            return new Diagnostic(textSpan, DiagnosticId.UnterminatedComment, Resources.UnterminatedComment);
        }

        public static Diagnostic UnterminatedString(TextSpan textSpan)
        {
            return new Diagnostic(textSpan, DiagnosticId.UnterminatedString, Resources.UnterminatedString);
        }

        public static Diagnostic UnterminatedQuotedIdentifier(TextSpan textSpan)
        {
            return new Diagnostic(textSpan, DiagnosticId.UnterminatedQuotedIdentifier, Resources.UnterminatedQuotedIdentifier);
        }

        public static Diagnostic UnterminatedParenthesizedIdentifier(TextSpan textSpan)
        {
            return new Diagnostic(textSpan, DiagnosticId.UnterminatedParenthesizedIdentifier, Resources.UnterminatedParenthesizedIdentifier);
        }

        public static Diagnostic UnterminatedDate(TextSpan textSpan)
        {
            return new Diagnostic(textSpan, DiagnosticId.UnterminatedDate, Resources.UnterminatedDate);
        }

        public static Diagnostic InvalidDate(TextSpan textSpan, string tokenText)
        {
            var message = String.Format(CultureInfo.CurrentCulture, Resources.InvalidDate, tokenText);
            return new Diagnostic(textSpan, DiagnosticId.InvalidDate, message);
        }

        public static Diagnostic InvalidInteger(TextSpan textSpan, string tokenText)
        {
            var message = String.Format(CultureInfo.CurrentCulture, Resources.InvalidInteger, tokenText);
            return new Diagnostic(textSpan, DiagnosticId.InvalidInteger, message);
        }

        public static Diagnostic InvalidReal(TextSpan textSpan, string tokenText)
        {
            var message = String.Format(CultureInfo.CurrentCulture, Resources.InvalidDecimal, tokenText);
            return new Diagnostic(textSpan, DiagnosticId.InvalidReal, message);
        }

        public static Diagnostic InvalidBinary(TextSpan textSpan, string tokenText)
        {
            var message = String.Format(CultureInfo.CurrentCulture, Resources.InvalidBinary, tokenText);
            return new Diagnostic(textSpan, DiagnosticId.InvalidBinary, message);
        }

        public static Diagnostic InvalidOctal(TextSpan textSpan, string tokenText)
        {
            var message = String.Format(CultureInfo.CurrentCulture, Resources.InvalidOctal, tokenText);
            return new Diagnostic(textSpan, DiagnosticId.InvalidOctal, message);
        }

        public static Diagnostic InvalidHex(TextSpan textSpan, string tokenText)
        {
            var message = String.Format(CultureInfo.CurrentCulture, Resources.InvalidHex, tokenText);
            return new Diagnostic(textSpan, DiagnosticId.InvalidHex, message);
        }

        public static Diagnostic NumberTooLarge(TextSpan textSpan, string tokenText)
        {
            var message = String.Format(CultureInfo.CurrentCulture, Resources.NumberTooLarge, tokenText);
            return new Diagnostic(textSpan, DiagnosticId.NumberTooLarge, message);
        }

        #endregion

        #region Parser Errors


        //public static Diagnostic InvalidTypeReference(SyntaxNodeOrToken nodeOrToken, string tokenText)
        //{
        //    var message = String.Format(CultureInfo.CurrentCulture, Resources.InvalidTypeReference, tokenText);
        //    return new Diagnostic(nodeOrToken, DiagnosticId.InvalidTypeReference, message);
        //}

        public static Diagnostic TokenExpected(SyntaxToken actual, SyntaxKind expected)
        {
            var actualText = actual.Kind.GetDisplayText();
            var expectedText = expected.GetDisplayText();
            var message = String.Format(CultureInfo.CurrentCulture, Resources.TokenExpected, actualText, expectedText);
            return new Diagnostic(actual.Span, DiagnosticId.TokenExpected, message);
        }

        //public static Diagnostic SimpleExpressionExpected(SyntaxNodeOrToken nodeOrToken, string tokenText)
        //{
        //    var message = String.Format(CultureInfo.CurrentCulture, Resources.SimpleExpressionExpected, tokenText);
        //    return new Diagnostic(nodeOrToken, DiagnosticId.SimpleExpressionExpected, message);
        //}

        //public static Diagnostic TableReferenceExpected(SyntaxNodeOrToken nodeOrToken, string foundTokenText)
        //{
        //    var message = String.Format(CultureInfo.CurrentCulture, Resources.TableReferenceExpected, foundTokenText);
        //    return new Diagnostic(nodeOrToken, DiagnosticId.TableReferenceExpected, message);
        //}

        //public static Diagnostic InvalidOperatorForAllAny(SyntaxNodeOrToken nodeOrToken, BinaryOperator foundOp)
        //{
        //    var message = String.Format(CultureInfo.CurrentCulture, Resources.InvalidOperatorForAllAny, foundOp.TokenText);
        //    return new Diagnostic(nodeOrToken, DiagnosticId.InvalidOperatorForAllAny, message);
        //}

        #endregion

        #region Resolving/Evaluation Errors

        public static Diagnostic UndeclaredTable(NamedTableReferenceSyntax namedTableReference)
        {
            var tableName = namedTableReference.TableName;
            var message = String.Format(CultureInfo.CurrentCulture, Resources.UndeclaredTable, tableName.Text);
            return new Diagnostic(tableName.Span, DiagnosticId.UndeclaredTable, message);
        }

        public static Diagnostic UndeclaredVariable(VariableExpressionSyntax node)
        {
            var variableName = node.Name;
            var message = String.Format(CultureInfo.CurrentCulture, Resources.UndeclaredVariable, variableName.Text);
            return new Diagnostic(variableName.Span, DiagnosticId.UndeclaredVariable, message);
        }

        public static Diagnostic UndeclaredFunction(FunctionInvocationExpressionSyntax node, IEnumerable<Type> argumentTypes)
        {
            var name = node.Name.ValueText;
            var argumentTypeList = string.Join(", ", argumentTypes.Select(t => t.Name));
            var message = String.Format(CultureInfo.CurrentCulture, Resources.UndeclaredFunction, name, argumentTypeList);
            return new Diagnostic(node.Span, DiagnosticId.UndeclaredFunction, message);
        }

        public static Diagnostic UndeclaredMethod(MethodInvocationExpressionSyntax node, Type declaringType, IEnumerable<Type> argumentTypes)
        {
            var name = node.Name.Text;
            var argumentTypeList = string.Join(", ", argumentTypes.Select(t => t.Name));
            var message = String.Format(CultureInfo.CurrentCulture, Resources.UndeclaredMethod, declaringType.Name, name, argumentTypeList);
            return new Diagnostic(node.Span, DiagnosticId.UndeclaredMethod, message);
        }

        public static Diagnostic UndeclaredColumn(PropertyAccessExpressionSyntax node, TableInstanceSymbol tableInstance)
        {
            var name = node.Name.Text;
            var message = String.Format(CultureInfo.CurrentCulture, Resources.UndeclaredColumn, tableInstance.Name, name);
            return new Diagnostic(node.Span, DiagnosticId.UndeclaredColumn, message);
        }

        public static Diagnostic UndeclaredProperty(PropertyAccessExpressionSyntax node, Type type)
        {
            var message = String.Format(CultureInfo.CurrentCulture, Resources.UndeclaredProperty, type.Name, node.Name.Text);
            return new Diagnostic(node.Span, DiagnosticId.UndeclaredProperty, message);
        }

        //public static Diagnostic UndeclaredType(SyntaxNodeOrToken nodeOrToken, string typeName)
        //{
        //    var message = String.Format(CultureInfo.CurrentCulture, Resources.UndeclaredType, typeName);
        //    return new Diagnostic(nodeOrToken, DiagnosticId.UndeclaredType, message);
        //}

        public static Diagnostic UndeclaredEntity(NameExpressionSyntax node)
        {
            var name = node.Name.Text;
            var message = String.Format(CultureInfo.CurrentCulture, Resources.UndeclaredEntity, name);
            return new Diagnostic(node.Span, DiagnosticId.UndeclaredEntity, message);
        }

        //public static Diagnostic AmbiguousReference(SyntaxNodeOrToken nodeOrToken, Identifier identifier, Binding[] candidates)
        //{
        //    var message = String.Format(CultureInfo.CurrentCulture, Resources.AmbiguousReference, identifier, FormattingHelpers.FormatBindingListWithCategory(candidates));
        //    return new Diagnostic(nodeOrToken, DiagnosticId.AmbiguousReference, message);
        //}

        //public static Diagnostic AmbiguousTableRef(SyntaxNodeOrToken nodeOrToken, Identifier identifier, TableRefBinding[] candidates)
        //{
        //    var message = String.Format(CultureInfo.CurrentCulture, Resources.AmbiguousTableRef, identifier, FormattingHelpers.FormatBindingList(candidates));
        //    return new Diagnostic(nodeOrToken, DiagnosticId.AmbiguousTableRef, message);
        //}

        //public static Diagnostic AmbiguousColumnRef(SyntaxNodeOrToken nodeOrToken, Identifier identifier, ColumnRefBinding[] candidates)
        //{
        //    var message = String.Format(CultureInfo.CurrentCulture, Resources.AmbiguousColumnRef, identifier, FormattingHelpers.FormatBindingList(candidates));
        //    return new Diagnostic(nodeOrToken, DiagnosticId.AmbiguousColumnRef, message);
        //}

        //public static Diagnostic AmbiguousTable(SyntaxNodeOrToken nodeOrToken, Identifier identifier, TableBinding[] candidates)
        //{
        //    var message = String.Format(CultureInfo.CurrentCulture, Resources.AmbiguousTable, identifier, FormattingHelpers.FormatBindingList(candidates));
        //    return new Diagnostic(nodeOrToken, DiagnosticId.AmbiguousTable, message);
        //}

        //public static Diagnostic AmbiguousConstant(SyntaxNodeOrToken nodeOrToken, Identifier identifier, ConstantBinding[] candidates)
        //{
        //    var message = String.Format(CultureInfo.CurrentCulture, Resources.AmbiguousConstant, identifier, FormattingHelpers.FormatBindingList(candidates));
        //    return new Diagnostic(nodeOrToken, DiagnosticId.AmbiguousConstant, message);
        //}

        //public static Diagnostic AmbiguousParameter(SyntaxNodeOrToken nodeOrToken, Identifier identifier, ParameterBinding[] candidates)
        //{
        //    var message = String.Format(CultureInfo.CurrentCulture, Resources.AmbiguousParameter, identifier, FormattingHelpers.FormatBindingList(candidates));
        //    return new Diagnostic(nodeOrToken, DiagnosticId.AmbiguousParameter, message);
        //}

        //public static Diagnostic AmbiguousAggregate(SyntaxNodeOrToken nodeOrToken, Identifier identifier, AggregateBinding[] candidates)
        //{
        //    var message = String.Format(CultureInfo.CurrentCulture, Resources.AmbiguousAggregate, identifier, FormattingHelpers.FormatBindingList(candidates));
        //    return new Diagnostic(nodeOrToken, DiagnosticId.AmbiguousAggregate, message);
        //}

        //public static Diagnostic AmbiguousProperty(SyntaxNodeOrToken nodeOrToken, Identifier identifier, PropertyBinding[] candidates)
        //{
        //    var message = String.Format(CultureInfo.CurrentCulture, Resources.AmbiguousProperty, identifier, FormattingHelpers.FormatBindingList(candidates));
        //    return new Diagnostic(nodeOrToken, DiagnosticId.AmbiguousProperty, message);
        //}

        //public static Diagnostic AmbiguousType(string typeReference, Type[] candidates)
        //{
        //    var message = String.Format(CultureInfo.CurrentCulture, Resources.AmbiguousType, typeReference, FormattingHelpers.FormatFullyQualifiedTypeList(candidates));
        //    return new Diagnostic(DiagnosticId.AmbiguousType, message);
        //}

        //public static Diagnostic AmbiguousInvocation(InvocableBinding function1, InvocableBinding function2, Type[] argumentTypes)
        //{
        //    if (argumentTypes.Length == 0)
        //    {
        //        var message = String.Format(CultureInfo.CurrentCulture, Resources.AmbiguousInvocationNoArgs, function1.GetFullName(), function2.GetFullName());
        //        return new Diagnostic(DiagnosticId.AmbiguousInvocation, message);
        //    }
        //    else
        //    {
        //        var message = String.Format(CultureInfo.CurrentCulture, Resources.AmbiguousInvocation, function1.GetFullName(), function2.GetFullName(), FormattingHelpers.FormatTypeList(argumentTypes));
        //        return new Diagnostic(DiagnosticId.AmbiguousInvocation, message);
        //    }
        //}

        //public static Diagnostic InvocationRequiresParenthesis(SyntaxNodeOrToken nodeOrToken, InvocableBinding[] invocableGroup)
        //{
        //    var message = String.Format(CultureInfo.CurrentCulture, Resources.InvocationRequiresParenthesis, invocableGroup[0].GetFullName());
        //    return new Diagnostic(nodeOrToken, DiagnosticId.InvocationRequiresParenthesis, message);
        //}

        //public static Diagnostic CannotApplyOperator(UnaryOperator op, Type type)
        //{
        //    var message = String.Format(CultureInfo.CurrentCulture, Resources.CannotApplyUnaryOp, op.TokenText, FormattingHelpers.FormatType(type));
        //    return new Diagnostic(DiagnosticId.CannotApplyUnaryOperator, message);
        //}

        //public static Diagnostic AmbiguousOperator(UnaryOperator op, Type type, MethodInfo opMethod1, MethodInfo opMethod2)
        //{
        //    var message = String.Format(
        //        CultureInfo.CurrentCulture,
        //        Resources.AmbiguousUnaryOp,
        //        op.TokenText,
        //        FormattingHelpers.FormatType(type),
        //        FormattingHelpers.FormatMethodInfo(opMethod1),
        //        FormattingHelpers.FormatMethodInfo(opMethod2)
        //        );

        //    return new Diagnostic(DiagnosticId.AmbiguousUnaryOperator, message);
        //}

        //public static Diagnostic CannotApplyOperator(BinaryOperator op, Type leftType, Type rightType)
        //{
        //    var message = String.Format(
        //        CultureInfo.CurrentCulture,
        //        Resources.CannotApplyBinaryOp,
        //        op.TokenText,
        //        FormattingHelpers.FormatType(leftType),
        //        FormattingHelpers.FormatType(rightType)
        //        );

        //    return new Diagnostic(DiagnosticId.CannotApplyBinaryOperator, message);
        //}

        //public static Diagnostic AmbiguousOperatorOverloading(BinaryOperator op, Type leftType, Type rightType)
        //{
        //    var message = String.Format(
        //        CultureInfo.CurrentCulture,
        //        Resources.AmbiguousOperatorOverloading,
        //        op.TokenText,
        //        FormattingHelpers.FormatType(leftType),
        //        FormattingHelpers.FormatType(rightType)
        //        );

        //    return new Diagnostic(DiagnosticId.AmbiguousOperatorOverloading, message);
        //}

        //public static Diagnostic AmbiguousOperator(BinaryOperator op, Type leftType, Type rightType, MethodInfo opMethod1, MethodInfo opMethod2)
        //{
        //    var message = String.Format(
        //        CultureInfo.CurrentCulture,
        //        Resources.AmbiguousBinaryOperator,
        //        op.TokenText,
        //        FormattingHelpers.FormatType(leftType),
        //        FormattingHelpers.FormatType(rightType),
        //        FormattingHelpers.FormatMethodInfo(opMethod1),
        //        FormattingHelpers.FormatMethodInfo(opMethod2)
        //        );

        //    return new Diagnostic(DiagnosticId.AmbiguousBinaryOperator, message);
        //}

        //public static Diagnostic AmbiguousOperator(CastingOperatorType castingOperatorType, MethodInfo targetFromSource, MethodInfo sourceToTarget)
        //{
        //    var message = String.Format(CultureInfo.CurrentCulture, Resources.AmbiguousCastingOperator, castingOperatorType, FormattingHelpers.FormatMethodInfo(targetFromSource), FormattingHelpers.FormatMethodInfo(sourceToTarget));
        //    return new Diagnostic(DiagnosticId.AmbiguousCastingOperator, message);
        //}

        //public static Diagnostic AsteriskModifierNotAllowed(SyntaxNodeOrToken nodeOrToken, ExpressionNode functionInvocation)
        //{
        //    var message = String.Format(CultureInfo.CurrentCulture, Resources.AsteriskModifierNotAllowed, functionInvocation.GenerateSource());
        //    return new Diagnostic(nodeOrToken, DiagnosticId.AsteriskModifierNotAllowed, message);
        //}

        //public static Diagnostic WhenMustEvaluateToBoolIfCaseInputIsOmitted(ExpressionNode whenExpression)
        //{
        //    var message = String.Format(CultureInfo.CurrentCulture, Resources.WhenMustEvaluateToBoolIfCaseInputIsOmitted, whenExpression.GenerateSource(), FormattingHelpers.FormatType(typeof(bool)));
        //    return new Diagnostic(DiagnosticId.WhenMustEvaluateToBoolIfCaseInputIsOmitted, message);
        //}

        //public static Diagnostic CannotLoadTypeAssembly(string assemblyName, Exception exception)
        //{
        //    var message = String.Format(CultureInfo.CurrentCulture, Resources.CannotLoadTypeAssembly, assemblyName, exception.Message);
        //    return new Diagnostic(DiagnosticId.CannotLoadTypeAssembly, message);
        //}

        //public static Diagnostic CannotFoldConstants(RuntimeException exception)
        //{
        //    var message = String.Format(CultureInfo.CurrentCulture, Resources.CannotFoldConstants, exception.Message);
        //    return new Diagnostic(DiagnosticId.CannotFoldConstants, message);
        //}

        //public static Diagnostic CannotCast(ExpressionNode expression, Type targetType)
        //{
        //    var message = String.Format(CultureInfo.CurrentCulture, Resources.CannotCast, expression.GenerateSource(), FormattingHelpers.FormatType(expression.ExpressionType), FormattingHelpers.FormatType(targetType));
        //    return new Diagnostic(DiagnosticId.CannotCast, message);
        //}

        #endregion

        #region Query Errors

        //public static Diagnostic MustSpecifyTableToSelectFrom()
        //{
        //    return new Diagnostic(DiagnosticId.MustSpecifyTableToSelectFrom, Resources.MustSpecifyTableToSelectFrom);
        //}

        //public static Diagnostic AggregateCannotContainAggregate(AggregateExpression expression, AggregateBinding parent, AggregateBinding nested)
        //{
        //    var message = String.Format(CultureInfo.CurrentCulture, Resources.AggregateCannotContainAggregate, expression.GenerateSource(), parent.Name, nested.Name);
        //    return new Diagnostic(DiagnosticId.AggregateCannotContainAggregate, message);
        //}

        //public static Diagnostic AggregateCannotContainSubquery(AggregateExpression expression)
        //{
        //    var message = String.Format(CultureInfo.CurrentCulture, Resources.AggregateCannotContainSubquery, expression.GenerateSource());
        //    return new Diagnostic(DiagnosticId.AggregateCannotContainAggregate, message);
        //}

        //public static Diagnostic AggregateDoesNotSupportType(AggregateBinding aggregateBinding, Type argumentType)
        //{
        //    var message = String.Format(CultureInfo.CurrentCulture, Resources.AggregateDoesNotSupportType, aggregateBinding.Name, FormattingHelpers.FormatType(argumentType));
        //    return new Diagnostic(DiagnosticId.AggregateDoesNotSupportType, message);
        //}

        //public static Diagnostic AggregateInWhere()
        //{
        //    return new Diagnostic(DiagnosticId.AggregateInWhere, Resources.AggregateInWhere);
        //}

        //public static Diagnostic AggregateInOn()
        //{
        //    return new Diagnostic(DiagnosticId.AggregateInOn, Resources.AggregateInOn);
        //}

        //public static Diagnostic AggregateInGroupBy()
        //{
        //    return new Diagnostic(DiagnosticId.AggregateInGroupBy, Resources.AggregateInGroupBy);
        //}

        //public static Diagnostic AggregateContainsColumnsFromDifferentQueries(ExpressionNode aggregateArgument)
        //{
        //    var message = String.Format(CultureInfo.CurrentCulture, Resources.AggregateContainsColumnsFromDifferentQueries, aggregateArgument.GenerateSource());
        //    return new Diagnostic(DiagnosticId.AggregateContainsColumnsFromDifferentQueries, message);
        //}

        //public static Diagnostic AggregateInvalidInCurrentContext(AggregateExpression aggregateExpression)
        //{
        //    var message = String.Format(CultureInfo.CurrentCulture, Resources.AggregateInvalidInCurrentContext, aggregateExpression.GenerateSource());
        //    return new Diagnostic(DiagnosticId.AggregateInvalidInCurrentContext, message);
        //}

        //public static Diagnostic DuplicateTableRefInFrom(Identifier identifier)
        //{
        //    var message = String.Format(CultureInfo.CurrentCulture, Resources.DuplicateTableRefInFrom, identifier);
        //    return new Diagnostic(DiagnosticId.DuplicateTableRefInFrom, message);
        //}

        //public static Diagnostic TableRefInaccessible(TableRefBinding tableRefBinding)
        //{
        //    var message = String.Format(CultureInfo.CurrentCulture, Resources.TableRefInaccessible, tableRefBinding.Name, tableRefBinding.TableBinding.Name);
        //    return new Diagnostic(DiagnosticId.TableRefInaccessible, message);
        //}

        //public static Diagnostic TopWithTiesRequiresOrderBy()
        //{
        //    return new Diagnostic(DiagnosticId.TopWithTiesRequiresOrderBy, Resources.TopWithTiesRequiresOrderBy);
        //}

        //public static Diagnostic OrderByColumnPositionIsOutOfRange(long index)
        //{
        //    var message = String.Format(CultureInfo.CurrentCulture, Resources.OrderByColumnPositionIsOutOfRange, index);
        //    return new Diagnostic(DiagnosticId.OrderByColumnPositionIsOutOfRange, message);
        //}

        //public static Diagnostic WhereClauseMustEvaluateToBool()
        //{
        //    return new Diagnostic(DiagnosticId.WhereClauseMustEvaluateToBool, Resources.WhereClauseMustEvaluateToBool);
        //}

        //public static Diagnostic HavingClauseMustEvaluateToBool()
        //{
        //    return new Diagnostic(DiagnosticId.HavingClauseMustEvaluateToBool, Resources.HavingClauseMustEvaluateToBool);
        //}

        //public static Diagnostic SelectExpressionNotAggregatedAndNoGroupBy(ColumnRefBinding columnRefBinding)
        //{
        //    var message = String.Format(CultureInfo.CurrentCulture, Resources.SelectExpressionNotAggregatedAndNoGroupBy, columnRefBinding.GetFullName());
        //    return new Diagnostic(DiagnosticId.SelectExpressionNotAggregatedAndNoGroupBy, message);
        //}

        //public static Diagnostic SelectExpressionNotAggregatedOrGrouped(ColumnRefBinding columnRefBinding)
        //{
        //    var message = String.Format(CultureInfo.CurrentCulture, Resources.SelectExpressionNotAggregatedOrGrouped, columnRefBinding.GetFullName());
        //    return new Diagnostic(DiagnosticId.SelectExpressionNotAggregatedOrGrouped, message);
        //}

        //public static Diagnostic HavingExpressionNotAggregatedOrGrouped(ColumnRefBinding columnRefBinding)
        //{
        //    var message = String.Format(CultureInfo.CurrentCulture, Resources.HavingExpressionNotAggregatedOrGrouped, columnRefBinding.GetFullName());
        //    return new Diagnostic(DiagnosticId.HavingExpressionNotAggregatedOrGrouped, message);
        //}

        //public static Diagnostic OrderByExpressionNotAggregatedAndNoGroupBy(ColumnRefBinding columnRefBinding)
        //{
        //    var message = String.Format(CultureInfo.CurrentCulture, Resources.OrderByExpressionNotAggregatedAndNoGroupBy, columnRefBinding.GetFullName());
        //    return new Diagnostic(DiagnosticId.OrderByExpressionNotAggregatedAndNoGroupBy, message);
        //}

        //public static Diagnostic OrderByExpressionNotAggregatedOrGrouped(ColumnRefBinding columnRefBinding)
        //{
        //    var message = String.Format(CultureInfo.CurrentCulture, Resources.OrderByExpressionNotAggregatedOrGrouped, columnRefBinding.GetFullName());
        //    return new Diagnostic(DiagnosticId.OrderByExpressionNotAggregatedOrGrouped, message);
        //}

        //public static Diagnostic OrderByInvalidInSubqueryUnlessTopIsAlsoSpecified()
        //{
        //    return new Diagnostic(DiagnosticId.OrderByInvalidInSubqueryUnlessTopIsAlsoSpecified, Resources.OrderByInvalidInSubqueryUnlessTopIsAlsoSpecified);
        //}

        //public static Diagnostic InvalidDataTypeInSelectDistinct(Type expressionType)
        //{
        //    var message = String.Format(CultureInfo.CurrentCulture, Resources.InvalidDataTypeInSelectDistinct, FormattingHelpers.FormatType(expressionType));
        //    return new Diagnostic(DiagnosticId.InvalidDataTypeInSelectDistinct, message);
        //}

        //public static Diagnostic InvalidDataTypeInGroupBy(Type expressionType)
        //{
        //    var message = String.Format(CultureInfo.CurrentCulture, Resources.InvalidDataTypeInGroupBy, FormattingHelpers.FormatType(expressionType));
        //    return new Diagnostic(DiagnosticId.InvalidDataTypeInGroupBy, message);
        //}

        //public static Diagnostic InvalidDataTypeInOrderBy(Type expressionType)
        //{
        //    var message = String.Format(CultureInfo.CurrentCulture, Resources.InvalidDataTypeInOrderBy, FormattingHelpers.FormatType(expressionType));
        //    return new Diagnostic(DiagnosticId.InvalidDataTypeInOrderBy, message);
        //}

        //public static Diagnostic InvalidDataTypeInUnion(Type expressionType, BinaryQueryOperator unionOperator)
        //{
        //    string unionOperatorString = unionOperator.ToString().ToUpper(CultureInfo.CurrentCulture);
        //    var message = String.Format(CultureInfo.CurrentCulture, Resources.InvalidDataTypeInUnion, FormattingHelpers.FormatType(expressionType), unionOperatorString);
        //    return new Diagnostic(DiagnosticId.InvalidDataTypeInUnion, message);
        //}

        //public static Diagnostic DifferentExpressionCountInBinaryQuery()
        //{
        //    return new Diagnostic(DiagnosticId.DifferentExpressionCountInBinaryQuery, Resources.DifferentExpressionCountInBinaryQuery);
        //}

        //public static Diagnostic OrderByItemsMustBeInSelectListIfUnionSpecified()
        //{
        //    return new Diagnostic(DiagnosticId.OrderByItemsMustBeInSelectListIfUnionSpecified, Resources.OrderByItemsMustBeInSelectListIfUnionSpecified);
        //}

        //public static Diagnostic OrderByItemsMustBeInSelectListIfDistinctSpecified()
        //{
        //    return new Diagnostic(DiagnosticId.OrderByItemsMustBeInSelectListIfDistinctSpecified, Resources.OrderByItemsMustBeInSelectListIfDistinctSpecified);
        //}

        //public static Diagnostic GroupByItemDoesNotReferenceAnyColumns()
        //{
        //    return new Diagnostic(DiagnosticId.GroupByItemDoesNotReferenceAnyColumns, Resources.GroupByItemDoesNotReferenceAnyColumns);
        //}

        //public static Diagnostic ConstantExpressionInOrderBy()
        //{
        //    return new Diagnostic(DiagnosticId.ConstantExpressionInOrderBy, Resources.ConstantExpressionInOrderBy);
        //}

        //public static Diagnostic TooManyExpressionsInSelectListOfSubquery()
        //{
        //    return new Diagnostic(DiagnosticId.TooManyExpressionsInSelectListOfSubquery, Resources.TooManyExpressionsInSelectListOfSubquery);
        //}

        //public static Diagnostic InvalidRowReference(SyntaxNodeOrToken nodeOrToken, TableRefBinding derivedTableRef)
        //{
        //    var message = String.Format(CultureInfo.CurrentCulture, Resources.InvalidRowReference, derivedTableRef.Name);
        //    return new Diagnostic(nodeOrToken, DiagnosticId.InvalidRowReference, message);
        //}

        //public static Diagnostic NoColumnAliasSpecified(SyntaxNodeOrToken nodeOrToken, int columnIndex, Identifier derivedTableName)
        //{
        //    var message = String.Format(CultureInfo.CurrentCulture, Resources.NoColumnAliasSpecified, columnIndex + 1, derivedTableName.ToSource());
        //    return new Diagnostic(nodeOrToken, DiagnosticId.NoColumnAliasSpecified, message);
        //}

        //public static Diagnostic CteHasMoreColumnsThanSpecified(Identifier cteTableName)
        //{
        //    var message = String.Format(CultureInfo.CurrentCulture, Resources.CteHasMoreColumnsThanSpecified, cteTableName.Text);
        //    return new Diagnostic(DiagnosticId.CteHasMoreColumnsThanSpecified, message);
        //}

        //public static Diagnostic CteHasFewerColumnsThanSpecified(Identifier cteTableName)
        //{
        //    var message = String.Format(CultureInfo.CurrentCulture, Resources.CteHasFewerColumnsThanSpecified, cteTableName.Text);
        //    return new Diagnostic(DiagnosticId.CteHasFewerColumnsThanSpecified, message);
        //}

        //public static Diagnostic CteHasDuplicateColumnName(Identifier columnName, Identifier cteTableName)
        //{
        //    var message = String.Format(CultureInfo.CurrentCulture, Resources.CteHasDuplicateColumnName, columnName.Text, cteTableName.Text);
        //    return new Diagnostic(DiagnosticId.CteHasDuplicateColumnName, message);
        //}

        //public static Diagnostic CteHasDuplicateTableName(Identifier cteTableName)
        //{
        //    var message = String.Format(CultureInfo.CurrentCulture, Resources.CteHasDuplicateTableName, cteTableName.Text);
        //    return new Diagnostic(DiagnosticId.CteHasDuplicateTableName, message);
        //}

        //public static Diagnostic CteDoesNotHaveUnionAll(Identifier cteTableName)
        //{
        //    var message = String.Format(CultureInfo.CurrentCulture, Resources.CteDoesNotHaveUnionAll, cteTableName.Text);
        //    return new Diagnostic(DiagnosticId.CteDoesNotHaveUnionAll, message);
        //}

        //public static Diagnostic CteDoesNotHaveAnchorMember(Identifier cteTableName)
        //{
        //    var message = String.Format(CultureInfo.CurrentCulture, Resources.CteDoesNotHaveAnchorMember, cteTableName.Text);
        //    return new Diagnostic(DiagnosticId.CteDoesNotHaveAnchorMember, message);
        //}

        //public static Diagnostic CteContainsRecursiveReferenceInSubquery(Identifier cteTableName)
        //{
        //    var message = String.Format(CultureInfo.CurrentCulture, Resources.CteContainsRecursiveReferenceInSubquery, cteTableName.Text);
        //    return new Diagnostic(DiagnosticId.CteContainsRecursiveReferenceInSubquery, message);
        //}

        //public static Diagnostic CteContainsUnexpectedAnchorMember(Identifier cteTableName)
        //{
        //    var message = String.Format(CultureInfo.CurrentCulture, Resources.CteContainsUnexpectedAnchorMember, cteTableName.Text);
        //    return new Diagnostic(DiagnosticId.CteContainsUnexpectedAnchorMember, message);
        //}

        //public static Diagnostic CteContainsMultipleRecursiveReferences(Identifier cteTableName)
        //{
        //    var message = String.Format(CultureInfo.CurrentCulture, Resources.CteContainsMultipleRecursiveReferences, cteTableName.Text);
        //    return new Diagnostic(DiagnosticId.CteContainsMultipleRecursiveReferences, message);
        //}

        //public static Diagnostic CteContainsUnion(Identifier cteTableName)
        //{
        //    var message = String.Format(CultureInfo.CurrentCulture, Resources.CteContainsUnion, cteTableName.Text);
        //    return new Diagnostic(DiagnosticId.CteContainsUnion, message);
        //}

        //public static Diagnostic CteContainsDistinct(Identifier cteTableName)
        //{
        //    var message = String.Format(CultureInfo.CurrentCulture, Resources.CteContainsDistinct, cteTableName.Text);
        //    return new Diagnostic(DiagnosticId.CteContainsDistinct, message);
        //}

        //public static Diagnostic CteContainsTop(Identifier cteTableName)
        //{
        //    var message = String.Format(CultureInfo.CurrentCulture, Resources.CteContainsTop, cteTableName.Text);
        //    return new Diagnostic(DiagnosticId.CteContainsTop, message);
        //}

        //public static Diagnostic CteContainsOuterJoin(Identifier cteTableName)
        //{
        //    var message = String.Format(CultureInfo.CurrentCulture, Resources.CteContainsOuterJoin, cteTableName.Text);
        //    return new Diagnostic(DiagnosticId.CteContainsOuterJoin, message);
        //}

        //public static Diagnostic CteContainsGroupByHavingOrAggregate(Identifier cteTableName)
        //{
        //    var message = String.Format(CultureInfo.CurrentCulture, Resources.CteContainsGroupByHavingOrAggregate, cteTableName.Text);
        //    return new Diagnostic(DiagnosticId.CteContainsGroupByHavingOrAggregate, message);
        //}

        //public static Diagnostic CteHasTypeMismatchBetweenAnchorAndRecursivePart(Identifier columnName, Identifier cteTableName)
        //{
        //    var message = String.Format(CultureInfo.CurrentCulture, Resources.CteHasTypeMismatchBetweenAnchorAndRecursivePart, columnName.Text, cteTableName);
        //    return new Diagnostic(DiagnosticId.CteHasTypeMismatchBetweenAnchorAndRecursivePart, message);
        //}

        #endregion
    }
}