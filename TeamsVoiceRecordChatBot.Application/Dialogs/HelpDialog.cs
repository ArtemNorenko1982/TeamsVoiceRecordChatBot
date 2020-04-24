using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TeamsVoiceRecordChatBot.Application.Dialogs
{
    public class HelpDialog : CancelAndHelpDialog
    {
        public HelpDialog() : base(nameof(HelpDialog))
        {
        }
    }
}
