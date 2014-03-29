using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Tagging;

using NQuery.Authoring.CodeActions;
using NQuery.Authoring.Composition.CodeActions;
using NQuery.Authoring.VSEditorWpf.Document;

namespace NQuery.Authoring.VSEditorWpf.Squiggles
{
    internal sealed class NQueryCodeIssueTagger : AsyncTagger<IErrorTag, CodeIssue>
    {
        private readonly INQueryDocument _document;
        private readonly ICodeIssueProviderService _codeIssueProviderService;

        public NQueryCodeIssueTagger(INQueryDocument document, ICodeIssueProviderService codeIssueProviderService)
        {
            _document = document;
            _codeIssueProviderService = codeIssueProviderService;
            _document.SemanticModelInvalidated += DocumentOnSemanticModelInvalidated;
            InvalidateTags();
        }

        private void DocumentOnSemanticModelInvalidated(object sender, EventArgs eventArgs)
        {
            InvalidateTags();
        }

        protected override async Task<Tuple<ITextSnapshot, IEnumerable<CodeIssue>>> GetRawTagsAsync()
        {
            var semanticModel = await _document.GetSemanticModelAsync();
            var snapshot = _document.GetTextSnapshot(semanticModel.Compilation.SyntaxTree);
            var providers = _codeIssueProviderService.Providers;
            var issues = await Task.Run(() => semanticModel.GetIssues(providers).Where(IsWarningOrError).ToImmutableArray());
            return Tuple.Create(snapshot, issues.AsEnumerable());
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