using System;
using System.Media;
using System.Windows.Input;

using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

using System.Linq;

namespace NQuery.Language.VSEditor
{
    internal sealed class NQueryKeyProcessor : KeyProcessor
    {
        private readonly ITextView _textView;
        private readonly ICompletionBroker _completionBroker;
        private readonly IIntellisenseSessionStackMapService _intellisenseSessionStackMapService;

        private ICompletionSession _session;
        private ITextSnapshot _lastSnapshot;

        public NQueryKeyProcessor(ITextView textView, ICompletionBroker completionBroker, IIntellisenseSessionStackMapService intellisenseSessionStackMapService)
        {
            _textView = textView;
            _completionBroker = completionBroker;
            _intellisenseSessionStackMapService = intellisenseSessionStackMapService;

            _textView.TextBuffer.PostChanged += TextBufferOnPostChanged;
            _lastSnapshot = _textView.TextBuffer.CurrentSnapshot;
        }

        private void TextBufferOnPostChanged(object sender, EventArgs eventArgs)
        {
            var lastSnapshot = _lastSnapshot;
            var snapshot = _textView.TextBuffer.CurrentSnapshot;
            _lastSnapshot = snapshot;

            var changes = lastSnapshot.Version.Changes;
            if (changes.Count == 1)
            {
                var change = changes[0];
                if (change.OldLength == 0 && change.NewLength == 1)
                {
                    var c = change.NewText[0];

                    if (_session == null && IsTriggerChar(c))
                        ListMembers();
                }
            }
        }

        public override void PreviewTextInput(TextCompositionEventArgs args)
        {
            if (_session != null && args.Text.Any(IsCommitChar))
                _session.Commit();

            base.PreviewTextInput(args);
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
                        if (_session != null)
                            _session.Commit();
                        // Don't eat the key
                        break;
                    case Key.Return:
                        if (_session != null)
                        {
                            _session.Commit();
                            args.Handled = true;
                        }
                        break;
                }
            }

            base.PreviewKeyDown(args);
        }

        private static bool IsTriggerChar(char c)
        {
            return char.IsLetter(c) ||
                   c == '_' ||
                   c == '.';
        }

        private static bool IsCommitChar(char c)
        {
            return char.IsWhiteSpace(c) ||
                   c == '\t' ||
                   c == '.' ||
                   c == ',' ||
                   c == '(';
        }

        private void ListMembers()
        {
            if (_session != null)
            {
                _session.Dismiss();
            }
            else
            {
                _session = _completionBroker.TriggerCompletion(_textView);
                _session.Dismissed += SessionOnDismissed;
            }
        }

        private void CompleteWord()
        {
            if (_session != null)
            {
                _session.Dismiss();
            }
            else
            {
                _session = _completionBroker.TriggerCompletion(_textView);
                _session.Dismissed += SessionOnDismissed;
            }
        }

        private void ParameterInfo()
        {
            SystemSounds.Hand.Play();
        }

        private void SessionOnDismissed(object sender, EventArgs e)
        {
            _session = null;
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