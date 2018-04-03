using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using log4net.Config;
using Log4NetAppender.Appender;
using Log4NetAppender.Consumer;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Log4NetAppender
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Čekam poruke...");
            Console.WriteLine(" Press [enter] to exit.");

            log4net.Config.BasicConfigurator.Configure();
            var logger = LogManager.GetLogger(typeof(Program));

            ConsumerRepo.DeclareQueue("test", false, new List<string>
            {
                "#"
            });
            ConsumerRepo.ConnectToQueue("test");

            try
            {
                throw new InvalidOperationException("ovo je vanjski", new InvalidOperationException("ovo je unutarnji", new InvalidOperationException("ovo je drugi unutarnji")));
            }
            catch (Exception e)
            {
                logger.Debug(e);
            }

            try
            {
                throw new InvalidOperationException("ovo je vanjski", new InvalidOperationException("ovo je unutarnji", new InvalidOperationException("ovo je drugi unutarnji")));
            }
            catch (Exception e)
            {
                logger.Error(e);
            }

            try
            {
                throw new InvalidOperationException("ovo je vanjski", new InvalidOperationException("ovo je unutarnji", new InvalidOperationException("ovo je drugi unutarnji")));
            }
            catch (Exception e)
            {
                logger.Info(e);
            }

            try
            {
                throw new InvalidOperationException("ovo je vanjski", new InvalidOperationException("ovo je unutarnji", new InvalidOperationException("ovo je drugi unutarnji")));
            }
            catch (Exception e)
            {
                logger.Fatal(e);
            }

            try
            {
                throw new InvalidOperationException("ovo je vanjski", new InvalidOperationException("ovo je unutarnji", new InvalidOperationException("ovo je drugi unutarnji")));
            }
            catch (Exception e)
            {
                logger.Warn(e);
            }

            try
            {
                throw new InvalidOperationException("ovo je vanjski", new InvalidOperationException("ovo je unutarnji", new InvalidOperationException("ovo je drugi unutarnji")));
            }
            catch (Exception e)
            {
                logger.Fatal(e);
            }

            try
            {
                throw new InvalidOperationException("ovo je vanjski", new InvalidOperationException("ovo je unutarnji", new InvalidOperationException("ovo je drugi unutarnji")));
            }
            catch (Exception e)
            {
                logger.Warn(e);
            }

            Console.ReadLine();
        }
    }
}
