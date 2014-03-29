using System;
using System.Collections.Immutable;

using NQuery.Authoring.CodeActions;

namespace NQuery.Authoring.Composition.CodeActions
{
    public interface ICodeIssueProviderService
    {
        ImmutableArray<ICodeIssueProvider> Providers { get; }
    }
}