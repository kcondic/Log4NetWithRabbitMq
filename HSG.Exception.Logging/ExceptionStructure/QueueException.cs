using System;
using Newtonsoft.Json;

namespace HSG.Exception.Logging.ExceptionStructure
{
    public class QueueException
    {
        [JsonConstructor]
        private QueueException()
        {           
        }

        public QueueException(string tenent, string environment, string appName, string logLevel, TransformException topLevelException)
        {
            ExceptionId = topLevelException.ExceptionId;
            Tenent = tenent;
            Environment = environment;
            AppName = appName;
            Status = logLevel;
            Exception = topLevelException;
            TimeOfException = DateTime.UtcNow;
        }

        // setters needed for JSON deserialization
        public Guid ExceptionId { get; set; }
        public string Tenent { get; set; }
        public string Environment { get; set; }
        public string AppName { get; set; }
        public string Status { get; set; }
        public TransformException Exception { get; set; }
        public DateTime TimeOfException { get; set; }
    }
}
