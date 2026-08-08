namespace NQuery.Authoring.LanguageServer.Protocol;

public enum MessageType
{
    Error = 1,
    Warning = 2,
    Info = 3,
    Log = 4
}

public sealed record ShowMessageParams
{
    public required MessageType Type { get; init; }
    public required string Message { get; init; }
}

public sealed record ShowMessageRequestParams
{
    public required MessageType Type { get; init; }
    public required string Message { get; init; }
    public IReadOnlyList<MessageActionItem>? Actions { get; init; }
}

public sealed record MessageActionItem
{
    public required string Title { get; init; }
}

public sealed record LogMessageParams
{
    public required MessageType Type { get; init; }
    public required string Message { get; init; }
}
