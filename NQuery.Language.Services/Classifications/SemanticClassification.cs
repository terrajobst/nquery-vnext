using System;

namespace NQuery.Language.Services.Classifications
{
    public enum SemanticClassification
    {
        SchemaTable,
        Column,
        DerivedTable,
        CommonTableExpression,
        Function,
        Aggregate,
        Variable,
        Property,
        Method
    }
}