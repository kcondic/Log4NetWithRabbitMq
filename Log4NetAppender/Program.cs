using System;
using System.CodeDom;
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
            System.AppDomain.CurrentDomain.FirstChanceException += ExceptionLogger.ExceptionTrapper;

            ThrowException();
            ThrowException();
            ThrowException();
            
            Console.ReadLine();
        }

        static void ThrowException()
        {
            try
            {
                throw new InvalidOperationException("ovo je vanjski", new InvalidOperationException("ovo je unutarnji", new InvalidOperationException("ovo je drugi unutarnji")));
            }
            catch (Exception e)
            {
                Console.WriteLine("catch blok");
            }
        }
    }
}
