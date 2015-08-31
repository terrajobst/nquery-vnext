using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;

using NQuery.Authoring.Classifications;

namespace NQuery.Authoring.VSEditorWpf.Classification
{
    internal sealed class NQuerySyntaxClassifier : AsyncTagger<IClassificationTag, SyntaxClassificationSpan>
    {
        private readonly INQueryClassificationService _classificationService;
        private readonly Workspace _workspace;

        private ClassificationProducer _producer;

        public NQuerySyntaxClassifier(INQueryClassificationService classificationService, Workspace workspace)
        {
            _classificationService = classificationService;
            _workspace = workspace;
            _workspace.CurrentDocumentChanged += WorkspaceOnCurrentDocumentChanged;
            UpdateClassifications();
        }

        private async void UpdateClassifications()
        {
            var producer = new ClassificationProducer(_workspace.CurrentDocument);
            _producer = producer;

            await producer.ClassifyFastAsync();
            if (_producer != producer)
                return;

            InvalidateTags();

            await producer.ClassifyFullAsync();
            if (_producer != producer)
                return;

            InvalidateTags();
        }

        private void WorkspaceOnCurrentDocumentChanged(object sender, EventArgs e)
        {
            UpdateClassifications();
        }

        protected override async Task<Tuple<ITextSnapshot, IEnumerable<SyntaxClassificationSpan>>> GetRawTagsAsync()
        {
            var document = _workspace.CurrentDocument;
            var snapshot = document.GetTextSnapshot();

            var classificationSpans = await _producer.ClassifyFastAsync();
            return Tuple.Create(snapshot, classificationSpans.AsEnumerable());
        }

        protected override ITagSpan<IClassificationTag> CreateTagSpan(ITextSnapshot snapshot, SyntaxClassificationSpan rawTag)
        {
            var span = rawTag.Span;
            var snapshotSpan = new SnapshotSpan(snapshot, span.Start, span.Length);
            var type = GetClassificationType(rawTag.Classification);
            var tag = new ClassificationTag(type);
            return new TagSpan<IClassificationTag>(snapshotSpan, tag);
        }

        private IClassificationType GetClassificationType(SyntaxClassification classification)
        {
            switch (classification)
            {
                case SyntaxClassification.Whitespace:
                    return _classificationService.WhiteSpace;
                case SyntaxClassification.Comment:
                    return _classificationService.Comment;
                case SyntaxClassification.Keyword:
                    return _classificationService.Keyword;
                 case SyntaxClassification.Punctuation:
                    return _classificationService.Punctuation;
                case SyntaxClassification.Identifier:
                    return _classificationService.Identifier;
                case SyntaxClassification.StringLiteral:
                    return _classificationService.StringLiteral;
                case SyntaxClassification.NumberLiteral:
                    return _classificationService.NumberLiteral;
                default:
                    throw ExceptionBuilder.UnexpectedValue(classification);
            }
        }

        private sealed class ClassificationProducer
        {
            private readonly Task<IReadOnlyList<SyntaxClassificationSpan>> _tokenClassificationTask;
            private readonly Task<IReadOnlyList<SyntaxClassificationSpan>> _syntaxClassificationTask;

            public ClassificationProducer(Document document)
            {
                _tokenClassificationTask = ClassifyTokens(document);
                _syntaxClassificationTask = ClassifySyntax(document);
            }

            public async Task<IReadOnlyList<SyntaxClassificationSpan>> ClassifyFastAsync()
            {
                await Task.WhenAny(_tokenClassificationTask, _syntaxClassificationTask);
                return _syntaxClassificationTask.IsCompleted
                        ? _syntaxClassificationTask.Result
                        : _tokenClassificationTask.Result;
            }

            public Task<IReadOnlyList<SyntaxClassificationSpan>> ClassifyFullAsync()
            {
                return _syntaxClassificationTask;
            }

            private static Task<IReadOnlyList<SyntaxClassificationSpan>> ClassifyTokens(Document document)
            {
                return Task.Run(() => document.Text.ClassifyTokens());
            }

            private static async Task<IReadOnlyList<SyntaxClassificationSpan>> ClassifySyntax(Document document)
            {
                var syntaxTree = await document.GetSyntaxTreeAsync();
                return await Task.Run(() => syntaxTree.Root.ClassifySyntax());
            }
        }
    }
}