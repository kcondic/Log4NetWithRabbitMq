using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;
using log4net;
using Newtonsoft.Json;

namespace Log4NetAppender
{
    public class ExceptionLogger
    {
        public ExceptionLogger()
        {
            log4net.Util.LogLog.InternalDebugging = true;
            log4net.Config.BasicConfigurator.Configure();
            Logger = LogManager.GetLogger(typeof(Program));
        }
        private ILog Logger { get; }
        // iz hgsport kako su napravili http logging
        //private ILog Logger { get; set; }

        //public ExceptionLogger()
        //{
        //    Logger = log4net.LogManager.GetLogger("Unhandled"); prominit u ime appendera

        //}

        //public override async void Log(System.Web.Http.ExceptionHandling.ExceptionLoggerContext context)
        //{
        //    base.Log(context);

        //    string requestBody = null;
        //    if (HttpContext.Current != null)
        //    {
        //        using (var reader = new StreamReader(HttpContext.Current.Request.InputStream))
        //        {
        //            reader.BaseStream.Seek(0, SeekOrigin.Begin);
        //            requestBody = reader.ReadToEnd();
        //        }
        //    }

        //    Logger.Error(Environment.NewLine + context.Request.ToString().Replace("System.Web.Http.WebHost.HttpControllerHandler+LazyStreamContent", requestBody ?? "N/A"), context.Exception);
        //}

        public void UnhandledExceptionTrapper(object sender, FirstChanceExceptionEventArgs e)
        {
            //var transformedException = new ExceptionTransformer.ExceptionTransformer(ex);
            Logger.Error(JsonConvert.SerializeObject(e.Exception));
        }
        //pogledat kako spriječit pucanje uslijed exceptiona
    }
}
