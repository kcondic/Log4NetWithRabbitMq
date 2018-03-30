using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using Newtonsoft.Json;

namespace Log4NetAppender
{
    class Program
    {
        static void Main(string[] args)
        {
            log4net.Config.BasicConfigurator.Configure();
            var logger = LogManager.GetLogger(typeof(Program));

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

            Console.ReadLine();
        }
    }
}
