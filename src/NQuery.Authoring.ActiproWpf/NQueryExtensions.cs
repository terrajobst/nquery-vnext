using System;
using System.Threading.Tasks;

using ActiproSoftware.Text;

using NQuery.Authoring.ActiproWpf.Text;
using NQuery.Authoring.Document;
using NQuery.Text;

namespace NQuery.Authoring.ActiproWpf
{
    public static class NQueryExtensions
    {
        private static readonly object NQueryDocumentKey = new object();

        public static NQueryDocument GetNQueryDocument(this ICodeDocument codeDocument)
        {
            return codeDocument.Properties.GetOrCreateSingleton(NQueryDocumentKey, () =>
            {
                var provider = new SnapshotTextBufferPovider(codeDocument);
                var document = new NQueryDocument(provider);
                new NQueryParseDataSynchronizer(codeDocument, document);
                return document;
            });
        }

        public static NQueryDocument GetNQueryDocument(this ITextDocument textDocument)
        {
            var codeDocument = textDocument as ICodeDocument;
            return codeDocument == null ? null : codeDocument.GetNQueryDocument();
        }

        public static Task<SyntaxTree> GetSyntaxTreeAsync(this ITextDocument textDocument)
        {
            var document = textDocument.GetNQueryDocument();
            return document == null ? Task.FromResult<SyntaxTree>(null) : document.GetSyntaxTreeAsync();
        }

        public static Task<SemanticModel> GetSemanticModelAsync(this ITextDocument textDocument)
        {
            var document = textDocument.GetNQueryDocument();
            return document == null ? Task.FromResult<SemanticModel>(null) : document.GetSemanticModelAsync();
        }

        public static TextSnapshotOffset ToSnapshotOffset(this TextBuffer textBuffer, ITextSnapshot snapshot, int position)
        {
            var textLine = textBuffer.GetLineFromPosition(position);
            var line = textLine.Index;
            var character = position - textLine.Span.Start;

            var textPosition = new TextPosition(line, character);
            var offset = snapshot.PositionToOffset(textPosition);
            return new TextSnapshotOffset(snapshot, offset);
        }

        public static TextSnapshotRange ToSnapshotRange(this TextBuffer textBuffer, ITextSnapshot snapshot, TextSpan span)
        {
            var start = textBuffer.ToSnapshotOffset(snapshot, span.Start);
            var end = textBuffer.ToSnapshotOffset(snapshot, span.End);
            return new TextSnapshotRange(snapshot, start.Offset, end.Offset);
        }

        public static int ToOffset(this TextSnapshotOffset offset, TextBuffer textBuffer)
        {
            var textPosition = offset.Position;
            var line = textBuffer.Lines[textPosition.Line];
            return line.Span.Start + textPosition.Character;
        }

        public static TextSpan ToTextSpan(this TextSnapshotRange range, TextBuffer textBuffer)
        {
            var startOffset = new TextSnapshotOffset(range.Snapshot, range.StartOffset);
            var endOffset = new TextSnapshotOffset(range.Snapshot, range.EndOffset);
            var start = startOffset.ToOffset(textBuffer);
            var end = endOffset.ToOffset(textBuffer);
            return TextSpan.FromBounds(start, end);
        }

        public static ITextSnapshot GetTextSnapshot(this SyntaxTree syntaxTree)
        {
            var textBuffer = syntaxTree.TextBuffer as SnapshotTextBuffer;
            return textBuffer == null ? null : textBuffer.Snapshot;
        }
    }
}