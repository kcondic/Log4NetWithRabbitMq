using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using Log4NetAppender.Appender;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Log4NetAppender.Consumer
{
    public static class ConsumerRepo
    {
        static ConsumerRepo()
        {
            ConnectionFactory = GetFactory();
            Connection = CreateConnection();
            Channel = CreateModel();
        }

        private static readonly IConnectionFactory ConnectionFactory;
        private static readonly IConnection Connection;
        private static readonly IModel Channel;

        private static IConnectionFactory GetFactory()
        {
            var rabbitAppenderConfig = LogManager.GetRepository()
                                                 .GetAppenders()
                                                 .SingleOrDefault(appender => appender.Name == "RabbitMqAppender") as RabbitMqAppender;

            if (rabbitAppenderConfig == null)
                return new ConnectionFactory()
                {
                    HostName = "localhost",
                    VirtualHost = "/",
                    UserName = "guest",
                    Password = "testtest",
                    RequestedHeartbeat = 0,
                    Port = 5672
                };

            return new ConnectionFactory()
            {
                HostName = rabbitAppenderConfig.HostName ?? "localhost",
                VirtualHost = rabbitAppenderConfig.VirtualHost ?? "/",
                UserName = rabbitAppenderConfig.UserName ?? "guest",
                Password = rabbitAppenderConfig.Password ?? "guest",
                RequestedHeartbeat = rabbitAppenderConfig.RequestedHeartbeat,
                Port = rabbitAppenderConfig.Port != 0 ? rabbitAppenderConfig.Port : 5672
            };
        }

        private static IConnection CreateConnection()
        {
            return ConnectionFactory.CreateConnection();
        }

        private static IModel CreateModel()
        {
            return Connection.CreateModel();
        }

        public static void DeclareQueue(string queueName, bool willDeleteAfterConnectionClose, IEnumerable<string> routingKeys)
        {
            Channel.ExchangeDeclare("HattrickExchange", "topic");
            Channel.QueueDeclare(queueName, true, willDeleteAfterConnectionClose, false,
                new Dictionary<string, object>
                {
                        {"x-queue-mode", "lazy"},
                        {"x-max-length-bytes", 100000000},
                        {"x-message-ttl", 172800000}
                });

            foreach (var routingKey in routingKeys)
                Channel.QueueBind(queueName, "HattrickExchange", routingKey);
        }

        public static string ConnectToQueue(string queueToConnectToName)
        {
            var consumer = new EventingBasicConsumer(Channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body;
                var message = Encoding.UTF8.GetString(body);
                var msgRoutingKey = ea.RoutingKey;
                Console.WriteLine(" Primljena ruta: '{0}' \nPoruka: '{1}'", msgRoutingKey, message);
                DeserializeAndConsume(message);
            };
            var consumerTag = Channel.BasicConsume(queueToConnectToName, true, consumer);
            return consumerTag;
        }

        public static void DisconnectFromQueue(string consumerToDisconnectTag)
        {
            Channel.BasicCancel(consumerToDisconnectTag);
        }

        private static void DeserializeAndConsume(string messageToConsume)
        {
            return;
        }

        public static void DeleteQueue(string queueToDeleteName)
        {
            Channel.QueueDelete(queueToDeleteName, false, true);
        }

        public static void CloseConnection()
        {
            Connection.Close();
        }
    }
}
