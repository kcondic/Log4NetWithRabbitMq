﻿using System;
using System.Collections.Generic;
using HSG.Exception.Logging.Consumer;
using log4net;

namespace Log4NetAppender
{
    class Program
    {
        static void Main()
        {
            Console.WriteLine("Čekam poruke...");

            log4net.Config.BasicConfigurator.Configure();
            var logger = LogManager.GetLogger("ExceptionLogger");

            var repo = new ConsumerRepo();
            repo.DeclareQueue("test", false, new List<string>
            {
                "#"
            });
            repo.ConnectToQueue("test", 8);

            for (var i = 0; i < 10; ++i)
            {
                try
                {
                    throw new InvalidOperationException("ovo je vanjski", new InvalidOperationException("ovo je unutarnji", new InvalidOperationException("ovo je drugi unutarnji")));
                }
                catch (Exception e)
                {
                    logger.Error(e);
                }
            }
            Console.ReadLine();
        }
    }
}
