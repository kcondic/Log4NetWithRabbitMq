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
            //log4net.Util.LogLog.InternalDebugging = true;
            log4net.Config.BasicConfigurator.Configure();
            Logger = LogManager.GetLogger(typeof(Program));
            LastException = null;
        }
        private static ILog Logger { get; }
        private static Exception LastException { get; set; }

        public static void ExceptionTrapper(object sender, FirstChanceExceptionEventArgs e)
        {
            //kako izdvojit u nuget
            //routing key -> appname, loglevel
            //razine settat u configu i odredit koliko detalja daje koja razina
            var currentException = e.Exception;

            if (LastException != null //usporedi je li to isti exception
                )
                return;
            Console.WriteLine("Ovo je trenutni: " + currentException);
            Console.WriteLine("Ovo je zadnji: " + LastException);
            LastException = currentException;

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
