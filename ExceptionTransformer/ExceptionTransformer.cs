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
            InnerException = null;
            TimeOfException = DateTime.UtcNow;
        }

        public string Message { get; }
        public string StackTrace { get; }
        public ExceptionTransformer InnerException { get; set; }
        public DateTime TimeOfException { get; }
    }
}
