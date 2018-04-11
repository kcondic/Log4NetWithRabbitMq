using System;
using System.Collections.Generic;

namespace HSG.Exception.Logging.ExceptionStructure
{
    public class QueueException
    {
        public Guid ExceptionId { get; set; }
        public string Tenent { get; set; }
        public string Environment { get; set; }
        public string AppName { get; set; }
        public string Status { get; set; }
        public TransformException Exception { get; set; }
        public DateTime LastTimeOfException { get; set; }
        public int Counter { get; set; }
        public ICollection<HistoricalQueueException> HistoricalExceptions { get; set; }
    }
}
