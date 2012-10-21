using System;
using System.Threading;
using System.Threading.Tasks;
using ActiproSoftware.Text.Implementation;
using ActiproSoftware.Text.Parsing;
using NQuery.Language;
using NQuery.Language.VSEditor.Document;

namespace NQueryViewerActiproWpf
{
    public class NQueryDocument : EditorDocument
    {
        private readonly SynchronizationContext _synchronizationContext = SynchronizationContext.Current;
        private readonly ResultProducer<Compilation, SemanticModel> _semanticModelProducer;

        private NQueryDocumentType _documentType;
        private Compilation _compilation = Compilation.Empty;
        private SemanticModel _semanticModel;

        public NQueryDocument()
        {
            _semanticModelProducer = new ResultProducer<Compilation, SemanticModel>(GetSemanticModel);
            Language = new NQueryLanguage();
        }

        public NQueryDocumentType DocumentType
        {
            get { return _documentType; }
            set
            {
                _documentType = value;
                QueueParseRequest();
            }
        }

        public DataContext DataContext
        {
            get { return _compilation.DataContext; }
            set
            {
                _compilation = _compilation.WithDataContext(value);
                UpdateSemanticModel();
            }
        }

        public SemanticModel SemanticModel
        {
            get { return _semanticModel; }
            private set
            {
                if (_semanticModel != value)
                {
                    _semanticModel = value;
                    OnSemanticModelChanged(EventArgs.Empty);
                }
            }
        }

        public Task<SemanticModel> GetSemanticModelAsync()
        {
            return _semanticModelProducer.GetResultAsync();
        }

        protected override void OnParseDataChanged(ParseDataPropertyChangedEventArgs e)
        {
            base.OnParseDataChanged(e);
            _synchronizationContext.Post(s => UpdateSemanticModel(), null);
        }

        protected virtual void OnSemanticModelChanged(EventArgs e)
        {
            var handler = SemanticModelChanged;
            if (handler != null)
                handler(this, e);
        }

        private void UpdateSemanticModel()
        {
            var parseData = ParseData as NQueryParseData;
            if (parseData == null)
                return;

            _compilation = _compilation.WithSyntaxTree(parseData.SyntaxTree);
            _semanticModelProducer.Update(_compilation);
            _semanticModelProducer
                .GetResultAsync()
                .ContinueWith(t => SemanticModel = t.Result, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private static SemanticModel GetSemanticModel(Compilation compilation, CancellationToken cancellationToken)
        {
            return compilation.GetSemanticModel();
        }

        public event EventHandler<EventArgs> SemanticModelChanged;
    }
}