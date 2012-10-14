using System;
using System.Collections.Generic;

namespace NQuery.Language.Symbols
{
    internal static class SymbolMarkupBuilder
    {
        private static void Append(this ICollection<SymbolMarkupNode> markup, SymbolMarkupKind kind, string text)
        {
            markup.Add(new SymbolMarkupNode(kind, text));
        }

        private static void AppendKeyword(this ICollection<SymbolMarkupNode> markup, string text)
        {
            markup.Append(SymbolMarkupKind.Keyword, text);
        }

        private static void AppendSpace(this ICollection<SymbolMarkupNode> markup)
        {
            markup.Append(SymbolMarkupKind.Whitespace, " ");
        }

        private static void AppendTableName(this ICollection<SymbolMarkupNode> markup, string text)
        {
            markup.Append(SymbolMarkupKind.TableName, text);
        }

        private static void AppendDerivedTableName(this ICollection<SymbolMarkupNode> markup, string text)
        {
            markup.Append(SymbolMarkupKind.DerivedTableName, text);
        }

        private static void AppendCommonTableExpressionName(this ICollection<SymbolMarkupNode> markup, string text)
        {
            markup.Append(SymbolMarkupKind.CommonTableExpressionName, text);
        }

        private static void AppendColumnName(this ICollection<SymbolMarkupNode> markup, string text)
        {
            markup.Append(SymbolMarkupKind.ColumnName, text);
        }

        private static void AppendVariableName(this ICollection<SymbolMarkupNode> markup, string text)
        {
            markup.Append(SymbolMarkupKind.VariableName, text);
        }

        private static void AppendParameterName(this ICollection<SymbolMarkupNode> markup, string text)
        {
            markup.Append(SymbolMarkupKind.ParameterName, text);
        }

        private static void AppendFunctionName(this ICollection<SymbolMarkupNode> markup, string text)
        {
            markup.Append(SymbolMarkupKind.FunctionName, text);
        }

        private static void AppendAggregateName(this ICollection<SymbolMarkupNode> markup, string text)
        {
            markup.Append(SymbolMarkupKind.AggregateName, text);
        }

        private static void AppendMethodName(this ICollection<SymbolMarkupNode> markup, string text)
        {
            markup.Append(SymbolMarkupKind.MethodName, text);
        }

        private static void AppendPropertyName(this ICollection<SymbolMarkupNode> markup, string text)
        {
            markup.Append(SymbolMarkupKind.PropertyName, text);
        }

        private static void AppendPunctuation(this ICollection<SymbolMarkupNode> markup, string text)
        {
            markup.Append(SymbolMarkupKind.Punctuation, text);
        }

        private static void AppendType(this ICollection<SymbolMarkupNode> markup, Type type)
        {
            markup.Append(SymbolMarkupKind.TypeName, type.Name.ToUpper());
        }

        private static void AppendAsType(this ICollection<SymbolMarkupNode> markup, Type type)
        {
            markup.AppendKeyword("AS");
            markup.AppendSpace();
            markup.AppendType(type);
        }

        public static void AppendSymbol(this ICollection<SymbolMarkupNode> markup, Symbol symbol)
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
                case SymbolKind.ColumnInstance:
                    markup.AppendColumnInstanceSymbolInfo((ColumnInstanceSymbol)symbol);
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

        private static void AppendColumnSymbolInfo(this ICollection<SymbolMarkupNode> markup, ColumnSymbol symbol)
        {
            markup.AppendKeyword("COLUMN");
            markup.AppendSpace();
            markup.AppendColumnName(symbol.Name);
            markup.AppendSpace();
            markup.AppendAsType(symbol.Type);
        }

        private static void AppendSchemaTableSymbolInfo(this ICollection<SymbolMarkupNode> markup, SchemaTableSymbol symbol)
        {
            markup.AppendKeyword("TABLE");
            markup.AppendSpace();
            markup.AppendTableName(symbol.Name);
            markup.AppendSpace();
            markup.AppendAsType(symbol.Type);
        }

        private static void AppendDerivedTableSymbolInfo(this ICollection<SymbolMarkupNode> markup, DerivedTableSymbol symbol)
        {
            markup.AppendKeyword("DERIVED TABLE");
            markup.AppendSpace();
            markup.AppendDerivedTableName(symbol.Name);
        }

        private static void AppendTableInstanceSymbolInfo(this ICollection<SymbolMarkupNode> markup, TableInstanceSymbol symbol)
        {
            markup.AppendKeyword("ALIAS");
            markup.AppendSpace();
            markup.AppendTableName(symbol.Name);
            markup.AppendSpace();
            markup.AppendKeyword("FOR");
            markup.AppendSpace();
            markup.AppendSymbol(symbol.Table);
        }

        private static void AppendColumnInstanceSymbolInfo(this ICollection<SymbolMarkupNode> markup, ColumnInstanceSymbol symbol)
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

        private static void AppendCommonTableExpressionSymbolInfo(this ICollection<SymbolMarkupNode> markup, CommonTableExpressionSymbol symbol)
        {
            markup.AppendKeyword("COMMON TABLE EXPRESSION");
            markup.AppendSpace();
            markup.AppendCommonTableExpressionName(symbol.Name);
        }

        private static void AppendVariableSymbolInfo(this ICollection<SymbolMarkupNode> markup, VariableSymbol symbol)
        {
            markup.AppendKeyword("VARIABLE");
            markup.AppendSpace();
            markup.AppendPunctuation("@");
            markup.AppendVariableName(symbol.Name);
            markup.AppendSpace();
            markup.AppendAsType(symbol.Type);
        }

        private static void AppendParameterSymbolInfo(this ICollection<SymbolMarkupNode> markup, ParameterSymbol symbol)
        {
            markup.AppendKeyword("PARAMETER");
            markup.AppendSpace();
            markup.AppendParameterName(symbol.Name);
            markup.AppendSpace();
            markup.AppendAsType(symbol.Type);
        }

        private static void AppendFunctionSymbolInfo(this ICollection<SymbolMarkupNode> markup, FunctionSymbol symbol)
        {
            markup.AppendKeyword("FUNCTION");
            markup.AppendSpace();
            markup.AppendFunctionName(symbol.Name);
            markup.AppendInvocable(symbol);
        }

        private static void AppendMethodSymbolInfo(this ICollection<SymbolMarkupNode> markup, MethodSymbol symbol)
        {
            markup.AppendKeyword("METHOD");
            markup.AppendSpace();
            markup.AppendMethodName(symbol.Name);
            markup.AppendInvocable(symbol);
        }

        private static void AppendPropertySymbolInfo(this ICollection<SymbolMarkupNode> markup, PropertySymbol symbol)
        {
            markup.AppendKeyword("PROPERTY");
            markup.AppendSpace();
            markup.AppendPropertyName(symbol.Name);
            markup.AppendSpace();
            markup.AppendAsType(symbol.Type);
        }

        private static void AppendAggregateSymbolInfo(this ICollection<SymbolMarkupNode> markup, AggregateSymbol symbol)
        {
            markup.AppendKeyword("AGGREGATE");
            markup.AppendSpace();
            markup.AppendAggregateName(symbol.Name);
            markup.AppendPunctuation("(");
            markup.AppendParameterName("value");
            markup.AppendPunctuation(")");
        }

        private static void AppendInvocable(this ICollection<SymbolMarkupNode> markup, InvocableSymbol symbol)
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