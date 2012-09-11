using System;
using System.Collections.Generic;

using NQuery.Language.Symbols;

namespace NQuery.Language
{
    internal static class DiagnosticExtensions
    {
        #region Lexer CompilationErrors

        public static void ReportIllegalInputCharacter(this ICollection<Diagnostic> diagnostics, TextSpan textSpan, char character)
        {
            diagnostics.Add(DiagnosticFactory.IllegalInputCharacter(textSpan, character));
        }

        public static void ReportUnterminatedComment(this ICollection<Diagnostic> diagnostics, TextSpan textSpan)
        {
            diagnostics.Add(DiagnosticFactory.UnterminatedComment(textSpan));
        }

        public static void ReportUnterminatedString(this ICollection<Diagnostic> diagnostics, TextSpan textSpan)
        {
            diagnostics.Add(DiagnosticFactory.UnterminatedString(textSpan));
        }

        public static void ReportUnterminatedQuotedIdentifier(this ICollection<Diagnostic> diagnostics, TextSpan textSpan)
        {
            diagnostics.Add(DiagnosticFactory.UnterminatedQuotedIdentifier(textSpan));
        }

        public static void ReportUnterminatedParenthesizedIdentifier(this ICollection<Diagnostic> diagnostics, TextSpan textSpan)
        {
            diagnostics.Add(DiagnosticFactory.UnterminatedParenthesizedIdentifier(textSpan));
        }

        public static void ReportUnterminatedDate(this ICollection<Diagnostic> diagnostics, TextSpan textSpan)
        {
            diagnostics.Add(DiagnosticFactory.UnterminatedDate(textSpan));
        }

        public static void ReportInvalidDate(this ICollection<Diagnostic> diagnostics, TextSpan textSpan, string tokenText)
        {
            diagnostics.Add(DiagnosticFactory.InvalidDate(textSpan, tokenText));
        }

        public static void ReportInvalidInteger(this ICollection<Diagnostic> diagnostics, TextSpan textSpan, string tokenText)
        {
            diagnostics.Add(DiagnosticFactory.InvalidInteger(textSpan, tokenText));
        }

        public static void ReportInvalidReal(this ICollection<Diagnostic> diagnostics, TextSpan textSpan, string tokenText)
        {
            diagnostics.Add(DiagnosticFactory.InvalidReal(textSpan, tokenText));
        }

        public static void ReportInvalidBinary(this ICollection<Diagnostic> diagnostics, TextSpan textSpan, string tokenText)
        {
            diagnostics.Add(DiagnosticFactory.InvalidBinary(textSpan, tokenText));
        }

        public static void ReportInvalidOctal(this ICollection<Diagnostic> diagnostics, TextSpan textSpan, string tokenText)
        {
            diagnostics.Add(DiagnosticFactory.InvalidOctal(textSpan, tokenText));
        }

        public static void ReportInvalidHex(this ICollection<Diagnostic> diagnostics, TextSpan textSpan, string tokenText)
        {
            diagnostics.Add(DiagnosticFactory.InvalidHex(textSpan, tokenText));
        }

        public static void ReportNumberTooLarge(this ICollection<Diagnostic> diagnostics, TextSpan textSpan, string tokenText)
        {
            diagnostics.Add(DiagnosticFactory.NumberTooLarge(textSpan, tokenText));
        }

        #endregion

        #region Parser Errors

        //public static void ReportInvalidTypeReference(this ICollection<Diagnostic> diagnostics, SyntaxNodeOrToken nodeOrToken, string tokenText)
        //{
        //    diagnostics.Add(DiagnosticFactory.InvalidTypeReference(nodeOrToken, tokenText));
        //}

        //public static void ReportTokenExpected(this ICollection<Diagnostic> diagnostics, SyntaxNodeOrToken nodeOrToken, string foundTokenText, TokenId expected)
        //{
        //    diagnostics.Add(DiagnosticFactory.TokenExpected(nodeOrToken, foundTokenText, expected));
        //}

        //public static void ReportSimpleExpressionExpected(this ICollection<Diagnostic> diagnostics, SyntaxNodeOrToken nodeOrToken, string tokenText)
        //{
        //    diagnostics.Add(DiagnosticFactory.SimpleExpressionExpected(nodeOrToken, tokenText));
        //}

        //public static void ReportTableReferenceExpected(this ICollection<Diagnostic> diagnostics, SyntaxNodeOrToken nodeOrToken, string foundTokenText)
        //{
        //    diagnostics.Add(DiagnosticFactory.TableReferenceExpected(nodeOrToken, foundTokenText));
        //}

        //public static void ReportInvalidOperatorForAllAny(this ICollection<Diagnostic> diagnostics, SyntaxNodeOrToken nodeOrToken, BinaryOperator foundOp)
        //{
        //    diagnostics.Add(DiagnosticFactory.InvalidOperatorForAllAny(nodeOrToken, foundOp));
        //}

        #endregion

        #region Resolving/Evaluation Errors

        public static void ReportUndeclaredTable(this ICollection<Diagnostic> diagnostics, NamedTableReferenceSyntax namedTableReference)
        {
            diagnostics.Add(DiagnosticFactory.UndeclaredTable(namedTableReference));
        }

        public static void ReportUndeclaredVariable(this ICollection<Diagnostic> diagnostics, VariableExpressionSyntax node)
        {
            diagnostics.Add(DiagnosticFactory.UndeclaredVariable(node));
        }

        public static void ReportUndeclaredFunction(this ICollection<Diagnostic> diagnostics, FunctionInvocationExpressionSyntax node, IEnumerable<Type> argumentTypes)
        {
            diagnostics.Add(DiagnosticFactory.UndeclaredFunction(node, argumentTypes));
        }

        public static void ReportUndeclaredMethod(this ICollection<Diagnostic> diagnostics, MethodInvocationExpressionSyntax node, Type declaringType, IEnumerable<Type> argumentTypes)
        {
            diagnostics.Add(DiagnosticFactory.UndeclaredMethod(node, declaringType, argumentTypes));
        }

        public static void ReportUndeclaredColumn(this ICollection<Diagnostic> diagnostics, PropertyAccessExpressionSyntax node, TableInstanceSymbol tableInstance)
        {
            diagnostics.Add(DiagnosticFactory.UndeclaredColumn(node, tableInstance));
        }

        public static void ReportUndeclaredProperty(this ICollection<Diagnostic> diagnostics, PropertyAccessExpressionSyntax node, Type type)
        {
            diagnostics.Add(DiagnosticFactory.UndeclaredProperty(node, type));
        }

        public static void ReportUndeclaredType(this ICollection<Diagnostic> diagnostics, SyntaxToken typeName)
        {
            diagnostics.Add(DiagnosticFactory.UndeclaredType(typeName));
        }

        public static void ReportUndeclaredEntity(this ICollection<Diagnostic> diagnostics, NameExpressionSyntax node)
        {
            diagnostics.Add(DiagnosticFactory.UndeclaredEntity(node));
        }

        //public static void ReportAmbiguousReference(this ICollection<Diagnostic> diagnostics, SyntaxNodeOrToken nodeOrToken, Identifier identifier, Binding[] candidates)
        //{
        //    diagnostics.Add(DiagnosticFactory.AmbiguousReference(nodeOrToken, identifier, candidates));
        //}

        //public static void ReportAmbiguousTableRef(this ICollection<Diagnostic> diagnostics, SyntaxNodeOrToken nodeOrToken, Identifier identifier, TableRefBinding[] candidates)
        //{
        //    diagnostics.Add(DiagnosticFactory.AmbiguousTableRef(nodeOrToken, identifier, candidates));
        //}

        //public static void ReportAmbiguousColumnRef(this ICollection<Diagnostic> diagnostics, SyntaxNodeOrToken nodeOrToken, Identifier identifier, ColumnRefBinding[] candidates)
        //{
        //    diagnostics.Add(DiagnosticFactory.AmbiguousColumnRef(nodeOrToken, identifier, candidates));
        //}

        //public static void ReportAmbiguousTable(this ICollection<Diagnostic> diagnostics, SyntaxNodeOrToken nodeOrToken, Identifier identifier, TableBinding[] candidates)
        //{
        //    diagnostics.Add(DiagnosticFactory.AmbiguousTable(nodeOrToken, identifier, candidates));
        //}

        //public static void ReportAmbiguousConstant(this ICollection<Diagnostic> diagnostics, SyntaxNodeOrToken nodeOrToken, Identifier identifier, ConstantBinding[] candidates)
        //{
        //    diagnostics.Add(DiagnosticFactory.AmbiguousConstant(nodeOrToken, identifier, candidates));
        //}

        //public static void ReportAmbiguousParameter(this ICollection<Diagnostic> diagnostics, SyntaxNodeOrToken nodeOrToken, Identifier identifier, ParameterBinding[] candidates)
        //{
        //    diagnostics.Add(DiagnosticFactory.AmbiguousParameter(nodeOrToken, identifier, candidates));
        //}

        //public static void ReportAmbiguousAggregate(this ICollection<Diagnostic> diagnostics, SyntaxNodeOrToken nodeOrToken, Identifier identifier, AggregateBinding[] candidates)
        //{
        //    diagnostics.Add(DiagnosticFactory.AmbiguousAggregate(nodeOrToken, identifier, candidates));
        //}

        //public static void ReportAmbiguousProperty(this ICollection<Diagnostic> diagnostics, SyntaxNodeOrToken nodeOrToken, Identifier identifier, PropertyBinding[] candidates)
        //{
        //    diagnostics.Add(DiagnosticFactory.AmbiguousProperty(nodeOrToken, identifier, candidates));
        //}

        //public static void ReportAmbiguousType(this ICollection<Diagnostic> diagnostics, string typeReference, Type[] candidates)
        //{
        //    diagnostics.Add(DiagnosticFactory.AmbiguousType(typeReference, candidates));
        //}

        //public static void ReportAmbiguousInvocation(this ICollection<Diagnostic> diagnostics, InvocableBinding function1, InvocableBinding function2, Type[] argumentTypes)
        //{
        //    diagnostics.Add(DiagnosticFactory.AmbiguousInvocation(function1, function2, argumentTypes));
        //}

        //public static void ReportInvocationRequiresParenthesis(this ICollection<Diagnostic> diagnostics, SyntaxNodeOrToken nodeOrToken, InvocableBinding[] invocableGroup)
        //{
        //    diagnostics.Add(DiagnosticFactory.InvocationRequiresParenthesis(nodeOrToken, invocableGroup));
        //}

        //public static void ReportCannotApplyOperator(this ICollection<Diagnostic> diagnostics, UnaryOperator op, Type type)
        //{
        //    diagnostics.Add(DiagnosticFactory.CannotApplyOperator(op, type));
        //}

        //public static void ReportAmbiguousOperator(this ICollection<Diagnostic> diagnostics, UnaryOperator op, Type type, MethodInfo opMethod1, MethodInfo opMethod2)
        //{
        //    diagnostics.Add(DiagnosticFactory.AmbiguousOperator(op, type, opMethod1, opMethod2));
        //}

        //public static void ReportCannotApplyOperator(this ICollection<Diagnostic> diagnostics, BinaryOperator op, Type leftType, Type rightType)
        //{
        //    diagnostics.Add(DiagnosticFactory.CannotApplyOperator(op, leftType, rightType));
        //}

        //public static void ReportAmbiguousOperatorOverloading(this ICollection<Diagnostic> diagnostics, BinaryOperator op, Type leftType, Type rightType)
        //{
        //    diagnostics.Add(DiagnosticFactory.AmbiguousOperatorOverloading(op, leftType, rightType));
        //}

        //public static void ReportAmbiguousOperator(this ICollection<Diagnostic> diagnostics, BinaryOperator op, Type leftType, Type rightType, MethodInfo opMethod1, MethodInfo opMethod2)
        //{
        //    diagnostics.Add(DiagnosticFactory.AmbiguousOperator(op, leftType, rightType, opMethod1, opMethod2));
        //}

        //public static void ReportAmbiguousOperator(this ICollection<Diagnostic> diagnostics, CastingOperatorType castingOperatorType, MethodInfo targetFromSource, MethodInfo sourceToTarget)
        //{
        //    diagnostics.Add(DiagnosticFactory.AmbiguousOperator(castingOperatorType, targetFromSource, sourceToTarget));
        //}

        //public static void ReportAsteriskModifierNotAllowed(this ICollection<Diagnostic> diagnostics, SyntaxNodeOrToken nodeOrToken, ExpressionNode functionInvocation)
        //{
        //    diagnostics.Add(DiagnosticFactory.AsteriskModifierNotAllowed(nodeOrToken, functionInvocation));
        //}

        //public static void ReportWhenMustEvaluateToBoolIfCaseInputIsOmitted(this ICollection<Diagnostic> diagnostics, ExpressionNode whenExpression)
        //{
        //    diagnostics.Add(DiagnosticFactory.WhenMustEvaluateToBoolIfCaseInputIsOmitted(whenExpression));
        //}

        //public static void ReportCannotLoadTypeAssembly(this ICollection<Diagnostic> diagnostics, string assemblyName, Exception exception)
        //{
        //    diagnostics.Add(DiagnosticFactory.CannotLoadTypeAssembly(assemblyName, exception));
        //}

        //public static void ReportCannotFoldConstants(this ICollection<Diagnostic> diagnostics, RuntimeException exception)
        //{
        //    diagnostics.Add(DiagnosticFactory.CannotFoldConstants(exception));
        //}

        public static void ReportCannotConvert(this ICollection<Diagnostic> diagnostics, CastExpressionSyntax expression, Type sourceType, Type targetType)
        {
            diagnostics.Add(DiagnosticFactory.CannotConvert(expression, sourceType, targetType));
        }

        #endregion

        #region Query Errors

        //public static void ReportMustSpecifyTableToSelectFrom(this ICollection<Diagnostic> diagnostics)
        //{
        //    diagnostics.Add(DiagnosticFactory.MustSpecifyTableToSelectFrom());
        //}

        //public static void ReportAggregateCannotContainAggregate(this ICollection<Diagnostic> diagnostics, AggregateExpression expression, AggregateBinding parent, AggregateBinding nested)
        //{
        //    diagnostics.Add(DiagnosticFactory.AggregateCannotContainAggregate(expression, parent, nested));
        //}

        //public static void ReportAggregateCannotContainSubquery(this ICollection<Diagnostic> diagnostics, AggregateExpression expression)
        //{
        //    diagnostics.Add(DiagnosticFactory.AggregateCannotContainSubquery(expression));
        //}

        //public static void ReportAggregateDoesNotSupportType(this ICollection<Diagnostic> diagnostics, AggregateBinding aggregateBinding, Type argumentType)
        //{
        //    diagnostics.Add(DiagnosticFactory.AggregateDoesNotSupportType(aggregateBinding, argumentType));
        //}

        //public static void ReportAggregateInWhere(this ICollection<Diagnostic> diagnostics)
        //{
        //    diagnostics.Add(DiagnosticFactory.AggregateInWhere());
        //}

        //public static void ReportAggregateInOn(this ICollection<Diagnostic> diagnostics)
        //{
        //    diagnostics.Add(DiagnosticFactory.AggregateInOn());
        //}

        //public static void ReportAggregateInGroupBy(this ICollection<Diagnostic> diagnostics)
        //{
        //    diagnostics.Add(DiagnosticFactory.AggregateInGroupBy());
        //}

        //public static void ReportAggregateContainsColumnsFromDifferentQueries(this ICollection<Diagnostic> diagnostics, ExpressionNode aggregateArgument)
        //{
        //    diagnostics.Add(DiagnosticFactory.AggregateContainsColumnsFromDifferentQueries(aggregateArgument));
        //}

        //public static void ReportAggregateInvalidInCurrentContext(this ICollection<Diagnostic> diagnostics, AggregateExpression aggregateExpression)
        //{
        //    diagnostics.Add(DiagnosticFactory.AggregateInvalidInCurrentContext(aggregateExpression));
        //}

        //public static void ReportDuplicateTableRefInFrom(this ICollection<Diagnostic> diagnostics, Identifier identifier)
        //{
        //    diagnostics.Add(DiagnosticFactory.DuplicateTableRefInFrom(identifier));
        //}

        //public static void ReportTableRefInaccessible(this ICollection<Diagnostic> diagnostics, TableRefBinding tableRefBinding)
        //{
        //    diagnostics.Add(DiagnosticFactory.TableRefInaccessible(tableRefBinding));
        //}

        //public static void ReportTopWithTiesRequiresOrderBy(this ICollection<Diagnostic> diagnostics)
        //{
        //    diagnostics.Add(DiagnosticFactory.TopWithTiesRequiresOrderBy());
        //}

        //public static void ReportOrderByColumnPositionIsOutOfRange(this ICollection<Diagnostic> diagnostics, long index)
        //{
        //    diagnostics.Add(DiagnosticFactory.OrderByColumnPositionIsOutOfRange(index));
        //}

        //public static void ReportWhereClauseMustEvaluateToBool(this ICollection<Diagnostic> diagnostics)
        //{
        //    diagnostics.Add(DiagnosticFactory.WhereClauseMustEvaluateToBool());
        //}

        //public static void ReportHavingClauseMustEvaluateToBool(this ICollection<Diagnostic> diagnostics)
        //{
        //    diagnostics.Add(DiagnosticFactory.HavingClauseMustEvaluateToBool());
        //}

        //public static void ReportSelectExpressionNotAggregatedAndNoGroupBy(this ICollection<Diagnostic> diagnostics, ColumnRefBinding columnRefBinding)
        //{
        //    diagnostics.Add(DiagnosticFactory.SelectExpressionNotAggregatedAndNoGroupBy(columnRefBinding));
        //}

        //public static void ReportSelectExpressionNotAggregatedOrGrouped(this ICollection<Diagnostic> diagnostics, ColumnRefBinding columnRefBinding)
        //{
        //    diagnostics.Add(DiagnosticFactory.SelectExpressionNotAggregatedOrGrouped(columnRefBinding));
        //}

        //public static void ReportHavingExpressionNotAggregatedOrGrouped(this ICollection<Diagnostic> diagnostics, ColumnRefBinding columnRefBinding)
        //{
        //    diagnostics.Add(DiagnosticFactory.HavingExpressionNotAggregatedOrGrouped(columnRefBinding));
        //}

        //public static void ReportOrderByExpressionNotAggregatedAndNoGroupBy(this ICollection<Diagnostic> diagnostics, ColumnRefBinding columnRefBinding)
        //{
        //    diagnostics.Add(DiagnosticFactory.OrderByExpressionNotAggregatedAndNoGroupBy(columnRefBinding));
        //}

        //public static void ReportOrderByExpressionNotAggregatedOrGrouped(this ICollection<Diagnostic> diagnostics, ColumnRefBinding columnRefBinding)
        //{
        //    diagnostics.Add(DiagnosticFactory.OrderByExpressionNotAggregatedOrGrouped(columnRefBinding));
        //}

        //public static void ReportOrderByInvalidInSubqueryUnlessTopIsAlsoSpecified(this ICollection<Diagnostic> diagnostics)
        //{
        //    diagnostics.Add(DiagnosticFactory.OrderByInvalidInSubqueryUnlessTopIsAlsoSpecified());
        //}

        //public static void ReportInvalidDataTypeInSelectDistinct(this ICollection<Diagnostic> diagnostics, Type expressionType)
        //{
        //    diagnostics.Add(DiagnosticFactory.InvalidDataTypeInSelectDistinct(expressionType));
        //}

        //public static void ReportInvalidDataTypeInGroupBy(this ICollection<Diagnostic> diagnostics, Type expressionType)
        //{
        //    diagnostics.Add(DiagnosticFactory.InvalidDataTypeInGroupBy(expressionType));
        //}

        //public static void ReportInvalidDataTypeInOrderBy(this ICollection<Diagnostic> diagnostics, Type expressionType)
        //{
        //    diagnostics.Add(DiagnosticFactory.InvalidDataTypeInOrderBy(expressionType));
        //}

        //public static void ReportInvalidDataTypeInUnion(this ICollection<Diagnostic> diagnostics, Type expressionType, BinaryQueryOperator unionOperator)
        //{
        //    diagnostics.Add(DiagnosticFactory.InvalidDataTypeInUnion(expressionType, unionOperator));
        //}

        //public static void ReportDifferentExpressionCountInBinaryQuery(this ICollection<Diagnostic> diagnostics)
        //{
        //    diagnostics.Add(DiagnosticFactory.DifferentExpressionCountInBinaryQuery());
        //}

        //public static void ReportOrderByItemsMustBeInSelectListIfUnionSpecified(this ICollection<Diagnostic> diagnostics)
        //{
        //    diagnostics.Add(DiagnosticFactory.OrderByItemsMustBeInSelectListIfUnionSpecified());
        //}

        //public static void ReportOrderByItemsMustBeInSelectListIfDistinctSpecified(this ICollection<Diagnostic> diagnostics)
        //{
        //    diagnostics.Add(DiagnosticFactory.OrderByItemsMustBeInSelectListIfDistinctSpecified());
        //}

        //public static void ReportGroupByItemDoesNotReferenceAnyColumns(this ICollection<Diagnostic> diagnostics)
        //{
        //    diagnostics.Add(DiagnosticFactory.GroupByItemDoesNotReferenceAnyColumns());
        //}

        //public static void ReportConstantExpressionInOrderBy(this ICollection<Diagnostic> diagnostics)
        //{
        //    diagnostics.Add(DiagnosticFactory.ConstantExpressionInOrderBy());
        //}

        //public static void ReportTooManyExpressionsInSelectListOfSubquery(this ICollection<Diagnostic> diagnostics)
        //{
        //    diagnostics.Add(DiagnosticFactory.TooManyExpressionsInSelectListOfSubquery());
        //}

        //public static void ReportInvalidRowReference(this ICollection<Diagnostic> diagnostics, SyntaxNodeOrToken nodeOrToken, TableRefBinding derivedTableRef)
        //{
        //    diagnostics.Add(DiagnosticFactory.InvalidRowReference(nodeOrToken, derivedTableRef));
        //}

        //public static void ReportNoColumnAliasSpecified(this ICollection<Diagnostic> diagnostics, SyntaxNodeOrToken nodeOrToken, int columnIndex, Identifier derivedTableName)
        //{
        //    diagnostics.Add(DiagnosticFactory.NoColumnAliasSpecified(nodeOrToken, columnIndex, derivedTableName));
        //}

        //public static void ReportCteHasMoreColumnsThanSpecified(this ICollection<Diagnostic> diagnostics, Identifier cteTableName)
        //{
        //    diagnostics.Add(DiagnosticFactory.CteHasMoreColumnsThanSpecified(cteTableName));
        //}

        //public static void ReportCteHasFewerColumnsThanSpecified(this ICollection<Diagnostic> diagnostics, Identifier cteTableName)
        //{
        //    diagnostics.Add(DiagnosticFactory.CteHasFewerColumnsThanSpecified(cteTableName));
        //}

        //public static void ReportCteHasDuplicateColumnName(this ICollection<Diagnostic> diagnostics, Identifier columnName, Identifier cteTableName)
        //{
        //    diagnostics.Add(DiagnosticFactory.CteHasDuplicateColumnName(columnName, cteTableName));
        //}

        //public static void ReportCteHasDuplicateTableName(this ICollection<Diagnostic> diagnostics, Identifier cteTableName)
        //{
        //    diagnostics.Add(DiagnosticFactory.CteHasDuplicateTableName(cteTableName));
        //}

        //public static void ReportCteDoesNotHaveUnionAll(this ICollection<Diagnostic> diagnostics, Identifier cteTableName)
        //{
        //    diagnostics.Add(DiagnosticFactory.CteDoesNotHaveUnionAll(cteTableName));
        //}

        //public static void ReportCteDoesNotHaveAnchorMember(this ICollection<Diagnostic> diagnostics, Identifier cteTableName)
        //{
        //    diagnostics.Add(DiagnosticFactory.CteDoesNotHaveAnchorMember(cteTableName));
        //}

        //public static void ReportCteContainsRecursiveReferenceInSubquery(this ICollection<Diagnostic> diagnostics, Identifier cteTableName)
        //{
        //    diagnostics.Add(DiagnosticFactory.CteContainsRecursiveReferenceInSubquery(cteTableName));
        //}

        //public static void ReportCteContainsUnexpectedAnchorMember(this ICollection<Diagnostic> diagnostics, Identifier cteTableName)
        //{
        //    diagnostics.Add(DiagnosticFactory.CteContainsUnexpectedAnchorMember(cteTableName));
        //}

        //public static void ReportCteContainsMultipleRecursiveReferences(this ICollection<Diagnostic> diagnostics, Identifier cteTableName)
        //{
        //    diagnostics.Add(DiagnosticFactory.CteContainsMultipleRecursiveReferences(cteTableName));
        //}

        //public static void ReportCteContainsUnion(this ICollection<Diagnostic> diagnostics, Identifier cteTableName)
        //{
        //    diagnostics.Add(DiagnosticFactory.CteContainsUnion(cteTableName));
        //}

        //public static void ReportCteContainsDistinct(this ICollection<Diagnostic> diagnostics, Identifier cteTableName)
        //{
        //    diagnostics.Add(DiagnosticFactory.CteContainsDistinct(cteTableName));
        //}

        //public static void ReportCteContainsTop(this ICollection<Diagnostic> diagnostics, Identifier cteTableName)
        //{
        //    diagnostics.Add(DiagnosticFactory.CteContainsTop(cteTableName));
        //}

        //public static void ReportCteContainsOuterJoin(this ICollection<Diagnostic> diagnostics, Identifier cteTableName)
        //{
        //    diagnostics.Add(DiagnosticFactory.CteContainsOuterJoin(cteTableName));
        //}

        //public static void ReportCteContainsGroupByHavingOrAggregate(this ICollection<Diagnostic> diagnostics, Identifier cteTableName)
        //{
        //    diagnostics.Add(DiagnosticFactory.CteContainsGroupByHavingOrAggregate(cteTableName));
        //}

        //public static void ReportCteHasTypeMismatchBetweenAnchorAndRecursivePart(this ICollection<Diagnostic> diagnostics, Identifier columnName, Identifier cteTableName)
        //{
        //    diagnostics.Add(DiagnosticFactory.CteHasTypeMismatchBetweenAnchorAndRecursivePart(columnName, cteTableName));
        //}

        #endregion
    }
}