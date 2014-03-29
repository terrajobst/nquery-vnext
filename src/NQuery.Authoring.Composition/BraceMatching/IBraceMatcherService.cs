using System;
using System.Collections.Immutable;

using NQuery.Authoring.BraceMatching;

namespace NQuery.Authoring.Composition.BraceMatching
{
    public interface IBraceMatcherService
    {
        ImmutableArray<IBraceMatcher> Matchers { get; }
    }
}