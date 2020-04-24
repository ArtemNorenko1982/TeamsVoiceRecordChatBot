using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;

namespace TeamsVoiceRecordChatBot.Application.Dialogs
{
    public class MediaRecordingDialog : CancelAndHelpDialog
    {
        
        public MediaRecordingDialog() : base(nameof(MediaRecordingDialog))
        {
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                HandleIncommingCommandStepAsync,
                ConfirmStepAsync,
                FinalStepAsync
            }));
            // dialog to run
            InitialDialogId = nameof(WaterfallDialog);
        }

        private Task<DialogTurnResult> HandleIncommingCommandStepAsync(WaterfallStepContext stepcontext, CancellationToken cancellationtoken)
        {
            return null;
        }

        private Task<DialogTurnResult> ConfirmStepAsync(WaterfallStepContext stepcontext, CancellationToken cancellationtoken)
        {
            return null;
        }
        private Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepcontext, CancellationToken cancellationtoken)
        {
            return null;
        }
    }
}
