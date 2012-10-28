using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NQuery.Language.Services
{
    public sealed class ResultProducer<TInput, TResult>
        where TInput : class
        where TResult : class
    {
        private readonly Func<TInput, CancellationToken, TResult> _selector;
        private readonly List<TaskCompletionSource<TResult>> _sources = new List<TaskCompletionSource<TResult>>();

        private TInput _lastInput;
        private TResult _lastResult;

        private CancellationTokenSource _cancellationTokenSource;
        private Task<TResult> _task;

        public ResultProducer(Func<TInput, CancellationToken, TResult> selector)
        {
            _selector = selector;
        }

        public bool Update(TInput input)
        {
            if (input == null)
                throw new ArgumentNullException("input");

            if (_lastInput != null && _lastInput.Equals(input))
                return false;

            _lastInput = input;
            _lastResult = null;

            if (_cancellationTokenSource != null)
                _cancellationTokenSource.Cancel();

            _cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = _cancellationTokenSource.Token;

            _task = Task.Factory.StartNew(() => _selector(input, cancellationToken), cancellationToken);
            _task.ContinueWith(UpdateResult,
                               cancellationToken,
                               TaskContinuationOptions.None,
                               TaskScheduler.FromCurrentSynchronizationContext());
            return true;
        }

        private void UpdateResult(Task<TResult> task)
        {
            if (task.IsCanceled)
            {
                foreach (var source in _sources)
                    source.SetCanceled();
            }
            else if (task.IsFaulted)
            {
                if (task.Exception != null)
                {
                    foreach (var source in _sources)
                        source.SetException(task.Exception);
                }
            }
            else
            {
                _lastResult = task.Result;
                foreach (var source in _sources)
                    source.SetResult(task.Result);
            }

            _sources.Clear();
        }

        public Task<TResult> GetResultAsync()
        {
            if (_lastResult != null)
                return Task.FromResult(_lastResult);

            var source = new TaskCompletionSource<TResult>();
            _sources.Add(source);
            return source.Task;
        }
    }
}