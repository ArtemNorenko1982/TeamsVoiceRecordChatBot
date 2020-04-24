using System;
using Microsoft.Teams.Integration.Common.Details;

namespace Microsoft.Teams.Integration.Common
{
    public class RecordDetails
    {
        public DateTime ScheduleDate { get; set; }
        public ChanelDetails Chanel { get; set; }
        public string RecordName { get; set; }
        public DateTime RecordDate { get; set; }
        public string RecordStoragePath { get; set; }
        public string RecordTopic { get; set; }
        public string MediaTypeFormat { get; set; }
    }
}
