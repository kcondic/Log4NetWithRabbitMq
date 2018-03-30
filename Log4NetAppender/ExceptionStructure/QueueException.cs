using System;

namespace Log4NetAppender.ExceptionStructure
{
    public class QueueException
    {
        public QueueException(string tennent, string environment, string appName, string logLevel, TransformException topLevelException)
        {
            Tennent = tennent;
            Environment = environment;
            AppName = appName;
            Status = logLevel;
            Exception = topLevelException;
            TimeOfException = DateTime.UtcNow;
        }

        public string Tennent { get; }
        public string Environment { get; }
        public string AppName { get; }
        public string Status { get; }
        public TransformException Exception { get; }
        public DateTime TimeOfException { get; }
    }
}
