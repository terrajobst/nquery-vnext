using System.Globalization;

using NQuery.CodeAnalysis;
using NQuery.CodeAnalysis.Binding;
using NQuery.CodeAnalysis.Symbols;
using NQuery.CodeAnalysis.Syntax;
using NQuery.CodeAnalysis.Text;

namespace NQuery.CodeAnalysis;

internal static class DiagnosticExtensions
{
    extension(DiagnosticId diagnosticId)
    {
        public string GetMessage()
        {
            switch (diagnosticId)
            {
                case DiagnosticId.IllegalInputCharacter:
                    return Resources.IllegalInputCharacter;
                case DiagnosticId.UnterminatedComment:
                    return Resources.UnterminatedComment;
                case DiagnosticId.UnterminatedString:
                    return Resources.UnterminatedString;
                case DiagnosticId.UnterminatedQuotedIdentifier:
                    return Resources.UnterminatedQuotedIdentifier;
                case DiagnosticId.UnterminatedParenthesizedIdentifier:
                    return Resources.UnterminatedParenthesizedIdentifier;
                case DiagnosticId.EmptyQuotedIdentifier:
                    return Resources.EmptyQuotedIdentifier;
                case DiagnosticId.EmptyParenthesizedIdentifier:
                    return Resources.EmptyParenthesizedIdentifier;
                case DiagnosticId.UnterminatedDate:
                    return Resources.UnterminatedDate;
                case DiagnosticId.InvalidDate:
                    return Resources.InvalidDate;
                case DiagnosticId.InvalidInteger:
                    return Resources.InvalidInteger;
                case DiagnosticId.InvalidReal:
                    return Resources.InvalidReal;
                case DiagnosticId.NumberTooLarge:
                    return Resources.NumberTooLarge;
                case DiagnosticId.TokenExpected:
                    return Resources.TokenExpected;
                case DiagnosticId.InvalidOperatorForAllAny:
                    return Resources.InvalidOperatorForAllAny;
                case DiagnosticId.UndeclaredTable:
                    return Resources.UndeclaredTable;
                case DiagnosticId.UndeclaredTableInstance:
                    return Resources.UndeclaredTableInstance;
                case DiagnosticId.UndeclaredVariable:
                    return Resources.UndeclaredVariable;
                case DiagnosticId.UndeclaredFunction:
                    return Resources.UndeclaredFunction;
                case DiagnosticId.UndeclaredAggregate:
                    return Resources.UndeclaredAggregate;
                case DiagnosticId.UndeclaredMethod:
                    return Resources.UndeclaredMethod;
                case DiagnosticId.UndeclaredColumn:
                    return Resources.UndeclaredColumn;
                case DiagnosticId.UndeclaredProperty:
                    return Resources.UndeclaredProperty;
                case DiagnosticId.UndeclaredType:
                    return Resources.UndeclaredType;
                case DiagnosticId.ColumnTableOrVariableNotDeclared:
                    return Resources.ColumnTableOrVariableNotDeclared;
                case DiagnosticId.AmbiguousReference:
                    return Resources.AmbiguousReference;
                case DiagnosticId.AmbiguousColumnRef:
                    return Resources.AmbiguousColumnRef;
                case DiagnosticId.AmbiguousTable:
                    return Resources.AmbiguousTable;
                case DiagnosticId.AmbiguousVariable:
                    return Resources.AmbiguousVariable;
                case DiagnosticId.AmbiguousAggregate:
                    return Resources.AmbiguousAggregate;
                case DiagnosticId.AmbiguousProperty:
                    return Resources.AmbiguousProperty;
                case DiagnosticId.AmbiguousInvocation:
                    return Resources.AmbiguousInvocation;
                case DiagnosticId.InvocationRequiresParenthesis:
                    return Resources.InvocationRequiresParenthesis;
                case DiagnosticId.CannotApplyUnaryOperator:
                    return Resources.CannotApplyUnaryOperator;
                case DiagnosticId.AmbiguousUnaryOperator:
                    return Resources.AmbiguousUnaryOperator;
                case DiagnosticId.CannotApplyBinaryOperator:
                    return Resources.CannotApplyBinaryOperator;
                case DiagnosticId.AmbiguousBinaryOperator:
                    return Resources.AmbiguousBinaryOperator;
                case DiagnosticId.AmbiguousConversion:
                    return Resources.AmbiguousConversion;
                case DiagnosticId.WhenMustEvaluateToBool:
                    return Resources.WhenMustEvaluateToBool;
                case DiagnosticId.CannotConvert:
                    return Resources.CannotConvert;
                case DiagnosticId.MustSpecifyTableToSelectFrom:
                    return Resources.MustSpecifyTableToSelectFrom;
                case DiagnosticId.AggregateCannotContainAggregate:
                    return Resources.AggregateCannotContainAggregate;
                case DiagnosticId.AggregateCannotContainSubquery:
                    return Resources.AggregateCannotContainSubquery;
                case DiagnosticId.GroupByCannotContainSubquery:
                    return Resources.GroupByCannotContainSubquery;
                case DiagnosticId.AggregateDoesNotSupportType:
                    return Resources.AggregateDoesNotSupportType;
                case DiagnosticId.AggregateInWhere:
                    return Resources.AggregateInWhere;
                case DiagnosticId.AggregateInOn:
                    return Resources.AggregateInOn;
                case DiagnosticId.AggregateInGroupBy:
                    return Resources.AggregateInGroupBy;
                case DiagnosticId.AggregateContainsColumnsFromDifferentQueries:
                    return Resources.AggregateContainsColumnsFromDifferentQueries;
                case DiagnosticId.AggregateInvalidInCurrentContext:
                    return Resources.AggregateInvalidInCurrentContext;
                case DiagnosticId.DuplicateTableRefInFrom:
                    return Resources.DuplicateTableRefInFrom;
                case DiagnosticId.TopWithTiesRequiresOrderBy:
                    return Resources.TopWithTiesRequiresOrderBy;
                case DiagnosticId.OrderByColumnPositionIsOutOfRange:
                    return Resources.OrderByColumnPositionIsOutOfRange;
                case DiagnosticId.WhereClauseMustEvaluateToBool:
                    return Resources.WhereClauseMustEvaluateToBool;
                case DiagnosticId.OnClauseMustEvaluateToBool:
                    return Resources.OnClauseMustEvaluateToBool;
                case DiagnosticId.HavingClauseMustEvaluateToBool:
                    return Resources.HavingClauseMustEvaluateToBool;
                case DiagnosticId.SelectExpressionNotAggregatedAndNoGroupBy:
                    return Resources.SelectExpressionNotAggregatedAndNoGroupBy;
                case DiagnosticId.SelectExpressionNotAggregatedOrGrouped:
                    return Resources.SelectExpressionNotAggregatedOrGrouped;
                case DiagnosticId.HavingExpressionNotAggregatedOrGrouped:
                    return Resources.HavingExpressionNotAggregatedOrGrouped;
                case DiagnosticId.OrderByExpressionNotAggregatedAndNoGroupBy:
                    return Resources.OrderByExpressionNotAggregatedAndNoGroupBy;
                case DiagnosticId.OrderByExpressionNotAggregatedOrGrouped:
                    return Resources.OrderByExpressionNotAggregatedOrGrouped;
                case DiagnosticId.OrderByInvalidInSubqueryUnlessTopIsAlsoSpecified:
                    return Resources.OrderByInvalidInSubqueryUnlessTopIsAlsoSpecified;
                case DiagnosticId.InvalidDataTypeInSelectDistinct:
                    return Resources.InvalidDataTypeInSelectDistinct;
                case DiagnosticId.InvalidDataTypeInGroupBy:
                    return Resources.InvalidDataTypeInGroupBy;
                case DiagnosticId.InvalidDataTypeInOrderBy:
                    return Resources.InvalidDataTypeInOrderBy;
                case DiagnosticId.InvalidDataTypeInUnion:
                    return Resources.InvalidDataTypeInUnion;
                case DiagnosticId.InvalidDataTypeInExcept:
                    return Resources.InvalidDataTypeInExcept;
                case DiagnosticId.InvalidDataTypeInIntersect:
                    return Resources.InvalidDataTypeInIntersect;
                case DiagnosticId.DifferentExpressionCountInBinaryQuery:
                    return Resources.DifferentExpressionCountInBinaryQuery;
                case DiagnosticId.OrderByItemsMustBeInSelectListIfUnionSpecified:
                    return Resources.OrderByItemsMustBeInSelectListIfUnionSpecified;
                case DiagnosticId.OrderByItemsMustBeInSelectListIfDistinctSpecified:
                    return Resources.OrderByItemsMustBeInSelectListIfDistinctSpecified;
                case DiagnosticId.GroupByItemDoesNotReferenceAnyColumns:
                    return Resources.GroupByItemDoesNotReferenceAnyColumns;
                case DiagnosticId.ConstantExpressionInOrderBy:
                    return Resources.ConstantExpressionInOrderBy;
                case DiagnosticId.TooManyExpressionsInSelectListOfSubquery:
                    return Resources.TooManyExpressionsInSelectListOfSubquery;
                case DiagnosticId.InvalidRowReference:
                    return Resources.InvalidRowReference;
                case DiagnosticId.NoColumnAliasSpecified:
                    return Resources.NoColumnAliasSpecified;
                case DiagnosticId.CteHasMoreColumnsThanSpecified:
                    return Resources.CteHasMoreColumnsThanSpecified;
                case DiagnosticId.CteHasFewerColumnsThanSpecified:
                    return Resources.CteHasFewerColumnsThanSpecified;
                case DiagnosticId.CteHasDuplicateColumnName:
                    return Resources.CteHasDuplicateColumnName;
                case DiagnosticId.CteHasDuplicateTableName:
                    return Resources.CteHasDuplicateTableName;
                case DiagnosticId.CteDoesNotHaveUnionAll:
                    return Resources.CteDoesNotHaveUnionAll;
                case DiagnosticId.CteDoesNotHaveAnchorMember:
                    return Resources.CteDoesNotHaveAnchorMember;
                case DiagnosticId.CteContainsRecursiveReferenceInSubquery:
                    return Resources.CteContainsRecursiveReferenceInSubquery;
                case DiagnosticId.CteContainsUnexpectedAnchorMember:
                    return Resources.CteContainsUnexpectedAnchorMember;
                case DiagnosticId.CteContainsMultipleRecursiveReferences:
                    return Resources.CteContainsMultipleRecursiveReferences;
                case DiagnosticId.CteContainsUnion:
                    return Resources.CteContainsUnion;
                case DiagnosticId.CteContainsDistinct:
                    return Resources.CteContainsDistinct;
                case DiagnosticId.CteContainsTop:
                    return Resources.CteContainsTop;
                case DiagnosticId.CteContainsOuterJoin:
                    return Resources.CteContainsOuterJoin;
                case DiagnosticId.CteContainsGroupByHavingOrAggregate:
                    return Resources.CteContainsGroupByHavingOrAggregate;
                case DiagnosticId.CteHasTypeMismatchBetweenAnchorAndRecursivePart:
                    return Resources.CteHasTypeMismatchBetweenAnchorAndRecursivePart;
                default:
                    throw ExceptionBuilder.UnexpectedValue(diagnosticId);
            }
        }
    }

    extension(ICollection<Diagnostic> diagnostics)
    {
        public void Report(TextSpan textSpan, DiagnosticId diagnosticId, params object[] args)
        {
            var diagnostic = Diagnostic.Format(textSpan, diagnosticId, args);
            diagnostics.Add(diagnostic);
        }

        public void ReportIllegalInputCharacter(TextSpan textSpan, char character)
        {
            diagnostics.Report(textSpan, DiagnosticId.IllegalInputCharacter, character);
        }

        public void ReportUnterminatedComment(TextSpan textSpan)
        {
            diagnostics.Report(textSpan, DiagnosticId.UnterminatedComment);
        }

        public void ReportUnterminatedString(TextSpan textSpan)
        {
            diagnostics.Report(textSpan, DiagnosticId.UnterminatedString);
        }

        public void ReportUnterminatedQuotedIdentifier(TextSpan textSpan)
        {
            diagnostics.Report(textSpan, DiagnosticId.UnterminatedQuotedIdentifier);
        }

        public void ReportUnterminatedParenthesizedIdentifier(TextSpan textSpan)
        {
            diagnostics.Report(textSpan, DiagnosticId.UnterminatedParenthesizedIdentifier);
        }

        public void ReportEmptyQuotedIdentifier(TextSpan textSpan)
        {
            diagnostics.Report(textSpan, DiagnosticId.EmptyQuotedIdentifier);
        }

        public void ReportEmptyParenthesizedIdentifier(TextSpan textSpan)
        {
            diagnostics.Report(textSpan, DiagnosticId.EmptyParenthesizedIdentifier);
        }

        public void ReportUnterminatedDate(TextSpan textSpan)
        {
            diagnostics.Report(textSpan, DiagnosticId.UnterminatedDate);
        }

        public void ReportInvalidDate(TextSpan textSpan, string tokenText)
        {
            diagnostics.Report(textSpan, DiagnosticId.InvalidDate, tokenText);
        }

        public void ReportInvalidInteger(TextSpan textSpan, string tokenText)
        {
            diagnostics.Report(textSpan, DiagnosticId.InvalidInteger, tokenText);
        }

        public void ReportInvalidReal(TextSpan textSpan, string tokenText)
        {
            diagnostics.Report(textSpan, DiagnosticId.InvalidReal, tokenText);
        }

        public void ReportNumberTooLarge(TextSpan textSpan, string tokenText)
        {
            diagnostics.Report(textSpan, DiagnosticId.NumberTooLarge, tokenText);
        }

        public void ReportTokenExpected(TextSpan span, SyntaxToken actual, SyntaxKind expected)
        {
            var actualText = actual.GetDisplayText();
            var expectedText = expected.GetDisplayText();
            diagnostics.Report(span, DiagnosticId.TokenExpected, actualText, expectedText);
        }

        public void ReportUndeclaredTable(NamedTableReferenceSyntax namedTableReference)
        {
            var tableName = namedTableReference.TableName;
            diagnostics.Report(tableName.Span, DiagnosticId.UndeclaredTable, tableName.ValueText);
        }

        public void ReportUndeclaredTableInstance(SyntaxToken name)
        {
            diagnostics.Report(name.Span, DiagnosticId.UndeclaredTableInstance, name.ValueText);
        }

        public void ReportUndeclaredVariable(VariableExpressionSyntax node)
        {
            var variableName = node.Name;
            diagnostics.Report(variableName.Span, DiagnosticId.UndeclaredVariable, variableName.ValueText);
        }

        public void ReportUndeclaredFunction(FunctionInvocationExpressionSyntax node, IEnumerable<Type> argumentTypes)
        {
            var name = node.Name.ValueText;
            var argumentTypeList = string.Join(@", ", argumentTypes.Select(t => t.ToDisplayName()));
            diagnostics.Report(node.Span, DiagnosticId.UndeclaredFunction, name, argumentTypeList);
        }

        public void ReportUndeclaredAggregate(SyntaxToken name)
        {
            var nameText = name.ValueText;
            diagnostics.Report(name.Span, DiagnosticId.UndeclaredAggregate, nameText);
        }

        public void ReportUndeclaredMethod(MethodInvocationExpressionSyntax node, Type declaringType, IEnumerable<Type> argumentTypes)
        {
            var name = node.Name.ValueText;
            var declaringTypeName = declaringType.ToDisplayName();
            var argumentTypeNames = string.Join(@", ", argumentTypes.Select(t => t.ToDisplayName()));
            diagnostics.Report(node.Span, DiagnosticId.UndeclaredMethod, declaringTypeName, name, argumentTypeNames);
        }

        public void ReportUndeclaredColumn(PropertyAccessExpressionSyntax node, TableInstanceSymbol tableInstance)
        {
            var tableName = tableInstance.Name;
            var columnName = node.Name.ValueText;
            diagnostics.Report(node.Span, DiagnosticId.UndeclaredColumn, tableName, columnName);
        }

        public void ReportUndeclaredProperty(PropertyAccessExpressionSyntax node, Type type)
        {
            var typeName = type.ToDisplayName();
            var propertyName = node.Name.ValueText;
            diagnostics.Report(node.Span, DiagnosticId.UndeclaredProperty, typeName, propertyName);
        }

        public void ReportUndeclaredType(SyntaxToken typeName)
        {
            diagnostics.Report(typeName.Span, DiagnosticId.UndeclaredType, typeName.ValueText);
        }

        public void ReportColumnTableOrVariableNotDeclared(SyntaxToken name)
        {
            diagnostics.Report(name.Span, DiagnosticId.ColumnTableOrVariableNotDeclared, name.ValueText);
        }

        public void ReportAmbiguousName(SyntaxToken name, IReadOnlyList<Symbol> candidates)
        {
            var symbol1 = candidates[0];
            var symbol2 = candidates[1];
            diagnostics.Report(name.Span, DiagnosticId.AmbiguousReference, name.ValueText, symbol1, symbol2);
        }

        public void ReportAmbiguousColumnInstance(SyntaxToken name, IReadOnlyList<ColumnInstanceSymbol> candidates)
        {
            var symbol1 = candidates[0];
            var symbol2 = candidates[1];
            diagnostics.Report(name.Span, DiagnosticId.AmbiguousColumnRef, name.ValueText, symbol1, symbol2);
        }

        public void ReportAmbiguousTable(SyntaxToken name, IReadOnlyList<TableSymbol> candidates)
        {
            var symbol1 = candidates[0];
            var symbol2 = candidates[1];
            diagnostics.Report(name.Span, DiagnosticId.AmbiguousTable, name.ValueText, symbol1, symbol2);
        }

        public void ReportAmbiguousVariable(SyntaxToken name)
        {
            diagnostics.Report(name.Span, DiagnosticId.AmbiguousVariable, name.ValueText);
        }

        public void ReportAmbiguousAggregate(SyntaxToken name, IReadOnlyList<AggregateSymbol> symbols)
        {
            var symbol1 = symbols[0];
            var symbol2 = symbols[1];
            diagnostics.Report(name.Span, DiagnosticId.AmbiguousAggregate, name.ValueText, symbol1, symbol2);
        }

        public void ReportAmbiguousProperty(SyntaxToken name)
        {
            diagnostics.Report(name.Span, DiagnosticId.AmbiguousProperty, name.ValueText);
        }

        public void ReportAmbiguousInvocation(TextSpan span, IInvocableSymbol symbol1, IInvocableSymbol symbol2, IReadOnlyList<Type> argumentTypes)
        {
            if (argumentTypes.Count > 0)
            {
                var displayTypes = string.Join(@", ", argumentTypes.Select(t => t.ToDisplayName()));
                diagnostics.Report(span, DiagnosticId.AmbiguousInvocation, symbol1, symbol2, displayTypes);
            }
            else
            {
                var message = string.Format(CultureInfo.CurrentCulture, Resources.AmbiguousInvocationNoArgs, symbol1, symbol2);
                var diagnostic = new Diagnostic(span, DiagnosticId.AmbiguousInvocation, message);
                diagnostics.Add(diagnostic);
            }
        }

        public void ReportInvocationRequiresParenthesis(SyntaxToken name)
        {
            diagnostics.Report(name.Span, DiagnosticId.InvocationRequiresParenthesis, name.ValueText);
        }

        public void ReportCannotApplyUnaryOperator(TextSpan span, UnaryOperatorKind operatorKind, Type type)
        {
            var operatorName = operatorKind.ToDisplayName();
            var argumentTypeName = type.ToDisplayName();
            diagnostics.Report(span, DiagnosticId.CannotApplyUnaryOperator, operatorName, argumentTypeName);
        }

        public void ReportAmbiguousUnaryOperator(TextSpan span, UnaryOperatorKind operatorKind, Type type)
        {
            var operatorName = operatorKind.ToDisplayName();
            var argumentTypeName = type.ToDisplayName();
            diagnostics.Report(span, DiagnosticId.AmbiguousUnaryOperator, operatorName, argumentTypeName);
        }

        public void ReportCannotApplyBinaryOperator(TextSpan span, BinaryOperatorKind operatorKind, Type leftType, Type rightType)
        {
            var operatorName = operatorKind.ToDisplayName();
            var leftTypeName = leftType.ToDisplayName();
            var rightTypeName = rightType.ToDisplayName();
            diagnostics.Report(span, DiagnosticId.CannotApplyBinaryOperator, operatorName, leftTypeName, rightTypeName);
        }

        public void ReportAmbiguousBinaryOperator(TextSpan span, BinaryOperatorKind operatorKind, Type leftType, Type rightType)
        {
            var operatorName = operatorKind.ToDisplayName();
            var leftTypeName = leftType.ToDisplayName();
            var rightTypeName = rightType.ToDisplayName();
            diagnostics.Report(span, DiagnosticId.AmbiguousBinaryOperator, operatorName, leftTypeName, rightTypeName);
        }

        public void ReportAmbiguousConversion(TextSpan span, Type sourceType, Type targetType)
        {
            var sourceTypeName = sourceType.ToDisplayName();
            var targetTypeName = targetType.ToDisplayName();
            diagnostics.Report(span, DiagnosticId.AmbiguousConversion, sourceTypeName, targetTypeName);
        }

        public void ReportCannotConvert(TextSpan span, Type sourceType, Type targetType)
        {
            var sourceTypeName = sourceType.ToDisplayName();
            var targetTypeName = targetType.ToDisplayName();
            diagnostics.Report(span, DiagnosticId.CannotConvert, sourceTypeName, targetTypeName);
        }

        public void ReportWhenMustEvaluateToBool(TextSpan span)
        {
            var typeName = typeof(bool).ToDisplayName();
            diagnostics.Report(span, DiagnosticId.WhenMustEvaluateToBool, typeName);
        }

        public void ReportMustSpecifyTableToSelectFrom(TextSpan textSpan)
        {
            diagnostics.Report(textSpan, DiagnosticId.MustSpecifyTableToSelectFrom);
        }

        public void ReportAggregateInAggregateArgument(TextSpan textSpan)
        {
            diagnostics.Report(textSpan, DiagnosticId.AggregateCannotContainAggregate);
        }

        public void ReportAggregateCannotContainSubquery(TextSpan textSpan)
        {
            diagnostics.Report(textSpan, DiagnosticId.AggregateCannotContainSubquery);
        }

        public void ReportGroupByCannotContainSubquery(TextSpan textSpan)
        {
            diagnostics.Report(textSpan, DiagnosticId.GroupByCannotContainSubquery);
        }

        public void ReportAggregateDoesNotSupportType(TextSpan textSpan, AggregateSymbol aggregate, Type argumentType)
        {
            diagnostics.Report(textSpan, DiagnosticId.AggregateDoesNotSupportType, aggregate.Name, argumentType.ToDisplayName());
        }

        public void ReportAggregateInWhere(TextSpan textSpan)
        {
            diagnostics.Report(textSpan, DiagnosticId.AggregateInWhere);
        }

        public void ReportAggregateInOn(TextSpan textSpan)
        {
            diagnostics.Report(textSpan, DiagnosticId.AggregateInOn);
        }

        public void ReportAggregateInGroupBy(TextSpan textSpan)
        {
            diagnostics.Report(textSpan, DiagnosticId.AggregateInGroupBy);
        }

        public void ReportAggregateContainsColumnsFromDifferentQueries(TextSpan textSpan)
        {
            diagnostics.Report(textSpan, DiagnosticId.AggregateContainsColumnsFromDifferentQueries);
        }

        public void ReportAggregateInvalidInCurrentContext(TextSpan textSpan)
        {
            diagnostics.Report(textSpan, DiagnosticId.AggregateInvalidInCurrentContext);
        }

        public void ReportDuplicateTableRefInFrom(SyntaxToken identifier)
        {
            diagnostics.Report(identifier.Span, DiagnosticId.DuplicateTableRefInFrom, identifier.GetDisplayText());
        }

        public void ReportTopWithTiesRequiresOrderBy(TextSpan textSpan)
        {
            diagnostics.Report(textSpan, DiagnosticId.TopWithTiesRequiresOrderBy);
        }

        public void ReportOrderByColumnPositionIsOutOfRange(TextSpan textSpan, int position, int numberOfColumns)
        {
            diagnostics.Report(textSpan, DiagnosticId.OrderByColumnPositionIsOutOfRange, position, numberOfColumns);
        }

        public void ReportWhereClauseMustEvaluateToBool(TextSpan span)
        {
            diagnostics.Report(span, DiagnosticId.WhereClauseMustEvaluateToBool);
        }

        public void ReportOnClauseMustEvaluateToBool(TextSpan span)
        {
            diagnostics.Report(span, DiagnosticId.OnClauseMustEvaluateToBool);
        }

        public void ReportHavingClauseMustEvaluateToBool(TextSpan span)
        {
            diagnostics.Report(span, DiagnosticId.HavingClauseMustEvaluateToBool);
        }

        //public static void ReportOrderByInvalidInSubqueryUnlessTopIsAlsoSpecified(this ICollection<Diagnostic> diagnostics)
        //{
        //    var diagnostic = new Diagnostic(DiagnosticId.OrderByInvalidInSubqueryUnlessTopIsAlsoSpecified, Resources.OrderByInvalidInSubqueryUnlessTopIsAlsoSpecified);
        //    diagnostics.Add(diagnostic);
        //}

        public void ReportDifferentExpressionCountInBinaryQuery(TextSpan textSpan)
        {
            diagnostics.Report(textSpan, DiagnosticId.DifferentExpressionCountInBinaryQuery);
        }

        public void ReportOrderByItemsMustBeInSelectListIfUnionSpecified(TextSpan textSpan)
        {
            diagnostics.Report(textSpan, DiagnosticId.OrderByItemsMustBeInSelectListIfUnionSpecified);
        }

        public void ReportOrderByItemsMustBeInSelectListIfDistinctSpecified(TextSpan textSpan)
        {
            diagnostics.Report(textSpan, DiagnosticId.OrderByItemsMustBeInSelectListIfDistinctSpecified);
        }

        //public static void ReportGroupByItemDoesNotReferenceAnyColumns(this ICollection<Diagnostic> diagnostics)
        //{
        //    var diagnostic = new Diagnostic(DiagnosticId.GroupByItemDoesNotReferenceAnyColumns, Resources.GroupByItemDoesNotReferenceAnyColumns);
        //    diagnostics.Add(diagnostic);
        //}

        public void ReportConstantExpressionInOrderBy(TextSpan textSpan)
        {
            diagnostics.Report(textSpan, DiagnosticId.ConstantExpressionInOrderBy);
        }

        public void ReportTooManyExpressionsInSelectListOfSubquery(TextSpan textSpan)
        {
            diagnostics.Report(textSpan, DiagnosticId.TooManyExpressionsInSelectListOfSubquery);
        }

        public void ReportInvalidRowReference(SyntaxToken tableName)
        {
            diagnostics.Report(tableName.Span, DiagnosticId.InvalidRowReference, tableName.ValueText);
        }

        public void ReportNoColumnAliasSpecified(SyntaxToken tableName, int columnIndex)
        {
            diagnostics.Report(tableName.Span, DiagnosticId.NoColumnAliasSpecified, columnIndex + 1, tableName.ValueText);
        }

        public void ReportCteHasMoreColumnsThanSpecified(SyntaxToken cteTableName)
        {
            diagnostics.Report(cteTableName.Span, DiagnosticId.CteHasMoreColumnsThanSpecified, cteTableName.ValueText);
        }

        public void ReportCteHasFewerColumnsThanSpecified(SyntaxToken cteTableName)
        {
            diagnostics.Report(cteTableName.Span, DiagnosticId.CteHasFewerColumnsThanSpecified, cteTableName.ValueText);
        }

        public void ReportCteHasDuplicateColumnName(SyntaxToken cteTableName, string columnName)
        {
            diagnostics.Report(cteTableName.Span, DiagnosticId.CteHasDuplicateColumnName, columnName, cteTableName.ValueText);
        }

        public void ReportCteHasDuplicateTableName(SyntaxToken cteTableName)
        {
            diagnostics.Report(cteTableName.Span, DiagnosticId.CteHasDuplicateTableName, cteTableName.ValueText);
        }

        public void ReportCteDoesNotHaveUnionAll(SyntaxToken cteTableName)
        {
            diagnostics.Report(cteTableName.Span, DiagnosticId.CteDoesNotHaveUnionAll, cteTableName.ValueText);
        }

        public void ReportCteDoesNotHaveAnchorMember(SyntaxToken cteTableName)
        {
            diagnostics.Report(cteTableName.Span, DiagnosticId.CteDoesNotHaveAnchorMember, cteTableName.ValueText);
        }

        public void ReportCteContainsRecursiveReferenceInSubquery(SyntaxToken cteTableName)
        {
            diagnostics.Report(cteTableName.Span, DiagnosticId.CteContainsRecursiveReferenceInSubquery, cteTableName.Text);
        }

        public void ReportCteContainsUnexpectedAnchorMember(SyntaxToken cteTableName)
        {
            diagnostics.Report(cteTableName.Span, DiagnosticId.CteContainsUnexpectedAnchorMember, cteTableName.Text);
        }

        public void ReportCteContainsMultipleRecursiveReferences(SyntaxToken cteTableName)
        {
            diagnostics.Report(cteTableName.Span, DiagnosticId.CteContainsMultipleRecursiveReferences, cteTableName.Text);
        }

        public void ReportCteContainsUnion(SyntaxToken cteTableName)
        {
            diagnostics.Report(cteTableName.Span, DiagnosticId.CteContainsUnion, cteTableName.Text);
        }

        public void ReportCteContainsDistinct(SyntaxToken cteTableName)
        {
            diagnostics.Report(cteTableName.Span, DiagnosticId.CteContainsDistinct, cteTableName.Text);
        }

        public void ReportCteContainsTop(SyntaxToken cteTableName)
        {
            diagnostics.Report(cteTableName.Span, DiagnosticId.CteContainsTop, cteTableName.Text);
        }

        public void ReportCteContainsOuterJoin(SyntaxToken cteTableName)
        {
            diagnostics.Report(cteTableName.Span, DiagnosticId.CteContainsOuterJoin, cteTableName.Text);
        }

        public void ReportCteContainsGroupByHavingOrAggregate(SyntaxToken cteTableName)
        {
            diagnostics.Report(cteTableName.Span, DiagnosticId.CteContainsGroupByHavingOrAggregate, cteTableName.Text);
        }

        public void ReportCteHasTypeMismatchBetweenAnchorAndRecursivePart(TextSpan diagnosticSpan, string anchorColumnName, string recursiveColumnName)
        {
            diagnostics.Report(diagnosticSpan, DiagnosticId.CteHasTypeMismatchBetweenAnchorAndRecursivePart, anchorColumnName, recursiveColumnName);
        }
    }

    extension(SyntaxToken operatorToken)
    {
        public SyntaxToken WithInvalidOperatorForAllAnyDiagnostics()
        {
            var operatorText = operatorToken.Kind.GetText();
            var diagnostic = Diagnostic.Format(operatorToken.Span, DiagnosticId.InvalidOperatorForAllAny, operatorText);
            return operatorToken.WithDiagnostics(new[] { diagnostic });
        }
    }
}
