using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using ActiproSoftware.Text;
using ActiproSoftware.Text.Implementation;
using ActiproSoftware.Text.Parsing;

using NQuery.Language.Services;

namespace NQuery.Language.ActiproWpf
{
    public class NQueryDocument : EditorDocument
    {
        private readonly SynchronizationContext _synchronizationContext = SynchronizationContext.Current;
        private readonly ResultProducer<Tuple<ITextSnapshot, NQueryDocumentType>, NQueryParseData> _parseDataProducer;
        private readonly ResultProducer<Tuple<NQueryParseData, DataContext>, NQuerySemanticData> _semanticDataProducer;

        private NQueryDocumentType _documentType;
        private DataContext _dataContext = DataContext.Default;

        private NQuerySemanticData _semanticData;

        public NQueryDocument()
        {
            _parseDataProducer = new ResultProducer<Tuple<ITextSnapshot, NQueryDocumentType>, NQueryParseData>(ComputeParseData);
            _semanticDataProducer = new ResultProducer<Tuple<NQueryParseData, DataContext>, NQuerySemanticData>(ComputeSemanticModel);
            Language = new NQueryLanguage();
            UpdateParseData();
        }

        public NQueryDocumentType DocumentType
        {
            get { return _documentType; }
            set
            {
                if (_documentType != value)
                {
                    _documentType = value;
                    UpdateParseData();
                }
            }
        }

        public DataContext DataContext
        {
            get { return _dataContext; }
            set
            {
                if (_dataContext != value)
                {
                    _dataContext = value;
                    UpdateSemanticData();
                }
            }
        }

        public new NQueryParseData ParseData
        {
            get { return base.ParseData as NQueryParseData; }
            private set { base.ParseData = value; }
        }

        public NQuerySemanticData SemanticData
        {
            get { return _semanticData; }
            private set
            {
                if (_semanticData != value)
                {
                    _semanticData = value;
                    OnSemanticModelChanged(EventArgs.Empty);
                }
            }
        }

        public Task<NQueryParseData> GetParseDataAsync()
        {
            UpdateParseData();
            return _parseDataProducer.GetResultAsync();
        }

        public async Task<NQuerySemanticData> GetSemanticDataAsync()
        {
            var parseData = await GetParseDataAsync();
            UpdateSemanticData(parseData);
            return await _semanticDataProducer.GetResultAsync();
        }

        protected override void OnTextChanged(TextSnapshotChangedEventArgs e)
        {
            base.OnTextChanged(e);

            UpdateParseData();
        }

        protected override void OnParseDataChanged(ParseDataPropertyChangedEventArgs e)
        {
            base.OnParseDataChanged(e);
            
            if (_synchronizationContext.IsWaitNotificationRequired())
                _synchronizationContext.Post(s => UpdateSemanticData(), null);
            else
                UpdateSemanticData();
        }

        private void UpdateParseData()
        {
            var input = Tuple.Create(CurrentSnapshot, _documentType);
            _parseDataProducer.Update(input);
            _parseDataProducer
                .GetResultAsync()
                .ContinueWith(t => ParseData = t.Result, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void UpdateSemanticData()
        {
            var parseData = ParseData;
            if (parseData == null)
                return;

            UpdateSemanticData(parseData);
        }

        private void UpdateSemanticData(NQueryParseData parseData)
        {
            var input = Tuple.Create(parseData, _dataContext);
            _semanticDataProducer.Update(input);
            _semanticDataProducer
                .GetResultAsync()
                .ContinueWith(t => SemanticData = t.Result, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private static NQueryParseData ComputeParseData(Tuple<ITextSnapshot, NQueryDocumentType> input, CancellationToken cancellationToken)
        {
            Trace.WriteLine("Parsing");
            var snapshot = input.Item1;
            var text = snapshot.Text;
            var documentType = input.Item2;
            var syntaxTree = documentType == NQueryDocumentType.Query
                                 ? SyntaxTree.ParseQuery(text)
                                 : SyntaxTree.ParseExpression(text);
            return new NQueryParseData(snapshot, syntaxTree);
        }

        private static NQuerySemanticData ComputeSemanticModel(Tuple<NQueryParseData, DataContext> input, CancellationToken cancellationToken)
        {
            Trace.WriteLine("Binding");
            var parseData = input.Item1;
            var dataContext = input.Item2;
            var compilation = Compilation.Empty
                .WithSyntaxTree(parseData.SyntaxTree)
                .WithDataContext(dataContext);
            var semanticModel = compilation.GetSemanticModel();
            return new NQuerySemanticData(parseData, semanticModel);
        }

        protected virtual void OnSemanticModelChanged(EventArgs e)
        {
            var handler = SemanticDataChanged;
            if (handler != null)
                handler(this, e);
        }

        public event EventHandler<EventArgs> SemanticDataChanged;
    }
}