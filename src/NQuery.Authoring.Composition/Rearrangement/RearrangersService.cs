using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.Composition;
using System.Linq;

using NQuery.Authoring.Rearrangement;

namespace NQuery.Authoring.Composition.Rearrangement
{
    [Export(typeof(IRearrangersService))]
    internal sealed class RearrangersService : IRearrangersService
    {
        private readonly ImmutableArray<IRearranger> _rearrangers;

        [ImportingConstructor]
        public RearrangersService([ImportMany] IEnumerable<IRearranger> rearrangers)
        {
            _rearrangers = rearrangers.Concat(RearrangementExtensions.GetStandardRearrangers()).ToImmutableArray();
        }

        public ImmutableArray<IRearranger> Rearrangers
        {
            get { return _rearrangers; }
        }
    }
}