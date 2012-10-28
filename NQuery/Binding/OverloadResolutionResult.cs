using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace NQuery.Binding
{
    internal sealed class OverloadResolutionResult<T>
        where T: Signature
    {
        private readonly OverloadResolutionCandidate<T> _best;
        private readonly ReadOnlyCollection<OverloadResolutionCandidate<T>> _candidates;
        private readonly OverloadResolutionCandidate<T> _selected;

        public OverloadResolutionResult(OverloadResolutionCandidate<T> best, OverloadResolutionCandidate<T> selected, IList<OverloadResolutionCandidate<T>> candidates)
        {
            _best = best;
            _selected = selected;
            _candidates = new ReadOnlyCollection<OverloadResolutionCandidate<T>>(candidates);
        }

        public static readonly OverloadResolutionResult<T> None = new OverloadResolutionResult<T>(null, null, new OverloadResolutionCandidate<T>[0]);

        public OverloadResolutionCandidate<T> Best
        {
            get { return _best; }
        }

        public OverloadResolutionCandidate<T> Selected
        {
            get { return _selected; }
        }

        public ReadOnlyCollection<OverloadResolutionCandidate<T>> Candidates
        {
            get { return _candidates; }
        }
    }
}