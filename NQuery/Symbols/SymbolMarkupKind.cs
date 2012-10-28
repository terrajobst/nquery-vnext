using System;

namespace NQuery.Symbols
{
    public enum SymbolMarkupKind
    {
        Whitespace,
        Punctuation,
        Keyword,
        TableName,
        DerivedTableName,
        CommonTableExpressionName,
        ColumnName,
        VariableName,
        ParameterName,
        FunctionName,
        AggregateName,
        MethodName,
        PropertyName,
        TypeName
    }
}