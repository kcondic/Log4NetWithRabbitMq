using System;

namespace HSG.Exception.Logging.ExceptionStructure
{
    public class TransformException
    {
        public Guid ExceptionId { get; set; }
        public string Message { get; set; }
        public string StackTrace { get; set; }
        public TransformException InnerException { get; set; }
        public int Order { get; set; }
    }
}
