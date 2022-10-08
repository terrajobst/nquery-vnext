using System.Collections.Immutable;

using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Tagging;

using NQuery.Authoring.CodeActions;
using NQuery.Authoring.Composition.CodeActions;

namespace NQuery.Authoring.VSEditorWpf.Squiggles
{
    internal sealed class NQueryCodeIssueTagger : AsyncTagger<IErrorTag, CodeIssue>
    {
        private readonly Workspace _workspace;
        private readonly ICodeIssueProviderService _codeIssueProviderService;

        public NQueryCodeIssueTagger(Workspace workspace, ICodeIssueProviderService codeIssueProviderService)
        {
            _workspace = workspace;
            _workspace.CurrentDocumentChanged += WorkspaceOnCurrentDocumentChanged;
            _codeIssueProviderService = codeIssueProviderService;
            InvalidateTagsAsync();
        }

        private void WorkspaceOnCurrentDocumentChanged(object sender, EventArgs e)
        {
            InvalidateTagsAsync();
        }

        protected override async Task<(ITextSnapshot Snapshot, IEnumerable<CodeIssue> RawTags)> GetRawTagsAsync()
        {
            var document = _workspace.CurrentDocument;
            var semanticModel = await document.GetSemanticModelAsync();
            var snapshot = document.GetTextSnapshot();
            var providers = _codeIssueProviderService.Providers;
            var issues = await Task.Run(() => semanticModel.GetIssues(providers).Where(IsWarningOrError).ToImmutableArray());
            return (snapshot, issues.AsEnumerable());
        }

        private static bool IsWarningOrError(CodeIssue issue)
        {
            return issue.Kind == CodeIssueKind.Warning ||
                   issue.Kind == CodeIssueKind.Error;
        }

        protected override ITagSpan<IErrorTag> CreateTagSpan(ITextSnapshot snapshot, CodeIssue rawTag)
        {
            var name = rawTag.Kind == CodeIssueKind.Warning
                ? PredefinedErrorTypeNames.Warning
                : PredefinedErrorTypeNames.CompilerError;

            var span = new SnapshotSpan(snapshot, rawTag.Span.Start, rawTag.Span.Length);
            var tag = new ErrorTag(name, rawTag.Description);
            return new TagSpan<IErrorTag>(span, tag);
        }
    }
}