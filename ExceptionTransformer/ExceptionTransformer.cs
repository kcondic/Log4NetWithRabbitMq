using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExceptionTransformer
{
    public class ExceptionTransformer
    {
        public ExceptionTransformer(Exception exceptionToTransform)
        {
            Message = exceptionToTransform.Message;
            StackTrace = exceptionToTransform.StackTrace;
            InnerException = exceptionToTransform.InnerException;
            TimeOfException = DateTime.UtcNow;
        }

        public string Message { get; }
        public string StackTrace { get; }
        public Exception InnerException { get; }
        public DateTime TimeOfException { get; }
    }
}
