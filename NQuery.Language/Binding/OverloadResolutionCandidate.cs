using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace NQuery.Language.Binding
{
    internal sealed class OverloadResolutionCandidate<T>
        where T: Signature
    {
        private readonly T _signature;
        private readonly ReadOnlyCollection<Conversion> _argumentConversions;
        private readonly bool _isApplicable;
        private readonly bool _hasBetterAlternative;

        internal OverloadResolutionCandidate(T signature, IList<Conversion> argumentConversions)
            : this(signature, argumentConversions, false, false)
        {
        }

        private OverloadResolutionCandidate(T signature, IList<Conversion> argumentConversions, bool isApplicable, bool hasBetterAlternative)
        {
            _signature = signature;
            _argumentConversions = new ReadOnlyCollection<Conversion>(argumentConversions);
            _isApplicable = isApplicable;
            _hasBetterAlternative = hasBetterAlternative;
        }

        public bool IsSuitable
        {
            get { return _isApplicable && !_hasBetterAlternative; }
        }

        public bool IsApplicable
        {
            get { return _isApplicable; }
        }

        public bool HasBetterAlternative
        {
            get { return _hasBetterAlternative; }
        }

        public T Signature
        {
            get { return _signature; }
        }

        public ReadOnlyCollection<Conversion> ArgumentConversions
        {
            get { return _argumentConversions; }
        }

        internal OverloadResolutionCandidate<T> MarkApplicable()
        {
            return new OverloadResolutionCandidate<T>(_signature, _argumentConversions, true, false);
        }

        internal OverloadResolutionCandidate<T> MarkNotApplicable()
        {
            return new OverloadResolutionCandidate<T>(_signature, _argumentConversions, false, false);
        }

        internal OverloadResolutionCandidate<T> MarkHasBetterAlternative()
        {
            return new OverloadResolutionCandidate<T>(_signature, _argumentConversions, true, true);
        }

        public override string ToString()
        {
            var type = !IsApplicable
                           ? "Not Applicable"
                           : HasBetterAlternative
                                 ? "Has Better Alternative"
                                 : "Suitable";

            return string.Format("{0} [{1}]", _signature, type);
        }
    }
}