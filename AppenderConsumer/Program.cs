using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace AppenderConsumer
{
    class Program
    {
        static void Main()
        {
            var factory = new ConnectionFactory() {
                                                    HostName = "localhost",
                                                    UserName = "guest",
                                                    Password = "testtest"
                                                  };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: "HattrickExchange", type: "topic");
                var queueName = "test";
                var willDeleteAfterConnection = false;
                var routingKeys = new List<string>
                {
                    "consumer.test",
                    "test.test"
                };
                //omogućit korisniku biranje imena queuea i oće li se brisat nakon gašenja veze
                channel.QueueDeclare(queueName, true, willDeleteAfterConnection, false, new Dictionary<string, object>
                {
                    { "x-queue-mode", "lazy" }
                });

                foreach (var routingKey in routingKeys)
                    channel.QueueBind(queueName, "HattrickExchange", routingKey);

                Console.WriteLine("Čekam poruke...");

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body;
                    var message = Encoding.UTF8.GetString(body);
                    var msgRoutingKey = ea.RoutingKey;
                    Console.WriteLine(" Primljena ruta: '{0}' \nPoruka: '{1}'", msgRoutingKey, message);
                };
                channel.BasicConsume(queueName, true, consumer);
                Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();

                //istraži razliku topic i header exchangea
            }
        }
    }
}
