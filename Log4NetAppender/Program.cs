using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using log4net.Config;
using Log4NetAppender.Appender;
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
            DeclareQueueAndConnect("test", false, new List<string>
            {
                "#"
            });
            Console.ReadLine();

            //log4net.Config.BasicConfigurator.Configure();
            //var logger = LogManager.GetLogger(typeof(Program));

            //try
            //{
            //    throw new InvalidOperationException("ovo je vanjski", new InvalidOperationException("ovo je unutarnji", new InvalidOperationException("ovo je drugi unutarnji")));
            //}
            //catch (Exception e)
            //{
            //    logger.Debug(e);
            //}

            //try
            //{
            //    throw new InvalidOperationException("ovo je vanjski", new InvalidOperationException("ovo je unutarnji", new InvalidOperationException("ovo je drugi unutarnji")));
            //}
            //catch (Exception e)
            //{
            //    logger.Error(e);
            //}

            //try
            //{
            //    throw new InvalidOperationException("ovo je vanjski", new InvalidOperationException("ovo je unutarnji", new InvalidOperationException("ovo je drugi unutarnji")));
            //}
            //catch (Exception e)
            //{
            //    logger.Info(e);
            //}

            //try
            //{
            //    throw new InvalidOperationException("ovo je vanjski", new InvalidOperationException("ovo je unutarnji", new InvalidOperationException("ovo je drugi unutarnji")));
            //}
            //catch (Exception e)
            //{
            //    logger.Fatal(e);
            //}

            //try
            //{
            //    throw new InvalidOperationException("ovo je vanjski", new InvalidOperationException("ovo je unutarnji", new InvalidOperationException("ovo je drugi unutarnji")));
            //}
            //catch (Exception e)
            //{
            //    logger.Warn(e);
            //}

            Console.ReadLine();
        }

        static void DeclareQueueAndConnect(string queueName, bool willDeleteAfterConnectionClose, IEnumerable<string> routingKeys)
        {
            var rabbitAppenderConfig = LogManager.GetRepository()
                                                 .GetAppenders()
                                                 .SingleOrDefault(appender => appender.Name == "RabbitMqAppender") as RabbitMqAppender;
            if (rabbitAppenderConfig == null)
                return;

            var factory = new ConnectionFactory()
            {
                HostName = rabbitAppenderConfig.HostName,
                VirtualHost = rabbitAppenderConfig.VirtualHost,
                UserName = rabbitAppenderConfig.UserName,
                Password = rabbitAppenderConfig.Password,
                RequestedHeartbeat = rabbitAppenderConfig.RequestedHeartbeat,
                Port = rabbitAppenderConfig.Port
            };

            using (var connection = factory.CreateConnection())
                using (var channel = connection.CreateModel())
                {
                    channel.ExchangeDeclare("HattrickExchange", "topic");
                    channel.QueueDeclare(queueName, true, willDeleteAfterConnectionClose, false,
                        new Dictionary<string, object>
                        {
                            {"x-queue-mode", "lazy"},
                            {"x-max-length-bytes", 100000000},
                            {"x-message-ttl", 172800000}
                        });

                    foreach (var routingKey in routingKeys)
                        channel.QueueBind(queueName, "HattrickExchange", routingKey);

                    var consumer = new EventingBasicConsumer(channel);
                    consumer.Received += (model, ea) =>
                    {
                        var body = ea.Body;
                        var message = Encoding.UTF8.GetString(body);
                        var msgRoutingKey = ea.RoutingKey;
                        Console.WriteLine(" Primljena ruta: '{0}' \nPoruka: '{1}'", msgRoutingKey, message);
                    };
                    //odredit kako se konzumira
                    channel.BasicConsume(queueName, true, consumer);
            }
        }
    }
}
