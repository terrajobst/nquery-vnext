using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using NQuery.Binding;
using NQuery.Symbols;
using NQuery.Syntax;

namespace NQuery
{
    internal static class DiagnosticExtensions
    {
        public static string GetMessage(this DiagnosticId diagnosticId)
        {
            switch (diagnosticId)
            {
                case DiagnosticId.InternalError:
                    return Resources.InternalError;
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
                case DiagnosticId.UnterminatedDate:
                    return Resources.UnterminatedDate;
                case DiagnosticId.InvalidDate:
                    return Resources.InvalidDate;
                case DiagnosticId.InvalidInteger:
                    return Resources.InvalidInteger;
                case DiagnosticId.InvalidReal:
                    return Resources.InvalidReal;
                case DiagnosticId.InvalidBinary:
                    return Resources.InvalidBinary;
                case DiagnosticId.InvalidOctal:
                    return Resources.InvalidOctal;
                case DiagnosticId.InvalidHex:
                    return Resources.InvalidHex;
                case DiagnosticId.InvalidTypeReference:
                    return Resources.InvalidTypeReference;
                case DiagnosticId.NumberTooLarge:
                    return Resources.NumberTooLarge;
                case DiagnosticId.TokenExpected:
                    return Resources.TokenExpected;
                case DiagnosticId.SimpleExpressionExpected:
                    return Resources.SimpleExpressionExpected;
                case DiagnosticId.TableReferenceExpected:
                    return Resources.TableReferenceExpected;
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
                case DiagnosticId.AmbiguousTableRef:
                    return Resources.AmbiguousTableRef;
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
                case DiagnosticId.AmbiguousType:
                    return Resources.AmbiguousType;
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
                case DiagnosticId.AsteriskModifierNotAllowed:
                    return Resources.AsteriskModifierNotAllowed;
                case DiagnosticId.WhenMustEvaluateToBool:
                    return Resources.WhenMustEvaluateToBool;
                case DiagnosticId.CannotLoadTypeAssembly:
                    return Resources.CannotLoadTypeAssembly;
                case DiagnosticId.CannotFoldConstants:
                    return Resources.CannotFoldConstants;
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
                case DiagnosticId.TableRefInaccessible:
                    return Resources.TableRefInaccessible;
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
                    throw new ArgumentOutOfRangeException("diagnosticId");
            }
        }

        public static void Report(this ICollection<Diagnostic> diagnostics, TextSpan textSpan, DiagnosticId diagnosticId, params object[] args)
        {
            var diagnostic = Diagnostic.Format(textSpan, diagnosticId, args);
            diagnostics.Add(diagnostic);
        }

        #region Lexer CompilationErrors

        public static void ReportIllegalInputCharacter(this ICollection<Diagnostic> diagnostics, TextSpan textSpan, char character)
        {
            diagnostics.Report(textSpan, DiagnosticId.IllegalInputCharacter, character);
        }

        public static void ReportUnterminatedComment(this ICollection<Diagnostic> diagnostics, TextSpan textSpan)
        {
            diagnostics.Report(textSpan, DiagnosticId.UnterminatedComment);
        }

        public static void ReportUnterminatedString(this ICollection<Diagnostic> diagnostics, TextSpan textSpan)
        {
            diagnostics.Report(textSpan, DiagnosticId.UnterminatedString);
        }

        public static void ReportUnterminatedQuotedIdentifier(this ICollection<Diagnostic> diagnostics, TextSpan textSpan)
        {
            diagnostics.Report(textSpan, DiagnosticId.UnterminatedQuotedIdentifier);
        }

        public static void ReportUnterminatedParenthesizedIdentifier(this ICollection<Diagnostic> diagnostics, TextSpan textSpan)
        {
            diagnostics.Report(textSpan, DiagnosticId.UnterminatedParenthesizedIdentifier);
        }

        public static void ReportUnterminatedDate(this ICollection<Diagnostic> diagnostics, TextSpan textSpan)
        {
            diagnostics.Report(textSpan, DiagnosticId.UnterminatedDate);
        }

        public static void ReportInvalidDate(this ICollection<Diagnostic> diagnostics, TextSpan textSpan, string tokenText)
        {
            diagnostics.Report(textSpan, DiagnosticId.InvalidDate, tokenText);
        }

        public static void ReportInvalidInteger(this ICollection<Diagnostic> diagnostics, TextSpan textSpan, string tokenText)
        {
            diagnostics.Report(textSpan, DiagnosticId.InvalidInteger, tokenText);
        }

        public static void ReportInvalidReal(this ICollection<Diagnostic> diagnostics, TextSpan textSpan, string tokenText)
        {
            diagnostics.Report(textSpan, DiagnosticId.InvalidReal, tokenText);
        }

        public static void ReportInvalidBinary(this ICollection<Diagnostic> diagnostics, TextSpan textSpan, string tokenText)
        {
            diagnostics.Report(textSpan, DiagnosticId.InvalidBinary, tokenText);
        }

        public static void ReportInvalidOctal(this ICollection<Diagnostic> diagnostics, TextSpan textSpan, string tokenText)
        {
            diagnostics.Report(textSpan, DiagnosticId.InvalidOctal, tokenText);
        }

        public static void ReportInvalidHex(this ICollection<Diagnostic> diagnostics, TextSpan textSpan, string tokenText)
        {
            diagnostics.Report(textSpan, DiagnosticId.InvalidHex, tokenText);
        }

        public static void ReportNumberTooLarge(this ICollection<Diagnostic> diagnostics, TextSpan textSpan, string tokenText)
        {
            diagnostics.Report(textSpan, DiagnosticId.NumberTooLarge, tokenText);
        }

        #endregion

        #region Parser Errors

        public static void ReportTokenExpected(this ICollection<Diagnostic> diagnostics, TextSpan span, SyntaxToken actual, SyntaxKind expected)
        {
            var actualText = actual.GetDisplayText();
            var expectedText = expected.GetDisplayText();
            diagnostics.Report(span, DiagnosticId.TokenExpected, actualText, expectedText);
        }

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

        public static SyntaxToken WithInvalidOperatorForAllAnyDiagnostics(this SyntaxToken operatorToken)
        {
            var operatorText = operatorToken.Kind.GetText();
            var diagnostic = Diagnostic.Format(operatorToken.Span, DiagnosticId.InvalidOperatorForAllAny, operatorText);
            return operatorToken.WithDiagnotics(new[] {diagnostic});
        }

        #endregion

        #region Resolving/Evaluation Errors

        public static void ReportUndeclaredTable(this ICollection<Diagnostic> diagnostics, NamedTableReferenceSyntax namedTableReference)
        {
            var tableName = namedTableReference.TableName;
            diagnostics.Report(tableName.Span, DiagnosticId.UndeclaredTable, tableName.ValueText);
        }

        public static void ReportUndeclaredTableInstance(this ICollection<Diagnostic> diagnostics, SyntaxToken name)
        {
            diagnostics.Report(name.Span, DiagnosticId.UndeclaredTableInstance, name.ValueText);
        }

        public static void ReportUndeclaredVariable(this ICollection<Diagnostic> diagnostics, VariableExpressionSyntax node)
        {
            var variableName = node.Name;
            diagnostics.Report(variableName.Span, DiagnosticId.UndeclaredVariable, variableName.ValueText);
        }

        public static void ReportUndeclaredFunction(this ICollection<Diagnostic> diagnostics, FunctionInvocationExpressionSyntax node, IEnumerable<Type> argumentTypes)
        {
            var name = node.Name.ValueText;
            var argumentTypeList = String.Join(", ", argumentTypes.Select(t => t.ToDisplayName()));
            diagnostics.Report(node.Span, DiagnosticId.UndeclaredFunction, name, argumentTypeList);
        }

        public static void ReportUndeclaredAggregate(this ICollection<Diagnostic> diagnostics, SyntaxToken name)
        {
            var nameText = name.ValueText;
            diagnostics.Report(name.Span, DiagnosticId.UndeclaredAggregate, nameText);
        }

        public static void ReportUndeclaredMethod(this ICollection<Diagnostic> diagnostics, MethodInvocationExpressionSyntax node, Type declaringType, IEnumerable<Type> argumentTypes)
        {
            var name = node.Name.ValueText;
            var declaringTypeName = declaringType.ToDisplayName();
            var argumentTypeNames = String.Join(", ", argumentTypes.Select(t => t.ToDisplayName()));
            diagnostics.Report(node.Span, DiagnosticId.UndeclaredMethod, declaringTypeName, name, argumentTypeNames);
        }

        public static void ReportUndeclaredColumn(this ICollection<Diagnostic> diagnostics, PropertyAccessExpressionSyntax node, TableInstanceSymbol tableInstance)
        {
            var tableName = tableInstance.Name;
            var columnName = node.Name.ValueText;
            diagnostics.Report(node.Span, DiagnosticId.UndeclaredColumn, tableName, columnName);
        }

        public static void ReportUndeclaredProperty(this ICollection<Diagnostic> diagnostics, PropertyAccessExpressionSyntax node, Type type)
        {
            var typeName = type.ToDisplayName();
            var propertyName = node.Name.ValueText;
            diagnostics.Report(node.Span, DiagnosticId.UndeclaredProperty, typeName, propertyName);
        }

        public static void ReportUndeclaredType(this ICollection<Diagnostic> diagnostics, SyntaxToken typeName)
        {
            diagnostics.Report(typeName.Span, DiagnosticId.UndeclaredType, typeName.ValueText);
        }

        public static void ReportColumnTableOrVariableNotDeclared(this ICollection<Diagnostic> diagnostics, SyntaxToken name)
        {
            diagnostics.Report(name.Span, DiagnosticId.ColumnTableOrVariableNotDeclared, name.ValueText);
        }

        public static void ReportAmbiguousName(this ICollection<Diagnostic> diagnostics, SyntaxToken name, IList<Symbol> candidates)
        {
            var symbol1 = candidates[0];
            var symbol2 = candidates[1];
            diagnostics.Report(name.Span, DiagnosticId.AmbiguousReference, name.ValueText, symbol1, symbol2);
        }

        //public static void ReportAmbiguousTableRef(this ICollection<Diagnostic> diagnostics, SyntaxNodeOrToken nodeOrToken, Identifier identifier, TableRefBinding[] candidates)
        //{
        //    diagnostics.Add(DiagnosticFactory.AmbiguousTableRef(nodeOrToken, identifier, candidates));
        //}

        public static void ReportAmbiguousColumnInstance(this ICollection<Diagnostic> diagnostics, SyntaxToken name, IList<ColumnInstanceSymbol> candidates)
        {
            var symbol1 = candidates[0];
            var symbol2 = candidates[1];
            diagnostics.Report(name.Span, DiagnosticId.AmbiguousColumnRef, name.ValueText, symbol1, symbol2);
        }

        public static void ReportAmbiguousTable(this ICollection<Diagnostic> diagnostics, SyntaxToken name, IList<TableSymbol> candidates)
        {
            var symbol1 = candidates[0];
            var symbol2 = candidates[1];
            diagnostics.Report(name.Span, DiagnosticId.AmbiguousTable, name.ValueText, symbol1, symbol2);
        }

        public static void ReportAmbiguousVariable(this ICollection<Diagnostic> diagnostics, SyntaxToken name)
        {
            diagnostics.Report(name.Span, DiagnosticId.AmbiguousVariable, name.ValueText);
        }

        public static void ReportAmbiguousAggregate(this ICollection<Diagnostic> diagnostics, SyntaxToken name, IList<AggregateSymbol> symbols)
        {
            var symbol1 = symbols[0];
            var symbol2 = symbols[1];
            diagnostics.Report(name.Span, DiagnosticId.AmbiguousAggregate, name.ValueText, symbol1, symbol2);
        }

        public static void ReportAmbiguousProperty(this ICollection<Diagnostic> diagnostics, SyntaxToken name)
        {
            diagnostics.Report(name.Span, DiagnosticId.AmbiguousProperty, name.ValueText);
        }

        //public static void ReportAmbiguousType(this ICollection<Diagnostic> diagnostics, string typeReference, Type[] candidates)
        //{
        //    diagnostics.Add(DiagnosticFactory.AmbiguousType(typeReference, candidates));
        //}

        public static void ReportAmbiguousInvocation(this ICollection<Diagnostic> diagnostics, TextSpan span, InvocableSymbol symbol1, InvocableSymbol symbol2, IList<Type> argumentTypes)
        {
            if (argumentTypes.Count > 0)
            {
                diagnostics.Report(span, DiagnosticId.AmbiguousInvocation, symbol1, symbol2);
            }
            else
            {
                var message = String.Format(CultureInfo.CurrentCulture, Resources.AmbiguousInvocationNoArgs, symbol1, symbol2);
                var diagnostic = new Diagnostic(span, DiagnosticId.AmbiguousInvocation, message);
                diagnostics.Add(diagnostic);
            }
        }

        public static void ReportInvocationRequiresParenthesis(this ICollection<Diagnostic> diagnostics, SyntaxToken name)
        {
            diagnostics.Report(name.Span, DiagnosticId.InvocationRequiresParenthesis, name.ValueText);
        }

        public static void ReportCannotApplyUnaryOperator(this ICollection<Diagnostic> diagnostics, TextSpan span, UnaryOperatorKind operatorKind, Type type)
        {
            var operatorName = operatorKind.ToDisplayName();
            var argumentTypeName = type.ToDisplayName();
            diagnostics.Report(span, DiagnosticId.CannotApplyUnaryOperator, operatorName, argumentTypeName);
        }

        public static void ReportAmbiguousUnaryOperator(this ICollection<Diagnostic> diagnostics, TextSpan span, UnaryOperatorKind operatorKind, Type type)
        {
            var operatorName = operatorKind.ToDisplayName();
            var argumentTypeName = type.ToDisplayName();
            diagnostics.Report(span, DiagnosticId.AmbiguousUnaryOperator, operatorName, argumentTypeName);
        }

        public static void ReportCannotApplyBinaryOperator(this ICollection<Diagnostic> diagnostics, TextSpan span, BinaryOperatorKind operatorKind, Type leftType, Type rightType)
        {
            var operatorName = operatorKind.ToDisplayName();
            var leftTypeName = leftType.ToDisplayName();
            var rightTypeName = rightType.ToDisplayName();
            diagnostics.Report(span, DiagnosticId.CannotApplyBinaryOperator, operatorName, leftTypeName, rightTypeName);
        }

        public static void ReportAmbiguousBinaryOperator(this ICollection<Diagnostic> diagnostics, TextSpan span, BinaryOperatorKind operatorKind, Type leftType, Type rightType)
        {
            var operatorName = operatorKind.ToDisplayName();
            var leftTypeName = leftType.ToDisplayName();
            var rightTypeName = rightType.ToDisplayName();
            diagnostics.Report(span, DiagnosticId.AmbiguousBinaryOperator, operatorName, leftTypeName, rightTypeName);
        }

        public static void ReportAmbiguousConversion(this ICollection<Diagnostic> diagnostics, TextSpan span, Type sourceType, Type targetType)
        {
            var sourceTypeName = sourceType.ToDisplayName();
            var targetTypeName = targetType.ToDisplayName();
            diagnostics.Report(span, DiagnosticId.AmbiguousConversion, sourceTypeName, targetTypeName);
        }

        public static void ReportCannotConvert(this ICollection<Diagnostic> diagnostics, TextSpan span, Type sourceType, Type targetType)
        {
            var sourceTypeName = sourceType.ToDisplayName();
            var targetTypeName = targetType.ToDisplayName();
            diagnostics.Report(span, DiagnosticId.CannotConvert, sourceTypeName, targetTypeName);
        }

        //public static void ReportAsteriskModifierNotAllowed(this ICollection<Diagnostic> diagnostics, SyntaxNodeOrToken nodeOrToken, ExpressionNode functionInvocation)
        //{
        //    diagnostics.Add(DiagnosticFactory.AsteriskModifierNotAllowed(nodeOrToken, functionInvocation));
        //}

        public static void ReportWhenMustEvaluateToBool(this ICollection<Diagnostic> diagnostics, TextSpan span)
        {
            var typeName = typeof (bool).ToDisplayName();
            diagnostics.Report(span, DiagnosticId.WhenMustEvaluateToBool, typeName);
        }

        //public static void ReportCannotLoadTypeAssembly(this ICollection<Diagnostic> diagnostics, string assemblyName, Exception exception)
        //{
        //    diagnostics.Add(DiagnosticFactory.CannotLoadTypeAssembly(assemblyName, exception));
        //}

        //public static void ReportCannotFoldConstants(this ICollection<Diagnostic> diagnostics, RuntimeException exception)
        //{
        //    diagnostics.Add(DiagnosticFactory.CannotFoldConstants(exception));
        //}

        #endregion

        #region Query Errors

        public static void ReportMustSpecifyTableToSelectFrom(this ICollection<Diagnostic> diagnostics, TextSpan textSpan)
        {
            diagnostics.Report(textSpan, DiagnosticId.MustSpecifyTableToSelectFrom);
        }

        public static void ReportAggregateInAggregateArgument(this ICollection<Diagnostic> diagnostics, TextSpan textSpan)
        {
            diagnostics.Report(textSpan, DiagnosticId.AggregateCannotContainAggregate);
        }

        public static void ReportAggregateCannotContainSubquery(this ICollection<Diagnostic> diagnostics, TextSpan textSpan)
        {
            diagnostics.Report(textSpan, DiagnosticId.AggregateCannotContainSubquery);
        }

        public static void ReportGroupByCannotContainSubquery(this ICollection<Diagnostic> diagnostics, TextSpan textSpan)
        {
            diagnostics.Report(textSpan, DiagnosticId.GroupByCannotContainSubquery);
        }

        //public static void ReportAggregateDoesNotSupportType(this ICollection<Diagnostic> diagnostics, AggregateBinding aggregateBinding, Type argumentType)
        //{
        //    diagnostics.Add(DiagnosticFactory.AggregateDoesNotSupportType(aggregateBinding, argumentType));
        //}

        public static void ReportAggregateInWhere(this ICollection<Diagnostic> diagnostics, TextSpan textSpan)
        {
            diagnostics.Report(textSpan, DiagnosticId.AggregateInWhere);
        }

        public static void ReportAggregateInOn(this ICollection<Diagnostic> diagnostics, TextSpan textSpan)
        {
            diagnostics.Report(textSpan, DiagnosticId.AggregateInOn);
        }

        public static void ReportAggregateInGroupBy(this ICollection<Diagnostic> diagnostics, TextSpan textSpan)
        {
            diagnostics.Report(textSpan, DiagnosticId.AggregateInGroupBy);
        }

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

        public static void ReportOrderByColumnPositionIsOutOfRange(this ICollection<Diagnostic> diagnostics, TextSpan textSpan, int position, int numberOfColumns)
        {
            diagnostics.Report(textSpan, DiagnosticId.OrderByColumnPositionIsOutOfRange, position, numberOfColumns);
        }

        public static void ReportWhereClauseMustEvaluateToBool(this ICollection<Diagnostic> diagnostics, TextSpan span)
        {
            diagnostics.Report(span, DiagnosticId.WhereClauseMustEvaluateToBool);
        }

        public static void ReportOnClauseMustEvaluateToBool(this ICollection<Diagnostic> diagnostics, TextSpan span)
        {
            diagnostics.Report(span, DiagnosticId.OnClauseMustEvaluateToBool);
        }

        public static void ReportHavingClauseMustEvaluateToBool(this ICollection<Diagnostic> diagnostics, TextSpan span)
        {
            diagnostics.Report(span, DiagnosticId.HavingClauseMustEvaluateToBool);
        }

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

        public static void ReportDifferentExpressionCountInBinaryQuery(this ICollection<Diagnostic> diagnostics, TextSpan textSpan)
        {
            diagnostics.Report(textSpan, DiagnosticId.DifferentExpressionCountInBinaryQuery);
        }

        public static void ReportOrderByItemsMustBeInSelectListIfUnionSpecified(this ICollection<Diagnostic> diagnostics, TextSpan textSpan)
        {
            diagnostics.Report(textSpan, DiagnosticId.OrderByItemsMustBeInSelectListIfUnionSpecified);
        }

        //public static void ReportOrderByItemsMustBeInSelectListIfDistinctSpecified(this ICollection<Diagnostic> diagnostics)
        //{
        //    diagnostics.Add(DiagnosticFactory.OrderByItemsMustBeInSelectListIfDistinctSpecified());
        //}

        //public static void ReportGroupByItemDoesNotReferenceAnyColumns(this ICollection<Diagnostic> diagnostics)
        //{
        //    diagnostics.Add(DiagnosticFactory.GroupByItemDoesNotReferenceAnyColumns());
        //}

        public static void ReportConstantExpressionInOrderBy(this ICollection<Diagnostic> diagnostics, TextSpan textSpan)
        {
            diagnostics.Report(textSpan, DiagnosticId.ConstantExpressionInOrderBy);
        }

        //public static void ReportTooManyExpressionsInSelectListOfSubquery(this ICollection<Diagnostic> diagnostics)
        //{
        //    diagnostics.Add(DiagnosticFactory.TooManyExpressionsInSelectListOfSubquery());
        //}

        public static void ReportInvalidRowReference(this ICollection<Diagnostic> diagnostics, SyntaxToken tableName)
        {
            diagnostics.Report(tableName.Span, DiagnosticId.InvalidRowReference, tableName.ValueText);
        }

        public static void ReportNoColumnAliasSpecified(this ICollection<Diagnostic> diagnostics, SyntaxToken tableName, int columnIndex)
        {
            diagnostics.Report(tableName.Span, DiagnosticId.NoColumnAliasSpecified, columnIndex + 1, tableName.ValueText);
        }

        public static void ReportCteHasMoreColumnsThanSpecified(this ICollection<Diagnostic> diagnostics, SyntaxToken cteTableName)
        {
            diagnostics.Report(cteTableName.Span, DiagnosticId.CteHasMoreColumnsThanSpecified, cteTableName.ValueText);
        }

        public static void ReportCteHasFewerColumnsThanSpecified(this ICollection<Diagnostic> diagnostics, SyntaxToken cteTableName)
        {
            diagnostics.Report(cteTableName.Span, DiagnosticId.CteHasFewerColumnsThanSpecified, cteTableName.ValueText);
        }

        public static void ReportCteHasDuplicateColumnName(this ICollection<Diagnostic> diagnostics, SyntaxToken cteTableName, string columnName)
        {
            diagnostics.Report(cteTableName.Span, DiagnosticId.CteHasDuplicateColumnName, columnName, cteTableName.ValueText);
        }

        public static void ReportCteHasDuplicateTableName(this ICollection<Diagnostic> diagnostics, SyntaxToken cteTableName)
        {
            diagnostics.Report(cteTableName.Span, DiagnosticId.CteHasDuplicateTableName, cteTableName.ValueText);
        }

        public static void ReportCteDoesNotHaveUnionAll(this ICollection<Diagnostic> diagnostics, SyntaxToken cteTableName)
        {
            diagnostics.Report(cteTableName.Span, DiagnosticId.CteDoesNotHaveUnionAll, cteTableName.ValueText);
        }

        public static void ReportCteDoesNotHaveAnchorMember(this ICollection<Diagnostic> diagnostics, SyntaxToken cteTableName)
        {
            diagnostics.Report(cteTableName.Span, DiagnosticId.CteDoesNotHaveAnchorMember, cteTableName.ValueText);
        }

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