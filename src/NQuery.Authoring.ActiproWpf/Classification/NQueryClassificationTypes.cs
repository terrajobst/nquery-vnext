using System;

using ActiproSoftware.Text;
using ActiproSoftware.Text.Implementation;

namespace NQuery.Authoring.ActiproWpf.Classification
{
    internal sealed class NQueryClassificationTypes : INQueryClassificationTypes
    {
        public IClassificationType WhiteSpace { get; } = new ClassificationType("whiteSpace");

        public IClassificationType Comment { get; } = new ClassificationType("comment");

        public IClassificationType Keyword { get; } = new ClassificationType("keyword");

        public IClassificationType Punctuation { get; } = new ClassificationType("punctuation");

        public IClassificationType Identifier { get; } = new ClassificationType("identifier");

        public IClassificationType StringLiteral { get; } = new ClassificationType("stringLiteral");

        public IClassificationType NumberLiteral { get; } = new ClassificationType("numberLiteral");

        public IClassificationType SchemaTable { get; } = new ClassificationType("schemaTable");

        public IClassificationType DerivedTable { get; } = new ClassificationType("derivedTable");

        public IClassificationType CommonTableExpression { get; } = new ClassificationType("commonTableExpression");

        public IClassificationType Column { get; } = new ClassificationType("column");

        public IClassificationType Method { get; } = new ClassificationType("method");

        public IClassificationType Property { get; } = new ClassificationType("property");

        public IClassificationType Function { get; } = new ClassificationType("function");

        public IClassificationType Aggregate { get; } = new ClassificationType("aggregate");

        public IClassificationType Operator { get; } = new ClassificationType("operator");

        public IClassificationType Variable { get; } = new ClassificationType("variable");

        public IClassificationType Unnecessary { get; } = new ClassificationType("unnecessary");
    }
}