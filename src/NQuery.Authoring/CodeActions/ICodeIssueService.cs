using System;
using System.Collections.Generic;

namespace NQuery.Authoring.CodeActions
{
    public interface ICodeIssueService
    {
        IEnumerable<CodeIssue> GetIssues(SemanticModel semanticModel);
    }
}