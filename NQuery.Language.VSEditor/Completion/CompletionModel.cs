using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace NQuery.Language.VSEditor.Completion
{
    public sealed class CompletionModel
    {
        private readonly SemanticModel _semanticModel;
        private readonly TextSpan _applicableSpan;
        private readonly ReadOnlyCollection<CompletionItem> _items;

        public CompletionModel(SemanticModel semanticModel, TextSpan applicableSpan, IList<CompletionItem> items)
        {
            _semanticModel = semanticModel;
            _applicableSpan = applicableSpan;
            _items = new ReadOnlyCollection<CompletionItem>(items);
        }

        public SemanticModel SemanticModel
        {
            get { return _semanticModel; }
        }

        public TextSpan ApplicableSpan
        {
            get { return _applicableSpan; }
        }

        public ReadOnlyCollection<CompletionItem> Items
        {
            get { return _items; }
        }
    }
}