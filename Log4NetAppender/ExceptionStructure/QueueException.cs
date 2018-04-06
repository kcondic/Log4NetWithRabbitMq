using System;
using Newtonsoft.Json;

namespace Log4NetAppender.ExceptionStructure
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

        public override bool Equals(object exceptionToCheck)
        {
            if (exceptionToCheck == null || GetType() != exceptionToCheck.GetType())
                return false;

            var queueException = (QueueException)exceptionToCheck;

            return queueException.Tenent == Tenent 
                    && queueException.Environment == Environment 
                    && queueException.AppName == AppName 
                    && queueException.Status == Status 
                    && Math.Abs(queueException.TimeOfException.Subtract(TimeOfException).Milliseconds) <= 500 
                    && queueException.Exception.Message == Exception.Message 
                    && queueException.Exception.StackTrace == Exception.StackTrace;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = ExceptionId.GetHashCode();
                hashCode = (hashCode * 397) ^ (Tenent != null ? Tenent.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Environment != null ? Environment.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (AppName != null ? AppName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Status != null ? Status.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Exception != null ? Exception.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ TimeOfException.GetHashCode();
                return hashCode;
            }
        }
    }
}
