using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

using NQuery.Authoring.BraceMatching;

namespace NQuery.Authoring.Composition.BraceMatching
{
    [Export(typeof(IBraceMatcherService))]
    internal sealed class BraceMatcherService : IBraceMatcherService
    {
        private readonly IReadOnlyCollection<IBraceMatcher> _matchers;

        [ImportingConstructor]
        public BraceMatcherService([ImportMany] IEnumerable<IBraceMatcher> matchers)
        {
            _matchers = matchers.Concat(BraceMatchingExtensions.GetStandardBraceMatchers()).ToArray();
        }

        public IReadOnlyCollection<IBraceMatcher> Matchers
        {
            get { return _matchers; }
        }
    }
}