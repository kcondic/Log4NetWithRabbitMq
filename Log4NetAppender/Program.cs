using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using Log4NetAppender.Consumer;

namespace Log4NetAppender
{
    class Program
    {
        static void Main()
        {
            Console.WriteLine("Čekam poruke...");

            log4net.Config.BasicConfigurator.Configure();
            var logger = LogManager.GetLogger(typeof(Program));

            var repo = new ConsumerRepo();
            repo.DeclareQueue("test", false, new List<string>
            {
                "#"
            });
            repo.ConnectToQueue("test", 8);

            for (var i = 0; i < 100; ++i)
            {
                try
                {
                    throw new InvalidOperationException("ovo je vanjski", new InvalidOperationException("ovo je unutarnji", new InvalidOperationException("ovo je drugi unutarnji")));
                }
                catch (Exception e)
                {
                    logger.Debug(e);
                }
            }
            Console.ReadLine();
        }
    }
}
