using System;

namespace Log4NetAppender.ExceptionStructure
{
    public class TransformException
    {
        public TransformException(Exception exceptionToTransform, Guid exceptionGuid, int exceptionOrder)
        {
            ExceptionId = exceptionGuid;
            Message = exceptionToTransform.Message;
            StackTrace = exceptionToTransform.StackTrace;
            InnerException = null;
            Order = exceptionOrder;
        }

        public Guid ExceptionId { get; }
        public string Message { get; }
        public string StackTrace { get; }
        public TransformException InnerException { get; set; }
        public int Order { get; }
    }
}
