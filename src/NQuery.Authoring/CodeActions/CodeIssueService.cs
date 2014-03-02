using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

namespace NQuery.Authoring.CodeActions
{
    [Export(typeof(ICodeIssueService))]
    internal sealed class CodeIssueService : ICodeIssueService
    {
        [ImportMany]
        public IEnumerable<ICodeIssueProvider> IssueProviders { get; set; }

        public IEnumerable<CodeIssue> GetIssues(SemanticModel semanticModel)
        {
            return IssueProviders.SelectMany(p => p.GetIssues(semanticModel));
        }
    }
}