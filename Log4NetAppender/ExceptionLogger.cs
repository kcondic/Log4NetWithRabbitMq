using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using log4net;
using Newtonsoft.Json;

namespace Log4NetAppender
{
    public class ExceptionLogger
    {
        static ExceptionLogger()
        {
            log4net.Config.BasicConfigurator.Configure();
            Logger = LogManager.GetLogger(typeof(ExceptionLogger));
        }
        private static ILog Logger { get; }

        public static void ExceptionTrapper(object sender, FirstChanceExceptionEventArgs e)
        {
            var currentException = e.Exception;
            var exceptionGuid = Guid.NewGuid();

            var exceptionWithInner = new List<ExceptionTransformer>
            {
                new ExceptionTransformer(currentException, exceptionGuid, 0)
            };

            int.TryParse(ConfigurationManager.AppSettings["DepthOfLog"], out var depthOfLog);
         
            for (var i = 0; i < depthOfLog; i++)
            {
                if (currentException.InnerException == null)
                    break;

                exceptionWithInner.Add(new ExceptionTransformer(currentException.InnerException, exceptionGuid, i+1));
                currentException = currentException.InnerException;
            }

            for (var i = 0; i < exceptionWithInner.Count - 1; ++i)
                exceptionWithInner[i].InnerException = exceptionWithInner[i + 1];

            Logger.Error(JsonConvert.SerializeObject(exceptionWithInner[0]));
        }
    }
}
