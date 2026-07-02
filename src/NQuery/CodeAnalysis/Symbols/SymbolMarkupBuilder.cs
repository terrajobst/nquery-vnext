using NQuery.CodeAnalysis.Symbols;

namespace NQuery.CodeAnalysis.Symbols;

internal static class SymbolMarkupBuilder
{
    extension(ICollection<SymbolMarkupToken> markup)
    {
        public void AppendSymbol(Symbol symbol)
        {
            switch (symbol.Kind)
            {
                case SymbolKind.Column:
                    markup.AppendColumnSymbolInfo((ColumnSymbol)symbol);
                    break;
                case SymbolKind.Table:
                    markup.AppendTableSymbolInfo((TableSymbol)symbol);
                    break;
                case SymbolKind.TableInstance:
                    markup.AppendTableInstanceSymbolInfo((TableInstanceSymbol)symbol);
                    break;
                case SymbolKind.ColumnInstance:
                    markup.AppendColumnInstanceSymbolInfo((ColumnInstanceSymbol)symbol);
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
                    throw ExceptionBuilder.UnexpectedValue(symbol.Kind);
            }
        }

        public void AppendCastSymbol()
        {
            markup.AppendKeyword(@"CAST");
            markup.AppendPunctuation(@"(");
            markup.AppendParameterName(@"expression");
            markup.AppendSpace();
            markup.AppendKeyword(@"AS");
            markup.AppendSpace();
            markup.AppendParameterName(@"dataType");
            markup.AppendPunctuation(@")");
        }

        public void AppendCoalesceSymbol()
        {
            markup.AppendKeyword(@"COALESCE");
            markup.AppendPunctuation(@"(");
            markup.AppendParameterName(@"expression1");
            markup.AppendPunctuation(@",");
            markup.AppendSpace();
            markup.AppendParameterName(@"expression2");
            markup.AppendPunctuation(@",");
            markup.AppendSpace();
            markup.AppendParameterName(@"[, ...]");
            markup.AppendPunctuation(@")");
        }

        public void AppendNullIfSymbol()
        {
            markup.AppendKeyword(@"NULLIF");
            markup.AppendPunctuation(@"(");
            markup.AppendParameterName(@"expression1");
            markup.AppendPunctuation(@",");
            markup.AppendSpace();
            markup.AppendParameterName(@"expression2");
            markup.AppendPunctuation(@")");
        }

        private void Append(SymbolMarkupKind kind, string text)
        {
            markup.Add(new SymbolMarkupToken(kind, text));
        }

        private void AppendName(SymbolMarkupKind kind, string name)
        {
            var displayName = string.IsNullOrEmpty(name)
                                  ? @"?"
                                  : SyntaxFacts.GetValidIdentifier(name);
            markup.Append(kind, displayName);
        }

        private void AppendKeyword(string text)
        {
            markup.Append(SymbolMarkupKind.Keyword, text);
        }

        private void AppendSpace()
        {
            markup.Append(SymbolMarkupKind.Whitespace, @" ");
        }

        private void AppendTableName(string text)
        {
            markup.AppendName(SymbolMarkupKind.TableName, text);
        }

        private void AppendCommonTableExpressionName(string text)
        {
            markup.AppendName(SymbolMarkupKind.CommonTableExpressionName, text);
        }

        private void AppendColumnName(string text)
        {
            markup.AppendName(SymbolMarkupKind.ColumnName, text);
        }

        private void AppendVariableName(string text)
        {
            markup.AppendName(SymbolMarkupKind.VariableName, text);
        }

        private void AppendParameterName(string text)
        {
            markup.AppendName(SymbolMarkupKind.ParameterName, text);
        }

        private void AppendFunctionName(string text)
        {
            markup.AppendName(SymbolMarkupKind.FunctionName, text);
        }

        private void AppendAggregateName(string text)
        {
            markup.AppendName(SymbolMarkupKind.AggregateName, text);
        }

        private void AppendMethodName(string text)
        {
            markup.AppendName(SymbolMarkupKind.MethodName, text);
        }

        private void AppendPropertyName(string text)
        {
            markup.AppendName(SymbolMarkupKind.PropertyName, text);
        }

        private void AppendPunctuation(string text)
        {
            markup.Append(SymbolMarkupKind.Punctuation, text);
        }

        private void AppendType(Type type)
        {
            markup.AppendName(SymbolMarkupKind.TypeName, type.Name.ToUpper());
        }

        private void AppendAsType(Type type)
        {
            markup.AppendKeyword(@"AS");
            markup.AppendSpace();
            markup.AppendType(type);
        }

        private void AppendTableSymbolInfo(TableSymbol symbol)
        {
            switch (symbol.TableKind)
            {
                case TableKind.SchemaTable:
                    markup.AppendSchemaTableSymbolInfo((SchemaTableSymbol)symbol);
                    break;
                case TableKind.DerivedTable:
                    markup.AppendDerivedTableSymbolInfo((DerivedTableSymbol)symbol);
                    break;
                case TableKind.CommonTableExpression:
                    markup.AppendCommonTableExpressionSymbolInfo((CommonTableExpressionSymbol)symbol);
                    break;
                default:
                    throw ExceptionBuilder.UnexpectedValue(symbol.TableKind);
            }
        }

        private void AppendColumnInstanceSymbolInfo(ColumnInstanceSymbol symbol)
        {
            switch (symbol.ColumnInstanceKind)
            {
                case ColumnInstanceKind.TableColumn:
                    markup.AppendTableColumnInstanceSymbolInfo((TableColumnInstanceSymbol)symbol);
                    break;
                case ColumnInstanceKind.QueryColumn:
                    markup.AppendQueryColumnInstanceSymbolInfo((QueryColumnInstanceSymbol)symbol);
                    break;
                default:
                    throw ExceptionBuilder.UnexpectedValue(symbol.ColumnInstanceKind);
            }
        }

        private void AppendColumnSymbolInfo(ColumnSymbol symbol)
        {
            markup.AppendKeyword(@"COLUMN");
            markup.AppendSpace();
            markup.AppendColumnName(symbol.Name);
            markup.AppendSpace();
            markup.AppendAsType(symbol.Type);
        }

        private void AppendSchemaTableSymbolInfo(SchemaTableSymbol symbol)
        {
            markup.AppendKeyword(@"TABLE");
            markup.AppendSpace();
            markup.AppendTableName(symbol.Name);
            if (!symbol.Type.IsMissing())
            {
                markup.AppendSpace();
                markup.AppendAsType(symbol.Type);
            }
        }

        private void AppendDerivedTableSymbolInfo(DerivedTableSymbol symbol)
        {
            markup.AppendKeyword(@"DERIVED");
            markup.AppendSpace();
            markup.AppendKeyword(@"TABLE");
        }

        private void AppendTableInstanceSymbolInfo(TableInstanceSymbol symbol)
        {
            markup.AppendKeyword(@"ALIAS");
            markup.AppendSpace();
            markup.AppendTableName(symbol.Name);
            markup.AppendSpace();
            markup.AppendKeyword(@"FOR");
            markup.AppendSpace();
            markup.AppendSymbol(symbol.Table);
        }

        private void AppendTableColumnInstanceSymbolInfo(TableColumnInstanceSymbol symbol)
        {
            markup.AppendKeyword(@"COLUMN");
            markup.AppendSpace();
            markup.AppendTableName(symbol.TableInstance.Name);
            markup.AppendPunctuation(@".");
            markup.AppendColumnName(symbol.Name);
            markup.AppendSpace();
            markup.AppendAsType(symbol.Type);
            markup.AppendSpace();
            markup.AppendKeyword(@"OF");
            markup.AppendSpace();
            markup.AppendSymbol(symbol.TableInstance.Table);
        }

        private void AppendQueryColumnInstanceSymbolInfo(QueryColumnInstanceSymbol symbol)
        {
            markup.AppendKeyword(@"COLUMN");
            markup.AppendSpace();
            markup.AppendColumnName(symbol.Name);
            markup.AppendSpace();
            markup.AppendAsType(symbol.Type);
        }

        private void AppendCommonTableExpressionSymbolInfo(CommonTableExpressionSymbol symbol)
        {
            markup.AppendKeyword(@"COMMON");
            markup.AppendSpace();
            markup.AppendKeyword(@"TABLE");
            markup.AppendSpace();
            markup.AppendKeyword(@"EXPRESSION");
            markup.AppendSpace();
            markup.AppendCommonTableExpressionName(symbol.Name);
        }

        private void AppendVariableSymbolInfo(VariableSymbol symbol)
        {
            markup.AppendKeyword(@"VARIABLE");
            markup.AppendSpace();
            markup.AppendPunctuation(@"@");
            markup.AppendVariableName(symbol.Name);
            markup.AppendSpace();
            markup.AppendAsType(symbol.Type);
        }

        private void AppendParameterSymbolInfo(ParameterSymbol symbol)
        {
            markup.AppendKeyword(@"PARAMETER");
            markup.AppendSpace();
            markup.AppendParameterName(symbol.Name);
            markup.AppendSpace();
            markup.AppendAsType(symbol.Type);
        }

        private void AppendFunctionSymbolInfo(FunctionSymbol symbol)
        {
            markup.AppendKeyword(@"FUNCTION");
            markup.AppendSpace();
            markup.AppendFunctionName(symbol.Name);
            markup.AppendInvocable(symbol);
        }

        private void AppendMethodSymbolInfo(MethodSymbol symbol)
        {
            markup.AppendKeyword(@"METHOD");
            markup.AppendSpace();
            markup.AppendMethodName(symbol.Name);
            markup.AppendInvocable(symbol);
        }

        private void AppendPropertySymbolInfo(PropertySymbol symbol)
        {
            markup.AppendKeyword(@"PROPERTY");
            markup.AppendSpace();
            markup.AppendPropertyName(symbol.Name);
            markup.AppendSpace();
            markup.AppendAsType(symbol.Type);
        }

        private void AppendAggregateSymbolInfo(AggregateSymbol symbol)
        {
            markup.AppendKeyword(@"AGGREGATE");
            markup.AppendSpace();
            markup.AppendAggregateName(symbol.Name);
            markup.AppendPunctuation(@"(");
            markup.AppendParameterName(@"value");
            markup.AppendPunctuation(@")");
        }

        private void AppendInvocable(IInvocableSymbol symbol)
        {
            markup.AppendPunctuation(@"(");

            var isFirst = true;
            foreach (var parameterSymbol in symbol.Parameters)
            {
                if (isFirst)
                    isFirst = false;
                else
                {
                    markup.AppendPunctuation(@",");
                    markup.AppendSpace();
                }

                markup.AppendParameterName(parameterSymbol.Name);
                markup.AppendSpace();
                markup.AppendAsType(parameterSymbol.Type);
            }

            markup.AppendPunctuation(@")");
            markup.AppendSpace();
            markup.AppendAsType(symbol.ReturnType);
        }
    }
}
