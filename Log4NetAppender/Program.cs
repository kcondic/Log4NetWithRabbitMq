using System;
using System.Collections.Generic;
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

            ConsumerRepo.DeclareQueue("test", false, new List<string>
            {
                "#"
            });
            ConsumerRepo.ConnectToQueue("test", 8);

            while (true)
            {
                try
                {
                    throw new InvalidOperationException("ovo je vanjski", new InvalidOperationException("ovo je unutarnji", new InvalidOperationException("ovo je drugi unutarnji")));
                }
                catch (Exception e)
                {
                    logger.Debug(e);
                    System.Threading.Thread.Sleep(1000);
                }
            }
            Console.ReadLine();
        }
    }
}
