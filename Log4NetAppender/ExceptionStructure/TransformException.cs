using System;
using Newtonsoft.Json;

namespace Log4NetAppender.ExceptionStructure
{
    public class TransformException
    {
        [JsonConstructor]
        private TransformException()
        {
        }

        public TransformException(Exception exceptionToTransform, Guid exceptionGuid, int exceptionOrder)
        {
            ExceptionId = exceptionGuid;
            Message = exceptionToTransform.Message;
            StackTrace = exceptionToTransform.StackTrace;
            InnerException = null;
            Order = exceptionOrder;
        }

        public Guid ExceptionId { get; set; }
        public string Message { get; set; }
        public string StackTrace { get; set; }
        public TransformException InnerException { get; set; }
        public int Order { get; set; }
    }
}
