using ActiproSoftware.Text;
using ActiproSoftware.Windows.Controls.SyntaxEditor;

using NQuery.Authoring.ActiproWpf.Text;
using NQuery.CodeAnalysis.Text;

namespace NQuery.Authoring.ActiproWpf;

public static class NQueryExtensions
{
    private static readonly object WorkspaceKey = new();

    extension(ICodeDocument codeDocument)
    {
        public Workspace GetWorkspace()
        {
            return codeDocument.Properties.GetOrCreateSingleton(WorkspaceKey, () =>
            {
                var textContainer = new ActiproSourceTextContainer(codeDocument);
                var workspace = new Workspace(textContainer);
                new NQueryParseDataSynchronizer(codeDocument, workspace);
                return workspace;
            });
        }
    }

    extension(ITextDocument textDocument)
    {
        public Document? GetDocument()
        {
            if (textDocument is not ICodeDocument codeDocument)
                return null;

            var workspace = codeDocument.GetWorkspace();
            return workspace.CurrentDocument;
        }
    }

    extension(SyntaxEditor syntaxEditor)
    {
        public DocumentView GetDocumentView()
        {
            var document = syntaxEditor.Document.GetDocument()!;
            var snapshot = document.Text.ToTextSnapshot();
            var offset = syntaxEditor.ActiveView.Selection.CaretOffset;
            var selection = syntaxEditor.ActiveView.Selection.SnapshotRange.TranslateTo(snapshot, TextRangeTrackingModes.Default).ToTextSpan();
            var position = new TextSnapshotOffset(snapshot, offset).ToOffset();
            return new DocumentView(document, position, selection);
        }
    }

    extension(TextSnapshotOffset offset)
    {
        public DocumentView GetDocumentView()
        {
            var position = offset.ToOffset();
            var document = offset.Snapshot.ToDocument();
            return new DocumentView(document, position);
        }

        public int ToOffset()
        {
            var sourceText = offset.Snapshot.ToSourceText();
            var textPosition = offset.Position;
            var line = sourceText.Lines[textPosition.Line];
            return line.Span.Start + textPosition.Character;
        }
    }

    extension(ITextSnapshot snapshot)
    {
        public Document ToDocument()
        {
            var sourceText = snapshot.ToSourceText();
            var document = snapshot.Document.GetDocument()!;
            if (document.Text != snapshot.ToSourceText())
                document = document.WithText(sourceText);

            return document;
        }
    }

    extension(TextSnapshotRange range)
    {
        public TextSpan ToTextSpan()
        {
            var startOffset = new TextSnapshotOffset(range.Snapshot, range.StartOffset);
            var endOffset = new TextSnapshotOffset(range.Snapshot, range.EndOffset);
            var start = startOffset.ToOffset();
            var end = endOffset.ToOffset();
            return TextSpan.FromBounds(start, end);
        }
    }

    extension(DocumentView snapshot)
    {
        public TextSnapshotOffset ToSnapshotOffset(int position)
        {
            return snapshot.Text.ToSnapshotOffset(position);
        }

        public TextSnapshotRange ToSnapshotRange(TextSpan span)
        {
            return snapshot.Text.ToSnapshotRange(span);
        }
    }

    extension(SourceText text)
    {
        public TextSnapshotOffset ToSnapshotOffset(int position)
        {
            var snapshot = text.ToTextSnapshot();
            var offset = text.ToOffset(position);
            return new TextSnapshotOffset(snapshot, offset);
        }

        public int ToOffset(int position)
        {
            var textLine = text.GetLineFromPosition(position);
            var line = textLine.LineNumber;
            var delta = Enumerable.Range(0, line).Select(i => text.Lines[i].LineBreakLength - 1).Sum();
            var offset = position - delta;
            return offset;
        }

        public TextSnapshotRange ToSnapshotRange(TextSpan span)
        {
            var snapshot = text.ToTextSnapshot();
            var range = text.ToRange(span);
            return new TextSnapshotRange(snapshot, range);
        }

        public TextRange ToRange(TextSpan span)
        {
            var start = text.ToOffset(span.Start);
            var end = text.ToOffset(span.End);
            return new TextRange(start, end);
        }
    }
}
