#nullable enable

namespace DEH.Domain.LogAnalytics;
   
    public class DEHAnalyticsJobLogs
    {
        public string? JobId { get; set; }
        public string? JobStatus { get; set; }
        public string? ErrorMessage { get; set; }
        public string? TimeStamp { get; set; }
    }

