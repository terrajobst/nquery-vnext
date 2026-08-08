using System.Diagnostics;

using NQuery.Authoring.LanguageServer.Mapping;
using NQuery.Authoring.LanguageServer.Protocol;
using NQuery.CodeAnalysis;

using StreamJsonRpc;

namespace NQuery.Authoring.LanguageServer.Server;

internal sealed partial class LanguageServerTarget
{
    [JsonRpcMethod(Methods.NQueryExecute, UseSingleObjectParameterDeserialization = true)]
    public async Task<ExecuteResult> ExecuteAsync(ExecuteParams parameters, CancellationToken cancellationToken)
    {
        ThrowIfNull(parameters);

        if (!_options.AllowExecution)
            return ExecuteError(@"This server does not allow running queries.");

        var snapshot = await TryGetSnapshotAsync(parameters.TextDocument.Uri, cancellationToken);
        if (snapshot is null)
            return ExecuteError(@"The document is not open.");

        // Reported before compiling: without a catalog the compiler's complaint would be that
        // every table is undeclared, which hides the actual cause.
        if (CatalogError is { } catalogError)
            return ExecuteError($"The catalog is unavailable, so the query cannot run: {catalogError}");

        var compilation = await snapshot.Value.Document.GetCompilationAsync(cancellationToken);

        // A client may ask for fewer rows than the server allows, never more.
        var maxRows = Math.Min(_options.MaxRows, parameters.MaxRows ?? int.MaxValue);

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(_options.ExecutionTimeout);

        try
        {
            // Iterators are synchronous and blocking, so execution goes to the thread pool and
            // the rest of the server keeps answering requests while it runs.
            return await Task.Run(() => Execute(compilation, maxRows, timeout.Token), timeout.Token);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return ExecuteError($"The query did not finish within {_options.ExecutionTimeout.TotalSeconds:N0} seconds.");
        }
    }

    private static ExecuteResult Execute(Compilation compilation, int maxRows, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        CompiledQuery compiled;

        try
        {
            compiled = compilation.Compile();
        }
        catch (CompilationException ex)
        {
            // The document's diagnostics already say this in the editor; the message keeps the
            // results panel self-explanatory.
            return ExecuteError(ex.Message);
        }

        using var reader = compiled.CreateReader();

        var columns = new List<ResultColumn>(reader.ColumnCount);

        for (var i = 0; i < reader.ColumnCount; i++)
        {
            columns.Add(new ResultColumn
            {
                Name = reader.GetColumnName(i),
                Type = ValueFormatter.FormatTypeName(reader.GetColumnType(i))
            });
        }

        var rows = new List<IReadOnlyList<string?>>();
        var truncated = false;

        while (reader.Read())
        {
            // Checked per row rather than per cell: a single row that blocks forever cannot be
            // interrupted, but no realistic iterator does that, and per-cell checks would cost
            // more than they buy.
            cancellationToken.ThrowIfCancellationRequested();

            if (rows.Count >= maxRows)
            {
                truncated = true;
                break;
            }

            var row = new string?[reader.ColumnCount];

            for (var i = 0; i < reader.ColumnCount; i++)
                row[i] = ValueFormatter.Format(reader[i]);

            rows.Add(row);
        }

        stopwatch.Stop();

        return new ExecuteResult
        {
            Columns = columns,
            Rows = rows,
            Truncated = truncated,
            ElapsedMilliseconds = stopwatch.ElapsedMilliseconds
        };
    }

    [JsonRpcMethod(Methods.NQueryShowPlan, UseSingleObjectParameterDeserialization = true)]
    public async Task<ShowPlanResult> ShowPlanAsync(ShowPlanParams parameters, CancellationToken cancellationToken)
    {
        ThrowIfNull(parameters);

        var snapshot = await TryGetSnapshotAsync(parameters.TextDocument.Uri, cancellationToken);
        if (snapshot is null)
            return new ShowPlanResult { Steps = [], ErrorMessage = @"The document is not open." };

        if (CatalogError is { } catalogError)
            return new ShowPlanResult { Steps = [], ErrorMessage = $"The catalog is unavailable, so no plan can be built: {catalogError}" };

        var compilation = await snapshot.Value.Document.GetCompilationAsync(cancellationToken);

        // Optimization runs the whole pipeline, so it goes to the thread pool like execution.
        return await Task.Run(() => BuildShowPlan(compilation), cancellationToken);
    }

    private static ShowPlanResult BuildShowPlan(Compilation compilation)
    {
        // GetShowPlanSteps yields nothing when the query has diagnostics rather than throwing.
        var steps = compilation.GetShowPlanSteps()
                               .Select(s => new ShowPlanStep { Name = s.Name, Root = Convert(s.Root) })
                               .ToArray();

        return steps.Length == 0
            ? new ShowPlanResult { Steps = [], ErrorMessage = @"The query has errors, so no plan is available." }
            : new ShowPlanResult { Steps = steps };
    }

    private static ShowPlanNodeInfo Convert(ShowPlanNode node)
    {
        return new ShowPlanNodeInfo
        {
            OperatorName = node.OperatorName,
            IsScalar = node.IsScalar,
            Properties = [.. node.Properties.Select(p => new ShowPlanProperty { Name = p.Key, Value = p.Value })],
            Children = [.. node.Children.Select(Convert)]
        };
    }

    private static ExecuteResult ExecuteError(string message)
    {
        return new ExecuteResult
        {
            Columns = [],
            Rows = [],
            Truncated = false,
            ElapsedMilliseconds = 0,
            ErrorMessage = message
        };
    }
}
