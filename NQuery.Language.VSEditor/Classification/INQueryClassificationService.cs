using System;

using Microsoft.VisualStudio.Text.Classification;

namespace NQuery.Language.VSEditor.Classification
{
    public interface INQueryClassificationService
    {
        IClassificationType WhiteSpace { get; }
        IClassificationType Comment { get; }
        IClassificationType Identifier { get; }
        IClassificationType Keyword { get; }
        IClassificationType Punctuation { get; }
        IClassificationType NumberLiteral { get; }
        IClassificationType StringLiteral { get; }
        IClassificationType SchemaTable { get; }
        IClassificationType DerivedTable { get; }
        IClassificationType CommonTableExpression { get; }
        IClassificationType Column { get; }
        IClassificationType Method { get; }
        IClassificationType Property { get; }
        IClassificationType Function { get; }
        IClassificationType Aggregate { get; }
        IClassificationType Variable { get; }
    }
}