using System;

namespace NQuery.Authoring.Classifications
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