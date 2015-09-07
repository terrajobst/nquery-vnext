using System;
using System.Collections.Generic;

namespace NQuery.Text
{
    public abstract class SourceTextContainer
    {
        public abstract SourceText Current { get; }

        public virtual IEnumerable<TextChange> GetChanges(SourceText newText, SourceText oldText)
        {
            return newText.GetChanges(oldText);
        }

        public abstract event EventHandler<EventArgs> CurrentChanged;
    }
}