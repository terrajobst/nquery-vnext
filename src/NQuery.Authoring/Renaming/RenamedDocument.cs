using System;
using System.Collections.Immutable;

using NQuery.Text;

namespace NQuery.Authoring.Renaming
{
    public sealed class RenamedDocument
    {
        private readonly Document _document;
        private readonly ImmutableArray<TextChange> _changes;
        private readonly ImmutableArray<TextSpan> _locations;

        public RenamedDocument(Document document, ImmutableArray<TextChange> changes, ImmutableArray<TextSpan> locations)
        {
            _document = document;
            _changes = changes;
            _locations = locations;
        }

        public Document Document
        {
            get { return _document; }
        }

        public ImmutableArray<TextChange> Changes
        {
            get { return _changes; }
        }

        public ImmutableArray<TextSpan> Locations
        {
            get { return _locations; }
        }
    }
}