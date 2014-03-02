using System;
using System.Collections.Generic;

namespace NQuery.Authoring.CodeActions
{
    public interface ICodeIssueProvider
    {
        IEnumerable<CodeIssue> GetIssues(SemanticModel semanticModel);
    }
}