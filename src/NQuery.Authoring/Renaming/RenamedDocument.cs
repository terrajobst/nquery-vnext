using System;
using System.Collections.Immutable;

using NQuery.Text;

namespace NQuery.Authoring.Renaming
{
    public sealed class RenamedDocument
    {
        private readonly Document _document;
        private readonly ImmutableArray<TextSpan> _locations;

        public RenamedDocument(Document document, ImmutableArray<TextSpan> locations)
        {
            _document = document;
            _locations = locations;
        }

        public Document Document
        {
            get { return _document; }
        }

        public ImmutableArray<TextSpan> Locations
        {
            get { return _locations; }
        }
    }
}