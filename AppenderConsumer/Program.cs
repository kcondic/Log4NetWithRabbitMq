using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace AppenderConsumer
{
    class Program
    {
        static void Main(string[] args)
        {
            //prebacit sve u generalni slučaj
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: "test", type: "topic");
                //omogućit korisniku biranje imena queuea i oće li se brisat nakon gašenja veze
                channel.QueueDeclare("test", true, false, false, new Dictionary<string, object>
                {
                    { "x-queue-mode", "lazy" }
                });
                channel.QueueBind(queue: "test", exchange: "test", routingKey: "*.test");
                // ovdi sva bindanja koja ti tribaju
                Console.WriteLine(" [*] Waiting for messages. To exit press CTRL+C");

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body;
                    var message = Encoding.UTF8.GetString(body);
                    var routingKey = ea.RoutingKey;
                    Console.WriteLine(" [x] Received '{0}':'{1}'",
                        routingKey,
                        message);
                };
                channel.BasicConsume(queue: "test",
                    autoAck: true,
                    consumer: consumer);

                Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();
            }
        }
    }
}
