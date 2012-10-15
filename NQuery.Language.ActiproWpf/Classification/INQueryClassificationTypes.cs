using ActiproSoftware.Text;

namespace NQueryViewerActiproWpf
{
    public interface INQueryClassificationTypes
    {
        IClassificationType WhiteSpace { get; }
        IClassificationType Comment { get; }
        IClassificationType Keyword { get; }
        IClassificationType Punctuation { get; }
        IClassificationType Identifier { get; }
        IClassificationType StringLiteral { get; }
        IClassificationType NumberLiteral { get; }
        IClassificationType SchemaTable { get; }
        IClassificationType DerivedTable { get; }
        IClassificationType CommonTableExpression { get; }
        IClassificationType Column { get; }
        IClassificationType Method { get; }
        IClassificationType Property { get; }
        IClassificationType Function { get; }
        IClassificationType Aggregate { get; }
        IClassificationType Operator { get; }
        IClassificationType Variable { get; }
    }
}