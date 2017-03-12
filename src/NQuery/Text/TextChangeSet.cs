using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NQuery.Text
{
    public sealed class TextChangeSet : IEnumerable<TextChange>
    {
        private readonly List<TextChange> _changes = new List<TextChange>();

        public static TextChangeSet Create(IEnumerable<TextChange> changes)
        {
            var result = new TextChangeSet();
            foreach (var change in changes)
                result.Add(change);

            return result;
        }

        public void ReplaceText(TextSpan span, string newText)
        {
            if (newText == null)
                throw new ArgumentNullException(nameof(newText));

            var change = TextChange.ForReplacement(span, newText);
            Add(change);
        }

        public void InsertText(int position, string text)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            var change = TextChange.ForInsertion(position, text);
            Add(change);
        }

        public void DeleteText(TextSpan span)
        {
            var change = TextChange.ForDeletion(span);
            Add(change);
        }

        public void Add(TextChange newChange)
        {
            var conflicts = _changes.Any(existingChange => existingChange.Span.IntersectsWith(newChange.Span));
            if (conflicts)
            {
                var message = string.Format(Resources.CannotRegisterOverlappingChange, newChange);
                throw new InvalidOperationException(message);
            }

            _changes.Add(newChange);
        }

        public int TranslatePosition(int oldLocation)
        {
            var accumulativeDelta = 0;

            foreach (var change in _changes)
            {
                var oldSpan = change.Span;
                var oldLength = oldSpan.Length;

                var newLength = change.NewText.Length;
                var newSpan = new TextSpan(oldSpan.Start, newLength);

                if (oldLocation < oldSpan.Start)
                {
                    // The edit is after the position, thus
                    // nothing to do.
                }
                else if (oldSpan.End <= oldLocation)
                {
                    // The edit is fully before the position. Thus, we simply
                    // have to record the character delta.

                    var delta = newLength - oldLength;
                    accumulativeDelta += delta;
                }
                else
                {
                    // The position is within the edit itself.

                    if (oldLocation >= newSpan.End)
                    {
                        var adjustedLocation = newSpan.End - 1;
                        var delta = adjustedLocation - oldLocation;
                        accumulativeDelta += delta;
                    }
                }
            }

            return oldLocation + accumulativeDelta;
        }

        public IEnumerator<TextChange> GetEnumerator()
        {
            return _changes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}