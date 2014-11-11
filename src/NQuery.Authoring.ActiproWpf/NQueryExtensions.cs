using System;

using ActiproSoftware.Text;
using ActiproSoftware.Windows.Controls.SyntaxEditor;

using NQuery.Authoring.ActiproWpf.Text;
using NQuery.Text;

namespace NQuery.Authoring.ActiproWpf
{
    public static class NQueryExtensions
    {
        private static readonly object WorkspaceKey = new object();

        public static Workspace GetWorkspace(this ICodeDocument codeDocument)
        {
            return codeDocument.Properties.GetOrCreateSingleton(WorkspaceKey, () =>
            {
                var textContainer = new ActiproSourceTextContainer(codeDocument);
                var workspace = new Workspace(textContainer);
                new NQueryParseDataSynchronizer(codeDocument, workspace);
                return workspace;
            });
        }

        public static Document GetDocument(this ITextDocument textDocument)
        {
            var codeDocument = textDocument as ICodeDocument;
            if (codeDocument == null)
                return null;

            var workspace = codeDocument.GetWorkspace();
            return workspace.CurrentDocument;
        }

        public static DocumentView GetDocumentView(this SyntaxEditor syntaxEditor)
        {
            var document = syntaxEditor.Document.GetDocument();
            var snapshot = document.Text.ToTextSnapshot();
            var offset = syntaxEditor.Caret.Offset;
            var selection = syntaxEditor.ActiveView.Selection.SnapshotRange.TranslateTo(snapshot, TextRangeTrackingModes.Default).ToTextSpan();
            var position = new TextSnapshotOffset(snapshot, offset).ToOffset();
            return new DocumentView(document, position, selection);
        }

        public static DocumentView GetDocumentView(this TextSnapshotOffset offset)
        {
            var position = offset.ToOffset();
            var document = offset.Snapshot.ToDocument();
            return new DocumentView(document, position);
        }

        public static Document ToDocument(this ITextSnapshot snapshot)
        {
            var sourceText = snapshot.ToSourceText();
            var document = snapshot.Document.GetDocument();
            if (document.Text != snapshot.ToSourceText())
                document = document.WithText(sourceText);

            return document;
        }

        public static int ToOffset(this TextSnapshotOffset offset)
        {
            var sourceText = offset.Snapshot.ToSourceText();
            var textPosition = offset.Position;
            var line = sourceText.Lines[textPosition.Line];
            return line.Span.Start + textPosition.Character;
        }

        public static TextSpan ToTextSpan(this TextSnapshotRange range)
        {
            var startOffset = new TextSnapshotOffset(range.Snapshot, range.StartOffset);
            var endOffset = new TextSnapshotOffset(range.Snapshot, range.EndOffset);
            var start = startOffset.ToOffset();
            var end = endOffset.ToOffset();
            return TextSpan.FromBounds(start, end);
        }

        public static TextSnapshotOffset ToSnapshotOffset(this DocumentView snapshot, int position)
        {
            return snapshot.Text.ToSnapshotOffset(position);
        }

        public static TextSnapshotRange ToSnapshotRange(this DocumentView snapshot, TextSpan span)
        {
            return snapshot.Text.ToSnapshotRange(span);
        }

        public static TextSnapshotOffset ToSnapshotOffset(this SourceText text, int position)
        {
            var snapshot = text.ToTextSnapshot();

            var textLine = text.GetLineFromPosition(position);
            var line = textLine.LineNumber;
            var character = position - textLine.Span.Start;

            var textPosition = new TextPosition(line, character);
            var offset = snapshot.PositionToOffset(textPosition);
            return new TextSnapshotOffset(snapshot, offset);
        }

        public static TextSnapshotRange ToSnapshotRange(this SourceText text, TextSpan span)
        {
            var snapshot = text.ToTextSnapshot();
            var start = text.ToSnapshotOffset(span.Start);
            var end = text.ToSnapshotOffset(span.End);
            return new TextSnapshotRange(snapshot, start.Offset, end.Offset);
        }
    }
}