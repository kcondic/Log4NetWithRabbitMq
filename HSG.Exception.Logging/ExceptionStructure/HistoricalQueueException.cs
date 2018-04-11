using System;

namespace HSG.Exception.Logging.ExceptionStructure
{
    public class HistoricalQueueException
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public DateTime TimeOfException { get; set; }
        public Guid QueueExceptionId { get; set; }
        public QueueException QueueException { get; set; }
    }
}
