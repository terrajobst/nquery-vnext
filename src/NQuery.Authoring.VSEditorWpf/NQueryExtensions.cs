using System;

using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

using NQuery.Authoring.VSEditorWpf.Text;
using NQuery.Text;

namespace NQuery.Authoring.VSEditorWpf
{
    public static class NQueryExtensions
    {
        private static readonly object WorkspaceKey = new object();

        public static Workspace GetWorkspace(this ITextBuffer textBuffer)
        {
            return textBuffer.Properties.GetOrCreateSingletonProperty(WorkspaceKey, () =>
            {
                var textContainer = new VisualStudioSourceTextContainer(textBuffer);
                return new Workspace(textContainer);
            });
        }

        public static ITextSnapshot GetTextSnapshot(this Document document)
        {
            return document.Text.ToTextSnapshot();
        }

        public static Document GetDocument(this ITextBuffer textBuffer)
        {
            var workspace = textBuffer.GetWorkspace();
            return workspace.CurrentDocument;
        }

        public static DocumentView GetDocumentView(this ITextView syntaxEditor)
        {
            var document = syntaxEditor.TextBuffer.GetDocument();
            var snapshot = document.Text.ToTextSnapshot();
            var start = syntaxEditor.Selection.Start.Position.TranslateTo(snapshot, PointTrackingMode.Negative);
            var end = syntaxEditor.Selection.End.Position.TranslateTo(snapshot, PointTrackingMode.Negative);
            var selection = TextSpan.FromBounds(start.Position, end.Position);
            var position = syntaxEditor.Caret.Position.BufferPosition.TranslateTo(snapshot, PointTrackingMode.Negative);
            return new DocumentView(document, position, selection);
        }
    }
}