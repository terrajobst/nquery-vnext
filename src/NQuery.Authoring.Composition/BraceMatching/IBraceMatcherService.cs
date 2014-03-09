using System;
using System.Collections.Generic;

using NQuery.Authoring.BraceMatching;

namespace NQuery.Authoring.Composition.BraceMatching
{
    public interface IBraceMatcherService
    {
        IReadOnlyCollection<IBraceMatcher> Matchers { get; }
    }
}