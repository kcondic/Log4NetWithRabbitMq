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
            System.AppDomain.CurrentDomain.FirstChanceException += new ExceptionLogger().UnhandledExceptionTrapper;

            try
            {
                string str = String.Empty;
                string subStr = str.Substring(0, 4); //this line will create error as the string "str" is empty. 
            }
            catch (Exception e)
            {
                Console.WriteLine("catch blok");
            }
            

            Console.ReadLine();
        }
    }
}
