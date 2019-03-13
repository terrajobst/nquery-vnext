#nullable enable

using System;

namespace NQuery.Text
{
    internal class StaticSourceTextContainer : SourceTextContainer
    {
        public StaticSourceTextContainer(SourceText current)
        {
            if (current == null)
                throw new ArgumentNullException(nameof(current));

            Current = current;
        }

        public override SourceText Current { get; }

        public override event EventHandler<EventArgs> CurrentChanged
        {
            add {}
            remove {}
        }
    }
}