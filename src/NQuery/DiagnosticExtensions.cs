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
        #region Lexer CompilationErrors

        public static void ReportIllegalInputCharacter(this ICollection<Diagnostic> diagnostics, TextSpan textSpan, char character)
        {
            var message = String.Format(CultureInfo.CurrentCulture, Resources.IllegalInputCharacter, character);
            var diagnostic = new Diagnostic(textSpan, DiagnosticId.IllegalInputCharacter, message);
            diagnostics.Add(diagnostic);
        }

        public static void ReportUnterminatedComment(this ICollection<Diagnostic> diagnostics, TextSpan textSpan)
        {
            var diagnostic = new Diagnostic(textSpan, DiagnosticId.UnterminatedComment, Resources.UnterminatedComment);
            diagnostics.Add(diagnostic);
        }

        public static void ReportUnterminatedString(this ICollection<Diagnostic> diagnostics, TextSpan textSpan)
        {
            var diagnostic = new Diagnostic(textSpan, DiagnosticId.UnterminatedString, Resources.UnterminatedString);
            diagnostics.Add(diagnostic);
        }

        public static void ReportUnterminatedQuotedIdentifier(this ICollection<Diagnostic> diagnostics, TextSpan textSpan)
        {
            var diagnostic = new Diagnostic(textSpan, DiagnosticId.UnterminatedQuotedIdentifier, Resources.UnterminatedQuotedIdentifier);
            diagnostics.Add(diagnostic);
        }

        public static void ReportUnterminatedParenthesizedIdentifier(this ICollection<Diagnostic> diagnostics, TextSpan textSpan)
        {
            var diagnostic = new Diagnostic(textSpan, DiagnosticId.UnterminatedParenthesizedIdentifier, Resources.UnterminatedParenthesizedIdentifier);
            diagnostics.Add(diagnostic);
        }

        public static void ReportUnterminatedDate(this ICollection<Diagnostic> diagnostics, TextSpan textSpan)
        {
            var diagnostic = new Diagnostic(textSpan, DiagnosticId.UnterminatedDate, Resources.UnterminatedDate);
            diagnostics.Add(diagnostic);
        }

        public static void ReportInvalidDate(this ICollection<Diagnostic> diagnostics, TextSpan textSpan, string tokenText)
        {
            var message = String.Format(CultureInfo.CurrentCulture, Resources.InvalidDate, tokenText);
            var diagnostic = new Diagnostic(textSpan, DiagnosticId.InvalidDate, message);
            diagnostics.Add(diagnostic);
        }

        public static void ReportInvalidInteger(this ICollection<Diagnostic> diagnostics, TextSpan textSpan, string tokenText)
        {
            var message = String.Format(CultureInfo.CurrentCulture, Resources.InvalidInteger, tokenText);
            var diagnostic = new Diagnostic(textSpan, DiagnosticId.InvalidInteger, message);
            diagnostics.Add(diagnostic);
        }

        public static void ReportInvalidReal(this ICollection<Diagnostic> diagnostics, TextSpan textSpan, string tokenText)
        {
            var message = String.Format(CultureInfo.CurrentCulture, Resources.InvalidDecimal, tokenText);
            var diagnostic = new Diagnostic(textSpan, DiagnosticId.InvalidReal, message);
            diagnostics.Add(diagnostic);
        }

        public static void ReportInvalidBinary(this ICollection<Diagnostic> diagnostics, TextSpan textSpan, string tokenText)
        {
            var message = String.Format(CultureInfo.CurrentCulture, Resources.InvalidBinary, tokenText);
            var diagnostic = new Diagnostic(textSpan, DiagnosticId.InvalidBinary, message);
            diagnostics.Add(diagnostic);
        }

        public static void ReportInvalidOctal(this ICollection<Diagnostic> diagnostics, TextSpan textSpan, string tokenText)
        {
            var message = String.Format(CultureInfo.CurrentCulture, Resources.InvalidOctal, tokenText);
            var diagnostic = new Diagnostic(textSpan, DiagnosticId.InvalidOctal, message);
            diagnostics.Add(diagnostic);
        }

        public static void ReportInvalidHex(this ICollection<Diagnostic> diagnostics, TextSpan textSpan, string tokenText)
        {
            var message = String.Format(CultureInfo.CurrentCulture, Resources.InvalidHex, tokenText);
            var diagnostic = new Diagnostic(textSpan, DiagnosticId.InvalidHex, message);
            diagnostics.Add(diagnostic);
        }

        public static void ReportNumberTooLarge(this ICollection<Diagnostic> diagnostics, TextSpan textSpan, string tokenText)
        {
            var message = String.Format(CultureInfo.CurrentCulture, Resources.NumberTooLarge, tokenText);
            var diagnostic = new Diagnostic(textSpan, DiagnosticId.NumberTooLarge, message);
            diagnostics.Add(diagnostic);
        }

        #endregion

        #region Parser Errors

        public static void ReportTokenExpected(this ICollection<Diagnostic> diagnostics, TextSpan span, SyntaxToken actual, SyntaxKind expected)
        {
            var actualText = actual.GetDisplayText();
            var expectedText = expected.GetDisplayText();
            var message = String.Format(CultureInfo.CurrentCulture, Resources.TokenExpected, actualText, expectedText);
            var diagnostic = new Diagnostic(span, DiagnosticId.TokenExpected, message);
            diagnostics.Add(diagnostic);
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
            var message = String.Format(CultureInfo.CurrentCulture, Resources.InvalidOperatorForAllAny, operatorToken.Kind.GetText());
            var diagnostic = new Diagnostic(operatorToken.Span, DiagnosticId.InvalidOperatorForAllAny, message);
            return operatorToken.WithDiagnotics(new[] {diagnostic});
        }

        #endregion

        #region Resolving/Evaluation Errors

        public static void ReportUndeclaredTable(this ICollection<Diagnostic> diagnostics, NamedTableReferenceSyntax namedTableReference)
        {
            var tableName = namedTableReference.TableName;
            var message = String.Format(CultureInfo.CurrentCulture, Resources.UndeclaredTable, tableName.ValueText);
            var diagnostic = new Diagnostic(tableName.Span, DiagnosticId.UndeclaredTable, message);
            diagnostics.Add(diagnostic);
        }

        public static void ReportUndeclaredTableInstance(this ICollection<Diagnostic> diagnostics, SyntaxToken name)
        {
            var message = String.Format(CultureInfo.CurrentCulture, Resources.UndeclaredTableInstance, name.ValueText);
            var diagnostic = new Diagnostic(name.Span, DiagnosticId.UndeclaredTableInstance, message);
            diagnostics.Add(diagnostic);
        }

        public static void ReportUndeclaredVariable(this ICollection<Diagnostic> diagnostics, VariableExpressionSyntax node)
        {
            var variableName = node.Name;
            var message = String.Format(CultureInfo.CurrentCulture, Resources.UndeclaredVariable, variableName.ValueText);
            var diagnostic = new Diagnostic(variableName.Span, DiagnosticId.UndeclaredVariable, message);
            diagnostics.Add(diagnostic);
        }

        public static void ReportUndeclaredFunction(this ICollection<Diagnostic> diagnostics, FunctionInvocationExpressionSyntax node, IEnumerable<Type> argumentTypes)
        {
            var name = node.Name.ValueText;
            var argumentTypeList = string.Join(", ", argumentTypes.Select(t => t.ToDisplayName()));
            var message = String.Format(CultureInfo.CurrentCulture, Resources.UndeclaredFunction, name, argumentTypeList);
            var diagnostic = new Diagnostic(node.Span, DiagnosticId.UndeclaredFunction, message);
            diagnostics.Add(diagnostic);
        }

        public static void ReportUndeclaredAggregate(this ICollection<Diagnostic> diagnostics, SyntaxToken name)
        {
            var nameText = name.ValueText;
            var message = String.Format(CultureInfo.CurrentCulture, Resources.UndeclaredAggregate, nameText);
            var diagnostic = new Diagnostic(name.Span, DiagnosticId.UndeclaredAggregate, message);
            diagnostics.Add(diagnostic);
        }

        public static void ReportUndeclaredMethod(this ICollection<Diagnostic> diagnostics, MethodInvocationExpressionSyntax node, Type declaringType, IEnumerable<Type> argumentTypes)
        {
            var name = node.Name.ValueText;
            var declaringTypeName = declaringType.ToDisplayName();
            var argumentTypeNames = string.Join(", ", argumentTypes.Select(t => t.ToDisplayName()));
            var message = String.Format(CultureInfo.CurrentCulture, Resources.UndeclaredMethod, declaringTypeName, name, argumentTypeNames);
            var diagnostic = new Diagnostic(node.Span, DiagnosticId.UndeclaredMethod, message);
            diagnostics.Add(diagnostic);
        }

        public static void ReportUndeclaredColumn(this ICollection<Diagnostic> diagnostics, PropertyAccessExpressionSyntax node, TableInstanceSymbol tableInstance)
        {
            var tableName = tableInstance.Name;
            var columnName = node.Name.ValueText;
            var message = String.Format(CultureInfo.CurrentCulture, Resources.UndeclaredColumn, tableName, columnName);
            var diagnostic = new Diagnostic(node.Span, DiagnosticId.UndeclaredColumn, message);
            diagnostics.Add(diagnostic);
        }

        public static void ReportUndeclaredProperty(this ICollection<Diagnostic> diagnostics, PropertyAccessExpressionSyntax node, Type type)
        {
            var typeName = type.ToDisplayName();
            var propertyName = node.Name.ValueText;
            var message = String.Format(CultureInfo.CurrentCulture, Resources.UndeclaredProperty, typeName, propertyName);
            var diagnostic = new Diagnostic(node.Span, DiagnosticId.UndeclaredProperty, message);
            diagnostics.Add(diagnostic);
        }

        public static void ReportUndeclaredType(this ICollection<Diagnostic> diagnostics, SyntaxToken typeName)
        {
            var message = String.Format(CultureInfo.CurrentCulture, Resources.UndeclaredType, typeName.ValueText);
            var diagnostic = new Diagnostic(typeName.Span, DiagnosticId.UndeclaredType, message);
            diagnostics.Add(diagnostic);
        }

        public static void ReportColumnTableOrVariableNotDeclared(this ICollection<Diagnostic> diagnostics, SyntaxToken name)
        {
            var message = String.Format(CultureInfo.CurrentCulture, Resources.ColumnTableOrVariableNotDeclared, name.ValueText);
            var diagnostic = new Diagnostic(name.Span, DiagnosticId.ColumnTableOrVariableNotDeclared, message);
            diagnostics.Add(diagnostic);
        }

        public static void ReportAmbiguousName(this ICollection<Diagnostic> diagnostics, SyntaxToken name, IList<Symbol> candidates)
        {
            var symbol1 = candidates[0];
            var symbol2 = candidates[1];
            var message = String.Format(CultureInfo.CurrentCulture, Resources.AmbiguousReference, name.ValueText, symbol1, symbol2);
            var diagnostic = new Diagnostic(name.Span, DiagnosticId.AmbiguousReference, message);
            diagnostics.Add(diagnostic);
        }

        //public static void ReportAmbiguousTableRef(this ICollection<Diagnostic> diagnostics, SyntaxNodeOrToken nodeOrToken, Identifier identifier, TableRefBinding[] candidates)
        //{
        //    diagnostics.Add(DiagnosticFactory.AmbiguousTableRef(nodeOrToken, identifier, candidates));
        //}

        public static void ReportAmbiguousColumnInstance(this ICollection<Diagnostic> diagnostics, SyntaxToken name, IList<ColumnInstanceSymbol> candidates)
        {
            var symbol1 = candidates[0];
            var symbol2 = candidates[1];
            var message = String.Format(CultureInfo.CurrentCulture, Resources.AmbiguousColumnRef, name.ValueText, symbol1, symbol2);
            var diagnostic = new Diagnostic(name.Span, DiagnosticId.AmbiguousColumnRef, message);
            diagnostics.Add(diagnostic);
        }

        public static void ReportAmbiguousTable(this ICollection<Diagnostic> diagnostics, SyntaxToken name, IList<TableSymbol> candidates)
        {
            var symbol1 = candidates[0];
            var symbol2 = candidates[1];
            var message = String.Format(CultureInfo.CurrentCulture, Resources.AmbiguousTable, name.ValueText, symbol1, symbol2);
            var diagnostic = new Diagnostic(name.Span, DiagnosticId.AmbiguousTable, message);
            diagnostics.Add(diagnostic);
        }

        public static void ReportAmbiguousVariable(this ICollection<Diagnostic> diagnostics, SyntaxToken name)
        {
            var message = String.Format(CultureInfo.CurrentCulture, Resources.AmbiguousVariable, name.ValueText);
            var diagnostic = new Diagnostic(name.Span, DiagnosticId.AmbiguousVariable, message);
            diagnostics.Add(diagnostic);
        }

        public static void ReportAmbiguousAggregate(this ICollection<Diagnostic> diagnostics, SyntaxToken name, IList<AggregateSymbol> symbols)
        {
            var symbol1 = symbols[0];
            var symbol2 = symbols[1];
            var message = String.Format(CultureInfo.CurrentCulture, Resources.AmbiguousAggregate, name, symbol1, symbol2);
            var diagnostic = new Diagnostic(name.Span, DiagnosticId.AmbiguousAggregate, message);
            diagnostics.Add(diagnostic);
        }

        public static void ReportAmbiguousProperty(this ICollection<Diagnostic> diagnostics, SyntaxToken name)
        {
            var message = String.Format(CultureInfo.CurrentCulture, Resources.AmbiguousProperty, name.ValueText);
            var diagnostic = new Diagnostic(name.Span, DiagnosticId.AmbiguousProperty, message);
            diagnostics.Add(diagnostic);
        }

        //public static void ReportAmbiguousType(this ICollection<Diagnostic> diagnostics, string typeReference, Type[] candidates)
        //{
        //    diagnostics.Add(DiagnosticFactory.AmbiguousType(typeReference, candidates));
        //}

        public static void ReportAmbiguousInvocation(this ICollection<Diagnostic> diagnostics, TextSpan span, InvocableSymbol symbol1, InvocableSymbol symbol2, IList<Type> argumentTypes)
        {
            if (argumentTypes.Count == 0)
            {
                var message = String.Format(CultureInfo.CurrentCulture, Resources.AmbiguousInvocationNoArgs, symbol1, symbol2);
                var diagnostic = new Diagnostic(span, DiagnosticId.AmbiguousInvocation, message);
                diagnostics.Add(diagnostic);
            }
            else
            {
                var argumentTypeNames = string.Join(",", argumentTypes.Select(t => t.ToDisplayName()));
                var message = String.Format(CultureInfo.CurrentCulture, Resources.AmbiguousInvocation, symbol1, symbol2, argumentTypeNames);
                var diagnostic = new Diagnostic(span, DiagnosticId.AmbiguousInvocation, message);
                diagnostics.Add(diagnostic);
            }
        }

        public static void ReportInvocationRequiresParenthesis(this ICollection<Diagnostic> diagnostics, SyntaxToken name)
        {
            var message = String.Format(CultureInfo.CurrentCulture, Resources.InvocationRequiresParenthesis, name.ValueText);
            var diagnostic = new Diagnostic(name.Span, DiagnosticId.InvocationRequiresParenthesis, message);
            diagnostics.Add(diagnostic);
        }

        public static void ReportCannotApplyUnaryOperator(this ICollection<Diagnostic> diagnostics, TextSpan span, UnaryOperatorKind operatorKind, Type type)
        {
            var operatorName = operatorKind.ToDisplayName();
            var argumentTypeName = type.ToDisplayName();
            var message = String.Format(CultureInfo.CurrentCulture, Resources.CannotApplyUnaryOperator, operatorName, argumentTypeName);
            var diagnostic = new Diagnostic(span, DiagnosticId.CannotApplyUnaryOperator, message);
            diagnostics.Add(diagnostic);
        }

        public static void ReportAmbiguousUnaryOperator(this ICollection<Diagnostic> diagnostics, TextSpan span, UnaryOperatorKind operatorKind, Type type)
        {
            var operatorName = operatorKind.ToDisplayName();
            var argumentTypeName = type.ToDisplayName();
            var message = String.Format(CultureInfo.CurrentCulture, Resources.AmbiguousUnaryOp, operatorName, argumentTypeName);
            var diagnostic = new Diagnostic(span, DiagnosticId.AmbiguousUnaryOperator, message);
            diagnostics.Add(diagnostic);
        }

        public static void ReportCannotApplyBinaryOperator(this ICollection<Diagnostic> diagnostics, TextSpan span, BinaryOperatorKind operatorKind, Type leftType, Type rightType)
        {
            var operatorName = operatorKind.ToDisplayName();
            var leftTypeName = leftType.ToDisplayName();
            var rightTypeName = rightType.ToDisplayName();
            var message = String.Format(CultureInfo.CurrentCulture, Resources.CannotApplyBinaryOp, operatorName, leftTypeName, rightTypeName);
            var diagnostic = new Diagnostic(span, DiagnosticId.CannotApplyBinaryOperator, message);
            diagnostics.Add(diagnostic);
        }

        public static void ReportAmbiguousBinaryOperator(this ICollection<Diagnostic> diagnostics, TextSpan span, BinaryOperatorKind operatorKind, Type leftType, Type rightType)
        {
            var operatorName = operatorKind.ToDisplayName();
            var leftTypeName = leftType.ToDisplayName();
            var rightTypeName = rightType.ToDisplayName();
            var message = String.Format(CultureInfo.CurrentCulture, Resources.AmbiguousBinaryOperator, operatorName, leftTypeName, rightTypeName);
            var diagnostic = new Diagnostic(span, DiagnosticId.AmbiguousBinaryOperator, message);
            diagnostics.Add(diagnostic);
        }

        public static void ReportAmbiguousConversion(this ICollection<Diagnostic> diagnostics, TextSpan span, Type sourceType, Type targetType)
        {
            var sourceTypeName = sourceType.ToDisplayName();
            var targetTypeName = targetType.ToDisplayName();
            var message = String.Format(CultureInfo.CurrentCulture, Resources.AmbiguousConversion, sourceTypeName, targetTypeName);
            var diagnostic = new Diagnostic(span, DiagnosticId.AmbiguousConversion, message);
            diagnostics.Add(diagnostic);
        }

        public static void ReportCannotConvert(this ICollection<Diagnostic> diagnostics, TextSpan span, Type sourceType, Type targetType)
        {
            var sourceTypeName = sourceType.ToDisplayName();
            var targeTypeName = targetType.ToDisplayName();
            var message = String.Format(CultureInfo.CurrentCulture, Resources.CannotConvert, sourceTypeName, targeTypeName);
            var diagnostic = new Diagnostic(span, DiagnosticId.CannotConvert, message);
            diagnostics.Add(diagnostic);
        }

        //public static void ReportAsteriskModifierNotAllowed(this ICollection<Diagnostic> diagnostics, SyntaxNodeOrToken nodeOrToken, ExpressionNode functionInvocation)
        //{
        //    diagnostics.Add(DiagnosticFactory.AsteriskModifierNotAllowed(nodeOrToken, functionInvocation));
        //}

        public static void ReportWhenMustEvaluateToBool(this ICollection<Diagnostic> diagnostics, TextSpan span)
        {
            var typeName = typeof (bool).ToDisplayName();
            var message = String.Format(CultureInfo.CurrentCulture, Resources.WhenMustEvaluateToBool, typeName);
            var diagnostic = new Diagnostic(span, DiagnosticId.WhenMustEvaluateToBool, message);
            diagnostics.Add(diagnostic);
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
            var diagnostic = new Diagnostic(textSpan, DiagnosticId.MustSpecifyTableToSelectFrom, Resources.MustSpecifyTableToSelectFrom);
            diagnostics.Add(diagnostic);
        }

        public static void ReportAggregateInAggregateArgument(this ICollection<Diagnostic> diagnostics, TextSpan textSpan)
        {
            var diagnostic = new Diagnostic(textSpan, DiagnosticId.AggregateCannotContainAggregate, Resources.AggregateCannotContainAggregate);
            diagnostics.Add(diagnostic);
        }

        public static void ReportAggregateCannotContainSubquery(this ICollection<Diagnostic> diagnostics, TextSpan textSpan)
        {
            var diagnostic = new Diagnostic(textSpan, DiagnosticId.AggregateCannotContainSubquery, Resources.AggregateCannotContainSubquery);
            diagnostics.Add(diagnostic);
        }

        public static void ReportGroupByCannotContainSubquery(this ICollection<Diagnostic> diagnostics, TextSpan textSpan)
        {
            var diagnostic = new Diagnostic(textSpan, DiagnosticId.GroupByCannotContainSubquery, Resources.GroupByCannotContainSubquery);
            diagnostics.Add(diagnostic);
        }

        //public static void ReportAggregateDoesNotSupportType(this ICollection<Diagnostic> diagnostics, AggregateBinding aggregateBinding, Type argumentType)
        //{
        //    diagnostics.Add(DiagnosticFactory.AggregateDoesNotSupportType(aggregateBinding, argumentType));
        //}

        public static void ReportAggregateInWhere(this ICollection<Diagnostic> diagnostics, TextSpan textSpan)
        {
            var diagnostic = new Diagnostic(textSpan, DiagnosticId.AggregateInWhere, Resources.AggregateInWhere);
            diagnostics.Add(diagnostic);
        }

        public static void ReportAggregateInOn(this ICollection<Diagnostic> diagnostics, TextSpan textSpan)
        {
            var diagnostic = new Diagnostic(textSpan, DiagnosticId.AggregateInOn, Resources.AggregateInOn);
            diagnostics.Add(diagnostic);
        }

        public static void ReportAggregateInGroupBy(this ICollection<Diagnostic> diagnostics, TextSpan textSpan)
        {
            var diagnostic = new Diagnostic(textSpan, DiagnosticId.AggregateInGroupBy, Resources.AggregateInGroupBy);
            diagnostics.Add(diagnostic);
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
            var message = String.Format(CultureInfo.CurrentCulture, Resources.OrderByColumnPositionIsOutOfRange, position, numberOfColumns);
            var diagnostic = new Diagnostic(textSpan, DiagnosticId.OrderByColumnPositionIsOutOfRange, message);
            diagnostics.Add(diagnostic);
        }

        public static void ReportWhereClauseMustEvaluateToBool(this ICollection<Diagnostic> diagnostics, TextSpan span)
        {
            var diagnostic = new Diagnostic(span, DiagnosticId.WhereClauseMustEvaluateToBool, Resources.WhereClauseMustEvaluateToBool);
            diagnostics.Add(diagnostic);
        }

        public static void ReportOnClauseMustEvaluateToBool(this ICollection<Diagnostic> diagnostics, TextSpan span)
        {
            var diagnostic = new Diagnostic(span, DiagnosticId.OnClauseMustEvaluateToBool, Resources.OnClauseMustEvaluateToBool);
            diagnostics.Add(diagnostic);
        }

        public static void ReportHavingClauseMustEvaluateToBool(this ICollection<Diagnostic> diagnostics, TextSpan span)
        {
            var diagnostic = new Diagnostic(span, DiagnosticId.HavingClauseMustEvaluateToBool, Resources.HavingClauseMustEvaluateToBool);
            diagnostics.Add(diagnostic);
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

        public static void ReportOrderByItemsMustBeInSelectListIfUnionSpecified(this ICollection<Diagnostic> diagnostics, TextSpan textSpan)
        {
            var message = Resources.OrderByItemsMustBeInSelectListIfUnionSpecified;
            var diagnostic = new Diagnostic(textSpan, DiagnosticId.OrderByItemsMustBeInSelectListIfUnionSpecified, message);
            diagnostics.Add(diagnostic);
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
            var diagnostic = new Diagnostic(textSpan, DiagnosticId.ConstantExpressionInOrderBy, Resources.ConstantExpressionInOrderBy);
            diagnostics.Add(diagnostic);
        }

        //public static void ReportTooManyExpressionsInSelectListOfSubquery(this ICollection<Diagnostic> diagnostics)
        //{
        //    diagnostics.Add(DiagnosticFactory.TooManyExpressionsInSelectListOfSubquery());
        //}

        public static void ReportInvalidRowReference(this ICollection<Diagnostic> diagnostics, SyntaxToken tableName)
        {
            var message = String.Format(CultureInfo.CurrentCulture, Resources.InvalidRowReference, tableName.ValueText);
            var diagnostic = new Diagnostic(tableName.Span, DiagnosticId.InvalidRowReference, message);
            diagnostics.Add(diagnostic);
        }

        public static void ReportNoColumnAliasSpecified(this ICollection<Diagnostic> diagnostics, SyntaxToken tableName, int columnIndex)
        {
            var message = String.Format(CultureInfo.CurrentCulture, Resources.NoColumnAliasSpecified, columnIndex + 1, tableName.ValueText);
            var diagnostic = new Diagnostic(tableName.Span, DiagnosticId.NoColumnAliasSpecified, message);
            diagnostics.Add(diagnostic);
        }

        public static void ReportCteHasMoreColumnsThanSpecified(this ICollection<Diagnostic> diagnostics, SyntaxToken cteTableName)
        {
            var message = String.Format(CultureInfo.CurrentCulture, Resources.CteHasMoreColumnsThanSpecified, cteTableName.ValueText);
            var diagnostic = new Diagnostic(cteTableName.Span, DiagnosticId.CteHasMoreColumnsThanSpecified, message);
            diagnostics.Add(diagnostic);
        }

        public static void ReportCteHasFewerColumnsThanSpecified(this ICollection<Diagnostic> diagnostics, SyntaxToken cteTableName)
        {
            var message = String.Format(CultureInfo.CurrentCulture, Resources.CteHasFewerColumnsThanSpecified, cteTableName.ValueText);
            var diagnostic = new Diagnostic(cteTableName.Span, DiagnosticId.CteHasFewerColumnsThanSpecified, message);
            diagnostics.Add(diagnostic);
        }

        public static void ReportCteHasDuplicateColumnName(this ICollection<Diagnostic> diagnostics, SyntaxToken cteTableName, string columnName)
        {
            var message = String.Format(CultureInfo.CurrentCulture, Resources.CteHasDuplicateColumnName, columnName, cteTableName.ValueText);
            var diagnostic = new Diagnostic(cteTableName.Span, DiagnosticId.CteHasDuplicateColumnName, message);
            diagnostics.Add(diagnostic);
        }

        public static void ReportCteHasDuplicateTableName(this ICollection<Diagnostic> diagnostics, SyntaxToken cteTableName)
        {
            var message = String.Format(CultureInfo.CurrentCulture, Resources.CteHasDuplicateTableName, cteTableName.ValueText);
            var diagnostic = new Diagnostic(cteTableName.Span, DiagnosticId.CteHasDuplicateTableName, message);
            diagnostics.Add(diagnostic);
        }

        public static void ReportCteDoesNotHaveUnionAll(this ICollection<Diagnostic> diagnostics, SyntaxToken cteTableName)
        {
            var message = String.Format(CultureInfo.CurrentCulture, Resources.CteDoesNotHaveUnionAll, cteTableName.ValueText);
            var diagnostic = new Diagnostic(cteTableName.Span, DiagnosticId.CteDoesNotHaveUnionAll, message);
            diagnostics.Add(diagnostic);
        }

        public static void ReportCteDoesNotHaveAnchorMember(this ICollection<Diagnostic> diagnostics, SyntaxToken cteTableName)
        {
            var message = String.Format(CultureInfo.CurrentCulture, Resources.CteDoesNotHaveAnchorMember, cteTableName.ValueText);
            var diagnostic = new Diagnostic(cteTableName.Span, DiagnosticId.CteDoesNotHaveAnchorMember, message);
            diagnostics.Add(diagnostic);
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