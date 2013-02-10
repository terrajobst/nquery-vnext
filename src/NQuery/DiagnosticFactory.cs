using System;

namespace NQuery
{
    internal static class DiagnosticFactory
    {
        #region Parser Errors

        //public static Diagnostic InvalidTypeReference(SyntaxNodeOrToken nodeOrToken, string tokenText)
        //{
        //    var message = String.Format(CultureInfo.CurrentCulture, Resources.InvalidTypeReference, tokenText);
        //    return new Diagnostic(nodeOrToken, DiagnosticId.InvalidTypeReference, message);
        //}

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

        #endregion

        #region Resolving/Evaluation Errors

        //public static Diagnostic AmbiguousTableRef(SyntaxNodeOrToken nodeOrToken, Identifier identifier, TableRefBinding[] candidates)
        //{
        //    var message = String.Format(CultureInfo.CurrentCulture, Resources.AmbiguousTableRef, identifier, FormattingHelpers.FormatBindingList(candidates));
        //    return new Diagnostic(nodeOrToken, DiagnosticId.AmbiguousTableRef, message);
        //}

        //public static Diagnostic AmbiguousType(string typeReference, Type[] candidates)
        //{
        //    var message = String.Format(CultureInfo.CurrentCulture, Resources.AmbiguousType, typeReference, FormattingHelpers.FormatFullyQualifiedTypeList(candidates));
        //    return new Diagnostic(DiagnosticId.AmbiguousType, message);
        //}

        //public static Diagnostic AsteriskModifierNotAllowed(SyntaxNodeOrToken nodeOrToken, ExpressionNode functionInvocation)
        //{
        //    var message = String.Format(CultureInfo.CurrentCulture, Resources.AsteriskModifierNotAllowed, functionInvocation.GenerateSource());
        //    return new Diagnostic(nodeOrToken, DiagnosticId.AsteriskModifierNotAllowed, message);
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

        #endregion

        #region Query Errors

        //public static Diagnostic AggregateDoesNotSupportType(AggregateBinding aggregateBinding, Type argumentType)
        //{
        //    var message = String.Format(CultureInfo.CurrentCulture, Resources.AggregateDoesNotSupportType, aggregateBinding.Name, FormattingHelpers.FormatType(argumentType));
        //    return new Diagnostic(DiagnosticId.AggregateDoesNotSupportType, message);
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

        //public static Diagnostic OrderByItemsMustBeInSelectListIfDistinctSpecified()
        //{
        //    return new Diagnostic(DiagnosticId.OrderByItemsMustBeInSelectListIfDistinctSpecified, Resources.OrderByItemsMustBeInSelectListIfDistinctSpecified);
        //}

        //public static Diagnostic GroupByItemDoesNotReferenceAnyColumns()
        //{
        //    return new Diagnostic(DiagnosticId.GroupByItemDoesNotReferenceAnyColumns, Resources.GroupByItemDoesNotReferenceAnyColumns);
        //}

        //public static Diagnostic TooManyExpressionsInSelectListOfSubquery()
        //{
        //    return new Diagnostic(DiagnosticId.TooManyExpressionsInSelectListOfSubquery, Resources.TooManyExpressionsInSelectListOfSubquery);
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