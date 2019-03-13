#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NQuery.Text
{
    public sealed class TextChangeSet : IEnumerable<TextChange>
    {
        private readonly List<TextChange> _changes = new List<TextChange>();

        public void ReplaceText(TextSpan span, string newText)
        {
            if (newText == null)
                throw new ArgumentNullException(nameof(newText));

            var change = TextChange.ForReplacement(span, newText);
            RegisterChange(change);
        }

        public void InsertText(int position, string text)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            var change = TextChange.ForInsertion(position, text);
            RegisterChange(change);
        }

        public void DeleteText(TextSpan span)
        {
            var change = TextChange.ForDeletion(span);
            RegisterChange(change);
        }

        private void RegisterChange(TextChange newChange)
        {
            var conflicts = _changes.Any(existingChange => existingChange.Span.IntersectsWith(newChange.Span));
            if (conflicts)
            {
                var message = string.Format(Resources.CannotRegisterOverlappingChange, newChange);
                throw new InvalidOperationException(message);
            }

            _changes.Add(newChange);
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