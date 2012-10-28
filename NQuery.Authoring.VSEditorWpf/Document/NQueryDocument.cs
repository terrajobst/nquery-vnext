using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.Text;

using NQuery.Language.Services;

namespace NQuery.Language.VSEditor.Document
{
    internal sealed class NQueryDocument : INQueryDocument
    {
        private static readonly ConditionalWeakTable<SyntaxTree, ITextSnapshot> _syntaxTreeSnapshotMap = new ConditionalWeakTable<SyntaxTree, ITextSnapshot>();

        private readonly ITextBuffer _textBuffer;
        private readonly ResultProducer<ITextSnapshot, SyntaxTree> _syntaxTreeProducer;
        private readonly ResultProducer<Compilation, SemanticModel> _semanticModelProducer;

        private NQueryDocumentType _documentType;
        private Compilation _compilation;

        public NQueryDocument(ITextBuffer textBuffer)
        {
            _textBuffer = textBuffer;
            _textBuffer.PostChanged += TextBufferOnPostChanged;
            _syntaxTreeProducer = new ResultProducer<ITextSnapshot, SyntaxTree>(ParseSyntaxTree);
            _semanticModelProducer = new ResultProducer<Compilation, SemanticModel>(GetSemanticModel);
            _compilation = Compilation.Empty.WithDataContext(DataContext.Default);
            UpdateSyntaxTree();
        }

        private void TextBufferOnPostChanged(object sender, EventArgs e)
        {
            UpdateSyntaxTree();
        }

        private void UpdateSyntaxTree()
        {
            if (!_syntaxTreeProducer.Update(_textBuffer.CurrentSnapshot))
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

        private static SyntaxTree ParseSyntaxTree(ITextSnapshot snapshot, CancellationToken cancellationToken)
        {
            Trace.WriteLine("ParseSyntaxTree for " + snapshot.Version);
            
            var syntaxTree = SyntaxTree.ParseQuery(snapshot.GetText());
            lock (_syntaxTreeSnapshotMap)
                _syntaxTreeSnapshotMap.Add(syntaxTree, snapshot);

            return syntaxTree;
        }

        private static SemanticModel GetSemanticModel(Compilation compilation, CancellationToken cancellationToken)
        {
            Trace.WriteLine("GetSemanticModel");
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

        public ITextSnapshot GetTextSnapshot(SyntaxTree syntaxTree)
        {
            ITextSnapshot result;
            _syntaxTreeSnapshotMap.TryGetValue(syntaxTree, out result);
            return result;
        }

        public Task<SyntaxTree> GetSyntaxTreeAsync()
        {
            return _syntaxTreeProducer.GetResultAsync();
        }

        public async Task<SemanticModel> GetSemanticModelAsync()
        {
            var syntaxTree = await GetSyntaxTreeAsync();
            UpdateSemanticModel(syntaxTree);
            return await _semanticModelProducer.GetResultAsync();
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