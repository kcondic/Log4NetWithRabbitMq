using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Log4NetAppender
{
    class Program
    {
        static void Main(string[] args)
        {
            log4net.Util.LogLog.InternalDebugging = true;
            log4net.Config.BasicConfigurator.Configure();
            log4net.ILog log = log4net.LogManager.GetLogger(typeof(Program));
            try
            {
                string str = String.Empty;
                string subStr = str.Substring(0, 4); //this line will create error as the string "str" is empty.  
            }
            catch (Exception ex)
            {
                //var transformedException = new ExceptionTransformer.ExceptionTransformer(ex);
                log.Error(JsonConvert.SerializeObject(ex));
            }

            Console.ReadLine();
        }
    }
}
