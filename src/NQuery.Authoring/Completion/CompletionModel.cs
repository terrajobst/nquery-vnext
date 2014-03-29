using System;
using System.Collections.Generic;
using System.Collections.Immutable;

using NQuery.Text;

namespace NQuery.Authoring.Completion
{
    public sealed class CompletionModel
    {
        private readonly SemanticModel _semanticModel;
        private readonly TextSpan _applicableSpan;
        private readonly ImmutableArray<CompletionItem> _items;

        public CompletionModel(SemanticModel semanticModel, TextSpan applicableSpan, IEnumerable<CompletionItem> items)
        {
            _semanticModel = semanticModel;
            _applicableSpan = applicableSpan;
            _items = items.ToImmutableArray();
        }

        public SemanticModel SemanticModel
        {
            get { return _semanticModel; }
        }

        public TextSpan ApplicableSpan
        {
            get { return _applicableSpan; }
        }

        public ImmutableArray<CompletionItem> Items
        {
            get { return _items; }
        }
    }
}