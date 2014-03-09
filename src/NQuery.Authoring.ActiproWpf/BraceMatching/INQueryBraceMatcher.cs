using System;
using System.Collections.ObjectModel;

using ActiproSoftware.Text.Analysis;

using NQuery.Authoring.BraceMatching;

namespace NQuery.Authoring.ActiproWpf.BraceMatching
{
    public interface INQueryBraceMatcher : IStructureMatcher
    {
        Collection<IBraceMatcher> Matchers { get; }
    }
}