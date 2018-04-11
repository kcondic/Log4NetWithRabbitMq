using System;
using Newtonsoft.Json;

namespace HSG.Exception.Logging.ExceptionStructure
{
    public class DirtyQueueException
    {
        [JsonConstructor]
        private DirtyQueueException()
        {
        }

        public DirtyQueueException(string tenent, string environment, string appName, string logLevel, DirtyTransformException topLevelException)
        {
            ExceptionId = topLevelException.ExceptionId;
            Tenent = tenent;
            Environment = environment;
            AppName = appName;
            Status = logLevel;
            DirtyException = topLevelException;
            TimeOfException = DateTime.UtcNow;
        }

        public Guid ExceptionId { get; set; }
        public string Tenent { get; set; }
        public string Environment { get; set; }
        public string AppName { get; set; }
        public string Status { get; set; }
        public DirtyTransformException DirtyException { get; set; }
        public DateTime TimeOfException { get; set; }
    }
}
