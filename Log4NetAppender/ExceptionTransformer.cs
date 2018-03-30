using System;

namespace Log4NetAppender
{
    public class ExceptionTransformer
    {
        public ExceptionTransformer(Exception exceptionToTransform, Guid exceptionGuid, int exceptionOrder)
        {
            ExceptionId = exceptionGuid;
            Message = exceptionToTransform.Message;
            StackTrace = exceptionToTransform.StackTrace;
            InnerException = null;
            TimeOfException = DateTime.UtcNow;
            Order = exceptionOrder;
        }

        public Guid ExceptionId { get; }
        public string Message { get; }
        public string StackTrace { get; }
        public ExceptionTransformer InnerException { get; set; }
        public DateTime TimeOfException { get; }
        public int Order { get; }
    }
}
