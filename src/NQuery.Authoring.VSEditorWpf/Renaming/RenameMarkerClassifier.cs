using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;

using NQuery.Authoring.VSEditorWpf.Classification;
using NQuery.Text;

namespace NQuery.Authoring.VSEditorWpf.Renaming
{
    internal sealed class RenameMarkerClassifier : AsyncTagger<IClassificationTag, TextSpan>
    {
        private readonly ITextBuffer _textBuffer;
        private readonly INQueryClassificationService _classificationService;
        private readonly IRenameService _renameService;

        private IRenameSession _activeSession;

        public RenameMarkerClassifier(ITextBuffer textBuffer, INQueryClassificationService classificationService, IRenameService renameService)
        {
            _textBuffer = textBuffer;
            _classificationService = classificationService;
            _renameService = renameService;
            _renameService.ActiveSessionChanged += RenameServiceOnActiveSessionChanged;
            InvalidateTags();
        }

        private void RenameServiceOnActiveSessionChanged(object sender, EventArgs e)
        {
            if (_activeSession != null)
                _activeSession.LocationsChanged -= CurrentSessionOnLocationsChanged;

            _activeSession = _renameService.ActiveSession;

            if (_activeSession != null)
                _activeSession.LocationsChanged += CurrentSessionOnLocationsChanged;

            InvalidateTags();
        }

        private void CurrentSessionOnLocationsChanged(object sender, EventArgs e)
        {
            InvalidateTags();
        }

        protected override Task<Tuple<ITextSnapshot, IEnumerable<TextSpan>>> GetRawTagsAsync()
        {
            if (_activeSession == null || _activeSession.Locations.Length == 0)
                return Task.FromResult(Tuple.Create(_textBuffer.CurrentSnapshot, Enumerable.Empty<TextSpan>()));

            var snapshot = _activeSession.Locations.First().Snapshot;
            var spans = _activeSession.Locations.Select(l => new TextSpan(l.Start, l.Length));
            return Task.FromResult(Tuple.Create(snapshot, spans));
        }

        protected override ITagSpan<IClassificationTag> CreateTagSpan(ITextSnapshot snapshot, TextSpan rawTag)
        {
            var snapshotSpan = new SnapshotSpan(snapshot, rawTag.Start, rawTag.Length);
            var tag = new ClassificationTag(_classificationService.RenameLocation);
            return new TagSpan<IClassificationTag>(snapshotSpan, tag);
        }
    }
}