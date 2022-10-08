using System.Windows.Input;

using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text.Editor;

using NQuery.Authoring.VSEditorWpf.CodeActions;
using NQuery.Authoring.VSEditorWpf.Commenting;
using NQuery.Authoring.VSEditorWpf.Completion;
using NQuery.Authoring.VSEditorWpf.Highlighting;
using NQuery.Authoring.VSEditorWpf.SignatureHelp;

namespace NQuery.Authoring.VSEditorWpf
{
    internal sealed class NQueryKeyProcessor : KeyProcessor
    {
        private readonly ITextView _textView;
        private readonly IIntellisenseSessionStackMapService _intellisenseSessionStackMapService;
        private readonly ICompletionModelManager _completionModelManager;
        private readonly ISignatureHelpManager _signatureHelpManager;
        private readonly IHighlightingNavigationManager _highlightingNavigationManager;
        private readonly ICodeActionGlyphBroker _codeActionGlyphBroker;
        private readonly ICommentOperations _commentOperations;

        public NQueryKeyProcessor(ITextView textView, IIntellisenseSessionStackMapService intellisenseSessionStackMapService, ICompletionModelManager completionModelManager, ISignatureHelpManager signatureHelpManager, IHighlightingNavigationManager highlightingNavigationManager, ICodeActionGlyphBroker codeActionGlyphBroker, ICommentOperations commentOperations)
        {
            _textView = textView;
            _intellisenseSessionStackMapService = intellisenseSessionStackMapService;
            _completionModelManager = completionModelManager;
            _signatureHelpManager = signatureHelpManager;
            _highlightingNavigationManager = highlightingNavigationManager;
            _codeActionGlyphBroker = codeActionGlyphBroker;
            _commentOperations = commentOperations;
        }

        public override bool IsInterestedInHandledEvents
        {
            get { return true; }
        }

        public override async void TextInput(TextCompositionEventArgs args)
        {
            base.TextInput(args);
            await _completionModelManager.HandleTextInputAsync(args.Text);
            await _signatureHelpManager.HandleTextInputAsync(args.Text);
        }

        public override void PreviewTextInput(TextCompositionEventArgs args)
        {
            base.PreviewTextInput(args);
            _completionModelManager.HandlePreviewTextInput(args.Text);
            _signatureHelpManager.HandlePreviewTextInput(args.Text);
        }

        public override void PreviewKeyDown(KeyEventArgs args)
        {
            var key = args.Key;
            var modifiers = args.KeyboardDevice.Modifiers;

            if (modifiers == ModifierKeys.Control && key == Key.OemPeriod)
            {
                var controller = _codeActionGlyphBroker.GetController(_textView);
                controller.Expand();
            }
            else if (modifiers == ModifierKeys.Control && key == Key.Space)
            {
                CompleteWordAsync();
                args.Handled = true;
            }
            else if (modifiers == ModifierKeys.Control && key == Key.J)
            {
                ListMembersAsync();
                args.Handled = true;
            }
            else if (modifiers == (ModifierKeys.Control | ModifierKeys.Shift) && key == Key.Space)
            {
                ParameterInfoAsync();
                args.Handled = true;
            }
            else if (modifiers == (ModifierKeys.Control | ModifierKeys.Shift) && key == Key.Up)
            {
                NavigateToPreviousHighlight();
                args.Handled = true;
            }
            else if (modifiers == (ModifierKeys.Control | ModifierKeys.Shift) && key == Key.Down)
            {
                NavigateToNextHighlight();
                args.Handled = true;
            }
            else if (modifiers == (ModifierKeys.Control | ModifierKeys.Alt) && key == Key.Oem2)
            {
                _commentOperations.ToggleSingleLineCommentAsync();
                args.Handled = true;
            }
            else if (modifiers == (ModifierKeys.Control | ModifierKeys.Shift) && key == Key.Oem2)
            {
                _commentOperations.ToggleMultiLineCommentAsync();
                args.Handled = true;
            }
            else if (modifiers == ModifierKeys.None)
            {
                switch (key)
                {
                    case Key.Escape:
                        args.Handled = ExecuteKeyboardCommandIfSessionActive(IntellisenseKeyboardCommand.Escape);
                        break;
                    case Key.Up:
                        args.Handled = ExecuteKeyboardCommandIfSessionActive(IntellisenseKeyboardCommand.Up);
                        break;
                    case Key.Down:
                        args.Handled = ExecuteKeyboardCommandIfSessionActive(IntellisenseKeyboardCommand.Down);
                        break;
                    case Key.PageUp:
                        args.Handled = ExecuteKeyboardCommandIfSessionActive(IntellisenseKeyboardCommand.PageUp);
                        break;
                    case Key.PageDown:
                        args.Handled = ExecuteKeyboardCommandIfSessionActive(IntellisenseKeyboardCommand.PageDown);
                        break;
                    case Key.Tab:
                        _completionModelManager.Commit();
                        // Don't eat the key
                        break;
                    case Key.Return:
                        if (_completionModelManager.Commit())
                            args.Handled = true;
                        break;
                }
            }

            base.PreviewKeyDown(args);
        }

        private async void ListMembersAsync()
        {
            await _completionModelManager.TriggerCompletionAsync(false);
        }

        private async void CompleteWordAsync()
        {
            await _completionModelManager.TriggerCompletionAsync(true);
        }

        private async void ParameterInfoAsync()
        {
            await _signatureHelpManager.TriggerSignatureHelpAsync();
        }

        private void NavigateToPreviousHighlight()
        {
            _highlightingNavigationManager.NavigateToPrevious();
        }

        private void NavigateToNextHighlight()
        {
            _highlightingNavigationManager.NavigateToNext();
        }

        private bool ExecuteKeyboardCommandIfSessionActive(IntellisenseKeyboardCommand command)
        {
            var stackForTextView = _intellisenseSessionStackMapService.GetStackForTextView(_textView);
            if (stackForTextView is not null)
            {
                var containsSigHelp = false;
                foreach (var session in stackForTextView.Sessions)
                {
                    if (!containsSigHelp && (session is ISignatureHelpSession))
                    {
                        containsSigHelp = true;
                    }
                    else if (session is ICompletionSession)
                    {
                        if (containsSigHelp)
                            stackForTextView.MoveSessionToTop(session);
                        break;
                    }
                }
            }

            return stackForTextView is IIntellisenseCommandTarget target && target.ExecuteKeyboardCommand(command);
        }
    }
}