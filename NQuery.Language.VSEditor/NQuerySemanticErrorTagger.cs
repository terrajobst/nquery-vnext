using System;
using System.Collections.Generic;

using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Tagging;

namespace NQuery.Language.VSEditor
{
    internal sealed class NQuerySemanticErrorTagger : ITagger<IErrorTag>
    {
        private readonly ITextBuffer _textBuffer;
        private readonly INQuerySemanticModelManager _semanticModelManager;

        public NQuerySemanticErrorTagger(ITextBuffer textBuffer, INQuerySemanticModelManager semanticModelManager)
        {
            _textBuffer = textBuffer;
            _semanticModelManager = semanticModelManager;
            _semanticModelManager.SemanticModelChanged += SemanticModelManagerOnSemanticModelChanged;
        }

        private void SemanticModelManagerOnSemanticModelChanged(object sender, EventArgs eventArgs)
        {
            var snapshot = _textBuffer.CurrentSnapshot;
            var snapshotSpan = new SnapshotSpan(snapshot, 0, snapshot.Length);
            OnTagsChanged(new SnapshotSpanEventArgs(snapshotSpan));
        }

        public IEnumerable<ITagSpan<IErrorTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            var result = new List<ITagSpan<IErrorTag>>();

            var semanticModel = _semanticModelManager.SemanticModel;
            if (semanticModel != null)
            {
                var diagnostics = semanticModel.GetDiagnostics();

                var snapshot = _textBuffer.CurrentSnapshot;

                foreach (var diagnostic in diagnostics)
                {
                    var span = new Span(diagnostic.NodeOrToken.Span.Start, diagnostic.NodeOrToken.Span.Length);
                    var snapshotSpan = new SnapshotSpan(snapshot, span);
                    var errorTag = new ErrorTag(PredefinedErrorTypeNames.CompilerError, diagnostic.Message);
                    var errorTagSpan = new TagSpan<IErrorTag>(snapshotSpan, errorTag);
                    result.Add(errorTagSpan);
                }
            }

            return result;
        }

        private void OnTagsChanged(SnapshotSpanEventArgs e)
        {
            var handler = TagsChanged;
            if (handler != null) handler(this, e);
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;
    }
}