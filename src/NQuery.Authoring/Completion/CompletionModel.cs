using System.Collections.Immutable;

using NQuery.CodeAnalysis;
using NQuery.CodeAnalysis.Text;

namespace NQuery.Authoring.Completion;

public sealed class CompletionModel
{
    public CompletionModel(SemanticModel semanticModel, TextSpan applicableSpan, IEnumerable<CompletionItem> items)
    {
        SemanticModel = semanticModel;
        ApplicableSpan = applicableSpan;
        Items = [.. items];
    }

    public SemanticModel SemanticModel { get; }

    public TextSpan ApplicableSpan { get; }

    public ImmutableArray<CompletionItem> Items { get; }
}
