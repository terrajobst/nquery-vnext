using System;
using System.Collections.Generic;
using System.Collections.Immutable;

using NQuery.Text;

namespace NQuery.Authoring.Completion
{
    public sealed class CompletionModel
    {
        public CompletionModel(SemanticModel semanticModel, TextSpan applicableSpan, IEnumerable<CompletionItem> items)
        {
            SemanticModel = semanticModel;
            ApplicableSpan = applicableSpan;
            Items = items.ToImmutableArray();
        }

        public SemanticModel SemanticModel { get; }

        public TextSpan ApplicableSpan { get; }

        public ImmutableArray<CompletionItem> Items { get; }
    }
}