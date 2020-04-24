using System.Collections.Generic;

namespace Microsoft.Teams.Integration.Common.Details
{
    public class ChanelDetails
    {
        public string ChanelTitle { get; set; }
        public IEnumerable<ChanelParticipantDetails> Participants { get; set; }
    }
}
