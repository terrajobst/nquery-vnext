using ActiproSoftware.Text;
using NQuery.Language;

namespace NQueryViewerActiproWpf
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

        private static SemanticModel GetSemanticModel(this ITextDocument document)
        {
            var nqueryDocument = document as NQueryDocument;
            return nqueryDocument == null
                       ? null
                       : nqueryDocument.SemanticModel;
        }

        private static NQuerySemanticData GetSemanticData(NQueryParseData parseData, SemanticModel semanticModel)
        {
            return parseData == null || semanticModel == null || parseData.SyntaxTree != semanticModel.Compilation.SyntaxTree
                       ? null
                       : new NQuerySemanticData(parseData, semanticModel);
        }

        public static NQuerySemanticData GetSemanticData(this ITextDocument document)
        {
            var parseData = document.CurrentSnapshot.GetParseData();
            var semanticModel = document.GetSemanticModel();
            return GetSemanticData(parseData, semanticModel);
        }

        public static NQuerySemanticData GetSemanticData(this ITextSnapshot snapshot)
        {
            var parseData = snapshot.GetParseData();
            var semanticModel = snapshot.Document.GetSemanticModel();
            return GetSemanticData(parseData, semanticModel);
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