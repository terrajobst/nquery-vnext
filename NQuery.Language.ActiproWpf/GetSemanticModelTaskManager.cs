using System.Threading.Tasks;
using NQuery.Language;

namespace NQueryViewerActiproWpf
{
    internal sealed class GetSemanticModelTaskManager
    {
        private readonly NQueryDocument _document;
        private readonly TaskCompletionSource<SemanticModel> _taskCompletionSource = new TaskCompletionSource<SemanticModel>(); 

        public GetSemanticModelTaskManager(NQueryDocument document)
        {
            _document = document;
            QueueRequest();
        }

        private void QueueRequest()
        {
            _document
                .GetSemanticModelAsync()
                .ContinueWith(t => UpdateSemanticModel(t.Result));
        }

        private void UpdateSemanticModel(SemanticModel result)
        {
            if (result != null)
            {
                var parseData = _document.GetParseData();
                if (parseData != null && parseData.SyntaxTree == result.Compilation.SyntaxTree) 
                    _taskCompletionSource.SetResult(result);
            }

            QueueRequest();
        }

        public Task<SemanticModel> Task
        {
            get { return _taskCompletionSource.Task; }
        }
    }
}