using NQuery.Authoring.LanguageServer.Protocol;
using NQuery.CodeAnalysis.Text;

using Range = NQuery.Authoring.LanguageServer.Protocol.Range;

namespace NQuery.Authoring.LanguageServer.Mapping;

// LSP positions are (line, character) pairs; NQuery works in absolute offsets. Character
// offsets are UTF-16 code units, which is what SourceText already indexes by, so no encoding
// conversion is involved -- see PositionEncodingKind.
//
// Everything here clamps rather than throws: clients legitimately send positions computed
// against a slightly stale document version, and a crashed request is worse than a request
// answered against the nearest valid position.
internal static class PositionMapping
{
    extension(SourceText text)
    {
        public Position ToPosition(int position)
        {
            var clamped = Math.Clamp(position, 0, text.Length);
            var location = text.GetTextLocation(clamped);
            return new Position { Line = location.Line, Character = location.Column };
        }

        public int ToOffset(Position position)
        {
            ThrowIfNull(position);

            if (position.Line < 0)
                return 0;

            var lines = text.Lines;
            if (position.Line >= lines.Count)
                return text.Length;

            var line = lines[position.Line];
            var character = Math.Clamp(position.Character, 0, line.Span.Length);
            return line.Span.Start + character;
        }

        public Range ToRange(TextSpan span)
        {
            return new Range
            {
                Start = text.ToPosition(span.Start),
                End = text.ToPosition(span.End)
            };
        }

        public TextSpan ToTextSpan(Range range)
        {
            ThrowIfNull(range);

            var start = text.ToOffset(range.Start);
            var end = text.ToOffset(range.End);
            return start <= end
                ? TextSpan.FromBounds(start, end)
                : TextSpan.FromBounds(end, start);
        }
    }
}
