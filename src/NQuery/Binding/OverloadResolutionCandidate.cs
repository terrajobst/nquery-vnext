using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace NQuery.Binding
{
    internal sealed class OverloadResolutionCandidate<T>
        where T: Signature
    {
        internal OverloadResolutionCandidate(T signature, IEnumerable<Conversion> argumentConversions)
            : this(signature, argumentConversions, false, false)
        {
        }

        private OverloadResolutionCandidate(T signature, IEnumerable<Conversion> argumentConversions, bool isApplicable, bool hasBetterAlternative)
        {
            Signature = signature;
            ArgumentConversions = argumentConversions.ToImmutableArray();
            IsApplicable = isApplicable;
            HasBetterAlternative = hasBetterAlternative;
        }

        public bool IsSuitable
        {
            get { return IsApplicable && !HasBetterAlternative; }
        }

        public bool IsApplicable { get; }

        public bool HasBetterAlternative { get; }

        public T Signature { get; }

        public ImmutableArray<Conversion> ArgumentConversions { get; }

        internal OverloadResolutionCandidate<T> MarkApplicable()
        {
            return new OverloadResolutionCandidate<T>(Signature, ArgumentConversions, true, false);
        }

        internal OverloadResolutionCandidate<T> MarkNotApplicable()
        {
            return new OverloadResolutionCandidate<T>(Signature, ArgumentConversions, false, false);
        }

        internal OverloadResolutionCandidate<T> MarkHasBetterAlternative()
        {
            return new OverloadResolutionCandidate<T>(Signature, ArgumentConversions, true, true);
        }

        public override string ToString()
        {
            var type = !IsApplicable
                           ? @"Not Applicable"
                           : HasBetterAlternative
                                 ? @"Has Better Alternative"
                                 : @"Suitable";

            return $"{Signature} [{type}]";
        }
    }
}