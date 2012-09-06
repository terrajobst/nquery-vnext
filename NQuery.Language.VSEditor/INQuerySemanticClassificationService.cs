using Microsoft.VisualStudio.Text.Classification;

namespace NQuery.Language.VSEditor
{
    public interface INQuerySemanticClassificationService
    {
        IClassificationType SchemaTable { get; }
        IClassificationType DerivedTable { get; }
        IClassificationType CteTable { get; } // TODO: Should we rename this?
        IClassificationType Column { get; }
        IClassificationType Method { get; }
        IClassificationType Property { get; }
        IClassificationType Function { get; }
        IClassificationType Aggregate { get; }
        IClassificationType Operator { get; }
        IClassificationType Variable { get; }
    }
}