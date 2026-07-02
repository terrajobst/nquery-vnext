using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

using NQuery.Authoring.VSEditorWpf.Text;
using NQuery.CodeAnalysis.Text;

namespace NQuery.Authoring.VSEditorWpf;

public static class NQueryExtensions
{
    private static readonly object WorkspaceKey = new();

    extension(ITextBuffer textBuffer)
    {
        public Workspace GetWorkspace()
        {
            return textBuffer.Properties.GetOrCreateSingletonProperty(WorkspaceKey, () =>
            {
                var textContainer = new VisualStudioSourceTextContainer(textBuffer);
                return new Workspace(textContainer);
            });
        }

        public Document GetDocument()
        {
            var workspace = textBuffer.GetWorkspace();
            return workspace.CurrentDocument;
        }
    }

    extension(Document document)
    {
        public ITextSnapshot GetTextSnapshot()
        {
            return document.Text.ToTextSnapshot();
        }
    }

    extension(ITextView syntaxEditor)
    {
        public DocumentView GetDocumentView()
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
