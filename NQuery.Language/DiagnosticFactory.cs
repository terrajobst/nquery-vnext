using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NQuery.Language.Symbols;

namespace NQuery.Language
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

        //public static Diagnostic AmbiguousAggregate(SyntaxNodeOrToken nodeOrToken, Identifier identifier, AggregateBinding[] candidates)
        //{
        //    var message = String.Format(CultureInfo.CurrentCulture, Resources.AmbiguousAggregate, identifier, FormattingHelpers.FormatBindingList(candidates));
        //    return new Diagnostic(nodeOrToken, DiagnosticId.AmbiguousAggregate, message);
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