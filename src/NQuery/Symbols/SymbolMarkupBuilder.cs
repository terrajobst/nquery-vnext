using System;
using System.Collections.Generic;

namespace NQuery.Symbols
{
    internal static class SymbolMarkupBuilder
    {
        public static void AppendSymbol(this ICollection<SymbolMarkupToken> markup, Symbol symbol)
        {
            switch (symbol.Kind)
            {
                case SymbolKind.BadSymbol:
                case SymbolKind.BadTable:
                    break;
                case SymbolKind.Column:
                    markup.AppendColumnSymbolInfo((ColumnSymbol)symbol);
                    break;
                case SymbolKind.SchemaTable:
                    markup.AppendSchemaTableSymbolInfo((SchemaTableSymbol)symbol);
                    break;
                case SymbolKind.DerivedTable:
                    markup.AppendDerivedTableSymbolInfo((DerivedTableSymbol)symbol);
                    break;
                case SymbolKind.TableInstance:
                    markup.AppendTableInstanceSymbolInfo((TableInstanceSymbol)symbol);
                    break;
                case SymbolKind.TableColumnInstance:
                    markup.AppendTableColumnInstanceSymbolInfo((TableColumnInstanceSymbol)symbol);
                    break;
                case SymbolKind.QueryColumnInstance:
                    markup.AppendQueryColumnInstanceSymbolInfo((QueryColumnInstanceSymbol)symbol);
                    break;
                case SymbolKind.CommonTableExpression:
                    markup.AppendCommonTableExpressionSymbolInfo((CommonTableExpressionSymbol)symbol);
                    break;
                case SymbolKind.Variable:
                    markup.AppendVariableSymbolInfo((VariableSymbol)symbol);
                    break;
                case SymbolKind.Parameter:
                    markup.AppendParameterSymbolInfo((ParameterSymbol)symbol);
                    break;
                case SymbolKind.Function:
                    markup.AppendFunctionSymbolInfo((FunctionSymbol)symbol);
                    break;
                case SymbolKind.Aggregate:
                    markup.AppendAggregateSymbolInfo((AggregateSymbol)symbol);
                    break;
                case SymbolKind.Method:
                    markup.AppendMethodSymbolInfo((MethodSymbol)symbol);
                    break;
                case SymbolKind.Property:
                    markup.AppendPropertySymbolInfo((PropertySymbol)symbol);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static void AppendCastSymbol(this ICollection<SymbolMarkupToken> markup)
        {
            markup.AppendKeyword("CAST");
            markup.AppendPunctuation("(");
            markup.AppendParameterName("expression");
            markup.AppendSpace();
            markup.AppendKeyword("AS");
            markup.AppendSpace();
            markup.AppendParameterName("dataType");
            markup.AppendPunctuation(")");
        }

        public static void AppendCoalesceSymbol(this ICollection<SymbolMarkupToken> markup)
        {
            markup.AppendKeyword("COALESCE");
            markup.AppendPunctuation("(");
            markup.AppendParameterName("expression1");
            markup.AppendPunctuation(",");
            markup.AppendSpace();
            markup.AppendParameterName("expression2");
            markup.AppendPunctuation(",");
            markup.AppendSpace();
            markup.AppendParameterName("[, ...]");
            markup.AppendPunctuation(")");
        }

        public static void AppendNullIfSymbol(this ICollection<SymbolMarkupToken> markup)
        {
            markup.AppendKeyword("NULLIF");
            markup.AppendPunctuation("(");
            markup.AppendParameterName("expression1");
            markup.AppendPunctuation(",");
            markup.AppendSpace();
            markup.AppendParameterName("expression2");
            markup.AppendPunctuation(")");
        }

        private static void Append(this ICollection<SymbolMarkupToken> markup, SymbolMarkupKind kind, string text)
        {
            markup.Add(new SymbolMarkupToken(kind, text));
        }

        private static void AppendName(this ICollection<SymbolMarkupToken> markup, SymbolMarkupKind kind, string name)
        {
            var displayName = string.IsNullOrEmpty(name)
                                  ? "?"
                                  : SyntaxFacts.GetValidIdentifier(name);
            markup.Append(kind, displayName);
        }

        private static void AppendKeyword(this ICollection<SymbolMarkupToken> markup, string text)
        {
            markup.Append(SymbolMarkupKind.Keyword, text);
        }

        private static void AppendSpace(this ICollection<SymbolMarkupToken> markup)
        {
            markup.Append(SymbolMarkupKind.Whitespace, " ");
        }

        private static void AppendTableName(this ICollection<SymbolMarkupToken> markup, string text)
        {
            markup.AppendName(SymbolMarkupKind.TableName, text);
        }

        private static void AppendCommonTableExpressionName(this ICollection<SymbolMarkupToken> markup, string text)
        {
            markup.AppendName(SymbolMarkupKind.CommonTableExpressionName, text);
        }

        private static void AppendColumnName(this ICollection<SymbolMarkupToken> markup, string text)
        {
            markup.AppendName(SymbolMarkupKind.ColumnName, text);
        }

        private static void AppendVariableName(this ICollection<SymbolMarkupToken> markup, string text)
        {
            markup.AppendName(SymbolMarkupKind.VariableName, text);
        }

        private static void AppendParameterName(this ICollection<SymbolMarkupToken> markup, string text)
        {
            markup.AppendName(SymbolMarkupKind.ParameterName, text);
        }

        private static void AppendFunctionName(this ICollection<SymbolMarkupToken> markup, string text)
        {
            markup.AppendName(SymbolMarkupKind.FunctionName, text);
        }

        private static void AppendAggregateName(this ICollection<SymbolMarkupToken> markup, string text)
        {
            markup.AppendName(SymbolMarkupKind.AggregateName, text);
        }

        private static void AppendMethodName(this ICollection<SymbolMarkupToken> markup, string text)
        {
            markup.AppendName(SymbolMarkupKind.MethodName, text);
        }

        private static void AppendPropertyName(this ICollection<SymbolMarkupToken> markup, string text)
        {
            markup.AppendName(SymbolMarkupKind.PropertyName, text);
        }

        private static void AppendPunctuation(this ICollection<SymbolMarkupToken> markup, string text)
        {
            markup.Append(SymbolMarkupKind.Punctuation, text);
        }

        private static void AppendType(this ICollection<SymbolMarkupToken> markup, Type type)
        {
            markup.AppendName(SymbolMarkupKind.TypeName, type.Name.ToUpper());
        }

        private static void AppendAsType(this ICollection<SymbolMarkupToken> markup, Type type)
        {
            markup.AppendKeyword("AS");
            markup.AppendSpace();
            markup.AppendType(type);
        }

        private static void AppendColumnSymbolInfo(this ICollection<SymbolMarkupToken> markup, ColumnSymbol symbol)
        {
            markup.AppendKeyword("COLUMN");
            markup.AppendSpace();
            markup.AppendColumnName(symbol.Name);
            markup.AppendSpace();
            markup.AppendAsType(symbol.Type);
        }

        private static void AppendSchemaTableSymbolInfo(this ICollection<SymbolMarkupToken> markup, SchemaTableSymbol symbol)
        {
            markup.AppendKeyword("TABLE");
            markup.AppendSpace();
            markup.AppendTableName(symbol.Name);
            if (!symbol.Type.IsMissing())
            {
                markup.AppendSpace();
                markup.AppendAsType(symbol.Type);
            }
        }

        private static void AppendDerivedTableSymbolInfo(this ICollection<SymbolMarkupToken> markup, DerivedTableSymbol symbol)
        {
            markup.AppendKeyword("DERIVED");
            markup.AppendSpace();
            markup.AppendKeyword("TABLE");
        }

        private static void AppendTableInstanceSymbolInfo(this ICollection<SymbolMarkupToken> markup, TableInstanceSymbol symbol)
        {
            markup.AppendKeyword("ALIAS");
            markup.AppendSpace();
            markup.AppendTableName(symbol.Name);
            markup.AppendSpace();
            markup.AppendKeyword("FOR");
            markup.AppendSpace();
            markup.AppendSymbol(symbol.Table);
        }

        private static void AppendTableColumnInstanceSymbolInfo(this ICollection<SymbolMarkupToken> markup, TableColumnInstanceSymbol symbol)
        {
            markup.AppendKeyword("COLUMN");
            markup.AppendSpace();
            markup.AppendTableName(symbol.TableInstance.Name);
            markup.AppendPunctuation(".");
            markup.AppendColumnName(symbol.Name);
            markup.AppendSpace();
            markup.AppendAsType(symbol.Type);
            markup.AppendSpace();
            markup.AppendKeyword("OF");
            markup.AppendSpace();
            markup.AppendSymbol(symbol.TableInstance.Table);
        }

        private static void AppendQueryColumnInstanceSymbolInfo(this ICollection<SymbolMarkupToken> markup, QueryColumnInstanceSymbol symbol)
        {
            markup.AppendKeyword("COLUMN");
            markup.AppendSpace();
            markup.AppendColumnName(symbol.Name);
            markup.AppendSpace();
            markup.AppendAsType(symbol.Type);
        }

        private static void AppendCommonTableExpressionSymbolInfo(this ICollection<SymbolMarkupToken> markup, CommonTableExpressionSymbol symbol)
        {
            markup.AppendKeyword("COMMON");
            markup.AppendSpace();
            markup.AppendKeyword("TABLE");
            markup.AppendSpace();
            markup.AppendKeyword("EXPRESSION");
            markup.AppendSpace();
            markup.AppendCommonTableExpressionName(symbol.Name);
        }

        private static void AppendVariableSymbolInfo(this ICollection<SymbolMarkupToken> markup, VariableSymbol symbol)
        {
            markup.AppendKeyword("VARIABLE");
            markup.AppendSpace();
            markup.AppendPunctuation("@");
            markup.AppendVariableName(symbol.Name);
            markup.AppendSpace();
            markup.AppendAsType(symbol.Type);
        }

        private static void AppendParameterSymbolInfo(this ICollection<SymbolMarkupToken> markup, ParameterSymbol symbol)
        {
            markup.AppendKeyword("PARAMETER");
            markup.AppendSpace();
            markup.AppendParameterName(symbol.Name);
            markup.AppendSpace();
            markup.AppendAsType(symbol.Type);
        }

        private static void AppendFunctionSymbolInfo(this ICollection<SymbolMarkupToken> markup, FunctionSymbol symbol)
        {
            markup.AppendKeyword("FUNCTION");
            markup.AppendSpace();
            markup.AppendFunctionName(symbol.Name);
            markup.AppendInvocable(symbol);
        }

        private static void AppendMethodSymbolInfo(this ICollection<SymbolMarkupToken> markup, MethodSymbol symbol)
        {
            markup.AppendKeyword("METHOD");
            markup.AppendSpace();
            markup.AppendMethodName(symbol.Name);
            markup.AppendInvocable(symbol);
        }

        private static void AppendPropertySymbolInfo(this ICollection<SymbolMarkupToken> markup, PropertySymbol symbol)
        {
            markup.AppendKeyword("PROPERTY");
            markup.AppendSpace();
            markup.AppendPropertyName(symbol.Name);
            markup.AppendSpace();
            markup.AppendAsType(symbol.Type);
        }

        private static void AppendAggregateSymbolInfo(this ICollection<SymbolMarkupToken> markup, AggregateSymbol symbol)
        {
            markup.AppendKeyword("AGGREGATE");
            markup.AppendSpace();
            markup.AppendAggregateName(symbol.Name);
            markup.AppendPunctuation("(");
            markup.AppendParameterName("value");
            markup.AppendPunctuation(")");
        }

        private static void AppendInvocable(this ICollection<SymbolMarkupToken> markup, InvocableSymbol symbol)
        {
            markup.AppendPunctuation("(");

            var isFirst = true;
            foreach (var parameterSymbol in symbol.Parameters)
            {
                if (isFirst)
                    isFirst = false;
                else
                {
                    markup.AppendPunctuation(",");
                    markup.AppendSpace();
                }

                markup.AppendParameterName(parameterSymbol.Name);
                markup.AppendSpace();
                markup.AppendAsType(parameterSymbol.Type);
            }

            markup.AppendPunctuation(")");
            markup.AppendSpace();
            markup.AppendAsType(symbol.Type);
        }
    }
}