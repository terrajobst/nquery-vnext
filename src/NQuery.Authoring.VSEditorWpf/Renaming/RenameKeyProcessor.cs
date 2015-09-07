using System;
using System.Windows.Input;

using Microsoft.VisualStudio.Text.Editor;

namespace NQuery.Authoring.VSEditorWpf.Renaming
{
    internal sealed class RenameKeyProcessor : KeyProcessor
    {
        private readonly ITextView _textView;
        private readonly IRenameService _renameService;

        public RenameKeyProcessor(ITextView textView, IRenameService renameService)
        {
            _textView = textView;
            _renameService = renameService;
        }

        public override void PreviewKeyDown(KeyEventArgs args)
        {
            var key = args.Key;
            var modifiers = args.KeyboardDevice.Modifiers;

            if (_renameService.ActiveSession == null)
            {
                if (modifiers == ModifierKeys.None && key == Key.F2)
                {
                    args.Handled = true;

                    var position = _textView.Caret.Position.BufferPosition;
                    _renameService.StartSession(position);
                }
            }
            else
            {
                if (modifiers == ModifierKeys.None && key == Key.Enter)
                {
                    args.Handled = true;
                    _renameService.ActiveSession.Commit();
                }
                else if (modifiers == ModifierKeys.None && key == Key.Escape)
                {
                    args.Handled = true;
                    _renameService.ActiveSession.Cancel();
                }
            }

            base.PreviewKeyDown(args);
        }
    }
}