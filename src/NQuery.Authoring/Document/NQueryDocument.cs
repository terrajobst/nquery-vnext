using System;
using System.Threading;
using System.Threading.Tasks;

using NQuery.Text;

namespace NQuery.Authoring.Document
{
    public sealed class NQueryDocument
    {
        private readonly TextBufferProvider _textBufferProvider;
        private readonly ResultProducer<TextBuffer, SyntaxTree> _syntaxTreeProducer;
        private readonly ResultProducer<Compilation, SemanticModel> _semanticModelProducer;

        private NQueryDocumentType _documentType;
        private Compilation _compilation;

        public NQueryDocument(TextBufferProvider textBufferProvider)
        {
            _textBufferProvider = textBufferProvider;
            _textBufferProvider.CurrentChanged += TextBufferProviderOnCurrentChanged;
            _syntaxTreeProducer = new ResultProducer<TextBuffer, SyntaxTree>(ParseSyntaxTree);
            _semanticModelProducer = new ResultProducer<Compilation, SemanticModel>(GetSemanticModel);
            _compilation = Compilation.Empty.WithDataContext(DataContext.Default);
            UpdateSyntaxTree();
        }

        private void TextBufferProviderOnCurrentChanged(object sender, EventArgs e)
        {
            UpdateSyntaxTree();
        }

        private void UpdateSyntaxTree()
        {
            if (!_syntaxTreeProducer.Update(_textBufferProvider.Current))
                return;

            UpdateSemanticModel();
            OnSyntaxTreeInvalidated(EventArgs.Empty);
        }

        private void UpdateSemanticModel()
        {
            _syntaxTreeProducer
                .GetResultAsync()
                .ContinueWith(t => UpdateSemanticModel(t.Result), TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void UpdateSemanticModel(SyntaxTree syntaxTree)
        {
            _compilation = _compilation.WithSyntaxTree(syntaxTree);
            if (_semanticModelProducer.Update(_compilation))
                OnSemanticModelInvalidated(EventArgs.Empty);
        }

        private static SyntaxTree ParseSyntaxTree(TextBuffer textBuffer, CancellationToken cancellationToken)
        {
            return SyntaxTree.ParseQuery(textBuffer);
        }

        private static SemanticModel GetSemanticModel(Compilation compilation, CancellationToken cancellationToken)
        {
            return compilation.GetSemanticModel();
        }

        public NQueryDocumentType DocumentType
        {
            get { return _documentType; }
            set
            {
                if (_documentType != value)
                {
                    _documentType = value;
                    UpdateSyntaxTree();
                }
            }
        }

        public DataContext DataContext
        {
            get { return _compilation.DataContext; }
            set
            {
                if (_compilation.DataContext != value)
                {
                    _compilation = _compilation.WithDataContext(value);
                    UpdateSemanticModel();
                }
            }
        }

        public Task<SyntaxTree> GetSyntaxTreeAsync()
        {
            UpdateSyntaxTree();
            return _syntaxTreeProducer.GetResultAsync();
        }

        public bool TryGetSyntaxTree(out SyntaxTree syntaxTree)
        {
            return _syntaxTreeProducer.TryGetResult(out syntaxTree);
        }

        public async Task<SemanticModel> GetSemanticModelAsync()
        {
            var syntaxTree = await GetSyntaxTreeAsync();
            UpdateSemanticModel(syntaxTree);
            return await _semanticModelProducer.GetResultAsync();
        }

        public bool TryGetSemanticModel(out SemanticModel semanticModel)
        {
            return _semanticModelProducer.TryGetResult(out semanticModel);
        }

        private void OnSyntaxTreeInvalidated(EventArgs e)
        {
            var handler = SyntaxTreeInvalidated;
            if (handler != null)
                handler(this, e);
        }

        private void OnSemanticModelInvalidated(EventArgs e)
        {
            var handler = SemanticModelInvalidated;
            if (handler != null)
                handler(this, e);
        }

        public event EventHandler<EventArgs> SyntaxTreeInvalidated;

        public event EventHandler<EventArgs> SemanticModelInvalidated;
    }
}