using System.Collections.Immutable;
using System.Text;

namespace NQuery.Text
{
    public abstract class SourceText
    {
        private SourceTextContainer _container;

        protected SourceText()
            : this(null)
        {
        }

        protected SourceText(SourceTextContainer container)
        {
            _container = container;
        }

        public static SourceText From(string text)
        {
            ArgumentNullException.ThrowIfNull(text);

            return new StringText(text);
        }

        public SourceTextContainer Container
        {
            get
            {
                if (_container is null)
                {
                    var container = new StaticSourceTextContainer(this);
                    Interlocked.CompareExchange(ref _container, container, null);
                }
                return _container;
            }
        }

        public TextLine GetLineFromPosition(int position)
        {
            if (position < 0 || position > Length)
                throw new ArgumentOutOfRangeException(nameof(position));

            var lineNumber = GetLineNumberFromPosition(position);
            return Lines[lineNumber];
        }

        public abstract int GetLineNumberFromPosition(int position);

        public TextLocation GetTextLocation(int position)
        {
            var line = GetLineFromPosition(position);
            var lineNumber = line.LineNumber;
            var column = position - line.Span.Start;
            return new TextLocation(lineNumber, column);
        }

        public int GetPosition(TextLocation location)
        {
            var textLine = Lines[location.Line];
            return textLine.Span.Start + location.Column;
        }

        public abstract string GetText(TextSpan textSpan);

        public string GetText(int position, int length)
        {
            return GetText(new TextSpan(position, length));
        }

        public string GetText(int position)
        {
            var remaining = Length - position;
            return GetText(position, remaining);
        }

        public string GetText()
        {
            return GetText(0, Length);
        }

        public SourceText WithChanges(params TextChange[] changes)
        {
            if (changes is null || changes.Length == 0)
                return this;

            return WithChanges((IEnumerable<TextChange>)changes);
        }

        public SourceText WithChanges(IEnumerable<TextChange> changes)
        {
            ArgumentNullException.ThrowIfNull(changes);

            var persistedChanges = changes.OrderByDescending(c => c.Span.Start)
                                          .ToImmutableArray();

            var sb = new StringBuilder(GetText());
            var hasChanges = false;
            var previousStart = int.MaxValue;

            foreach (var textChange in persistedChanges)
            {
                if (textChange.Span.End > previousStart)
                    throw new InvalidOperationException(Resources.SourceTextChangesMustNotOverlap);

                previousStart = textChange.Span.Start;

                if (!HasDifference(sb, textChange))
                    continue;

                hasChanges = true;
                sb.Remove(textChange.Span.Start, textChange.Span.Length);
                sb.Insert(textChange.Span.Start, textChange.NewText);
            }

            if (!hasChanges)
                return this;

            var newText = From(sb.ToString());

            return new ChangedSourceText(this, newText, persistedChanges);
        }

        private static bool HasDifference(StringBuilder sb, TextChange textChange)
        {
            var newText = textChange.NewText ?? string.Empty;
            var newLength = newText.Length;
            var oldLength = textChange.Span.Length;

            if (oldLength != newLength)
                return true;

            var bufferStart = textChange.Span.Start;
            var bufferEnd = textChange.Span.End;

            for (var bufferIndex = bufferStart; bufferIndex < bufferEnd; bufferIndex++)
            {
                var newTextIndex = bufferIndex - bufferStart;
                if (sb[bufferIndex] != newText[newTextIndex])
                    return true;
            }

            return false;
        }

        public IEnumerable<TextChange> GetChanges(SourceText oldText)
        {
            ArgumentNullException.ThrowIfNull(oldText);

            if (oldText == this)
                return Enumerable.Empty<TextChange>();

            var rootFound = false;
            var candidate = this;
            var path = new Stack<ChangedSourceText>();

            while (candidate is not null && !rootFound)
            {
                if (candidate is not ChangedSourceText changed)
                {
                    candidate = null;
                }
                else
                {
                    if (changed.OldText == oldText)
                        rootFound = true;

                    path.Push(changed);
                    candidate = changed.OldText;
                }
            }

            if (!rootFound)
            {
                var oldSpan = new TextSpan(0, oldText.Length);
                var newText = GetText();
                var textChange = new TextChange(oldSpan, newText);
                return ImmutableArray.Create(textChange);
            }

            var changes = new List<TextChange>();
            while (path.Count > 0)
            {
                var c = path.Pop();
                changes.AddRange(c.Changes);
            }

            return changes.ToImmutableArray();
        }

        public SourceText Replace(TextSpan span, string newText)
        {
            ArgumentNullException.ThrowIfNull(newText);

            return WithChanges(new TextChange(span, newText));
        }

        public SourceText Replace(int start, int length, string newText)
        {
            ArgumentNullException.ThrowIfNull(newText);

            var span = new TextSpan(start, length);
            return Replace(span, newText);
        }

        public abstract char this[int index] { get; }

        public abstract int Length { get; }

        public abstract TextLineCollection Lines { get; }
    }
}