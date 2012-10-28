using System;
using System.Threading.Tasks;

using ActiproSoftware.Text;

namespace NQuery.Authoring.ActiproWpf
{
    public static class NQueryExtensions
    {
        public static NQueryParseData GetParseData(this ITextDocument document)
        {
            return document.CurrentSnapshot.GetParseData();
        }

        public static NQueryParseData GetParseData(this ITextSnapshot snapshot)
        {
            var document = snapshot.Document as ICodeDocument;
            if (document == null)
                return null;

            var parseData = document.ParseData as NQueryParseData;
            if (parseData == null)
                return null;

            if (parseData.Snapshot != snapshot)
                return null;

            return parseData;
        }

        public static NQuerySemanticData GetSemanticData(this ITextDocument document)
        {
            return document.CurrentSnapshot.GetSemanticData();
        }

        public static NQuerySemanticData GetSemanticData(this ITextSnapshot snapshot)
        {
            var document = snapshot.Document as NQueryDocument;
            if (document == null)
                return null;

            var parseData = snapshot.GetParseData();
            if (parseData == null)
                return null;

            var semanticData = document.SemanticData;
            if (semanticData == null || semanticData.ParseData != parseData)
                return null;

            return semanticData;
        }

        public static Task<NQueryParseData> GetParseDataAsync(this ITextDocument document)
        {
            var queryDocument = document as NQueryDocument;
            return queryDocument == null
                       ? Task.FromResult<NQueryParseData>(null)
                       : queryDocument.GetParseDataAsync();
        }

        public static Task<NQuerySemanticData> GetSemanticDataAsync(this ITextDocument document)
        {
            var queryDocument = document as NQueryDocument;
            return queryDocument == null
                       ? Task.FromResult<NQuerySemanticData>(null)
                       : queryDocument.GetSemanticDataAsync();
        }

        public static TextSnapshotOffset ToSnapshotOffset(this TextBuffer textBuffer, ITextSnapshot snapshot, int position)
        {
            var textLine = textBuffer.GetLineFromPosition(position);
            var line = textLine.Index;
            var character = position - textLine.TextSpan.Start;

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
            return line.TextSpan.Start + textPosition.Character;
        }

        public static TextSpan ToTextSpan(this TextSnapshotRange range, TextBuffer textBuffer)
        {
            var startOffset = new TextSnapshotOffset(range.Snapshot, range.StartOffset);
            var endOffset = new TextSnapshotOffset(range.Snapshot, range.EndOffset);
            var start = startOffset.ToOffset(textBuffer);
            var end = endOffset.ToOffset(textBuffer);
            return TextSpan.FromBounds(start, end);
        }
    }
}