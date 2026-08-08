using NQuery.Authoring.LanguageServer.Mapping;
using NQuery.Authoring.LanguageServer.Protocol;
using NQuery.CodeAnalysis.Text;

namespace NQuery.Authoring.LanguageServer.Text;

// Unlike the Actipro and VS Editor adapters, which wrap a foreign snapshot type and therefore
// need a custom SourceText, the language server owns its text outright -- so this container
// just holds whatever SourceText.From/WithChanges produced.
internal sealed class LspSourceTextContainer : SourceTextContainer
{
    private SourceText _current;

    public LspSourceTextContainer(string text)
    {
        ThrowIfNull(text);

        _current = SourceText.From(text);
    }

    public override SourceText Current
    {
        get { return _current; }
    }

    public override event EventHandler<EventArgs>? CurrentChanged;

    public void Replace(string text)
    {
        ThrowIfNull(text);

        SetCurrent(SourceText.From(text));
    }

    // LSP requires content changes to be applied in order, each against the document produced
    // by the previous one. SourceText.WithChanges instead applies a batch against a single
    // snapshot (and rejects overlaps), so the two are NOT equivalent for a multi-change
    // notification -- hence one WithChanges call per change rather than one for the batch.
    public void ApplyChanges(IReadOnlyList<TextDocumentContentChangeEvent> changes)
    {
        ThrowIfNull(changes);

        var text = _current;

        foreach (var change in changes)
        {
            if (change.Range is null)
            {
                // Full-document replacement; discards any preceding incremental changes,
                // which matches how clients emit it.
                text = SourceText.From(change.Text);
            }
            else
            {
                var span = text.ToTextSpan(change.Range);
                text = text.WithChanges(TextChange.ForReplacement(span, change.Text));
            }
        }

        SetCurrent(text);
    }

    private void SetCurrent(SourceText text)
    {
        if (_current == text)
            return;

        _current = text;

        var handler = CurrentChanged;
        handler?.Invoke(this, EventArgs.Empty);
    }
}
