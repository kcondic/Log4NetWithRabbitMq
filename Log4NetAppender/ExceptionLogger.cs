using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using log4net;
using Newtonsoft.Json;

namespace Log4NetAppender
{
    public class ExceptionLogger
    {
        static ExceptionLogger()
        {
            log4net.Config.BasicConfigurator.Configure();
            Logger = LogManager.GetLogger("RabbitMqAppender");
        }
        private static ILog Logger { get; }

        public static void ExceptionTrapper(object sender, FirstChanceExceptionEventArgs e)
        {
            var currentException = e.Exception;
            var exceptionWithInner = new List<ExceptionTransformer.ExceptionTransformer>
            {
                new ExceptionTransformer.ExceptionTransformer(currentException)
            };

            while (currentException.InnerException != null)
            {
                exceptionWithInner.Add(new ExceptionTransformer.ExceptionTransformer(currentException.InnerException));
                currentException = currentException.InnerException;
            }

            for (var i = 0; i < exceptionWithInner.Count - 1; ++i)
                exceptionWithInner[i].InnerException = exceptionWithInner[i + 1];

            Logger.Error(JsonConvert.SerializeObject(exceptionWithInner[0]));
        }
    }
}
