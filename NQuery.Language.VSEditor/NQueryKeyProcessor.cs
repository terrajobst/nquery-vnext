using System;
using System.Windows.Input;

using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text.Editor;

using NQuery.Language.VSEditor.Completion;
using NQuery.Language.VSEditor.SignatureHelp;

namespace NQuery.Language.VSEditor
{
    internal sealed class NQueryKeyProcessor : KeyProcessor
    {
        private readonly ITextView _textView;
        private readonly IIntellisenseSessionStackMapService _intellisenseSessionStackMapService;
        private readonly ICompletionModelManager _completionModelManager;
        private readonly ISignatureHelpManager _signatureHelpManager;

        public NQueryKeyProcessor(ITextView textView, IIntellisenseSessionStackMapService intellisenseSessionStackMapService, ICompletionModelManager completionModelManager, ISignatureHelpManager signatureHelpManager)
        {
            _textView = textView;
            _intellisenseSessionStackMapService = intellisenseSessionStackMapService;
            _completionModelManager = completionModelManager;
            _signatureHelpManager = signatureHelpManager;
        }

        public override bool IsInterestedInHandledEvents
        {
            get { return true; }
        }

        public override void TextInput(TextCompositionEventArgs args)
        {
            base.TextInput(args);
            _completionModelManager.HandleTextInput(args.Text);
            _signatureHelpManager.HandleTextInput(args.Text);
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

            if (modifiers == ModifierKeys.Control && key == Key.Space)
            {
                CompleteWord();
                args.Handled = true;
            }
            else if (modifiers == ModifierKeys.Control && key == Key.J)
            {
                ListMembers();
                args.Handled = true;
            }
            else if (modifiers == (ModifierKeys.Control | ModifierKeys.Shift) && key == Key.Space)
            {
                ParameterInfo();
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

        private void ListMembers()
        {
            _completionModelManager.TriggerCompletion(false);
        }

        private void CompleteWord()
        {
            _completionModelManager.TriggerCompletion(true);
        }

        private void ParameterInfo()
        {
            _signatureHelpManager.TriggerSignatureHelp();
        }

        private bool ExecuteKeyboardCommandIfSessionActive(IntellisenseKeyboardCommand command)
        {
            var stackForTextView = _intellisenseSessionStackMapService.GetStackForTextView(_textView);
            if (stackForTextView != null)
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

            var target = stackForTextView as IIntellisenseCommandTarget;
            return target != null && target.ExecuteKeyboardCommand(command);
        }
    }
}