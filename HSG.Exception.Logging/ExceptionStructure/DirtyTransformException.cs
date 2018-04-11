using System;
using Newtonsoft.Json;

namespace HSG.Exception.Logging.ExceptionStructure
{
    public class DirtyTransformException
    {
        [JsonConstructor]
        private DirtyTransformException()
        {
        }

        public DirtyTransformException(System.Exception exceptionToTransform, Guid exceptionGuid, int exceptionOrder)
        {
            ExceptionId = exceptionGuid;
            Message = exceptionToTransform.Message;
            StackTrace = exceptionToTransform.StackTrace;
            DirtyInnerException = null;
            Order = exceptionOrder;
        }

        public Guid ExceptionId { get; set; }
        public string Message { get; set; }
        public string StackTrace { get; set; }
        public DirtyTransformException DirtyInnerException { get; set; }
        public int Order { get; set; }
    }
}
