using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using Log4NetAppender.Appender;
using Log4NetAppender.ExceptionDatabase;
using Log4NetAppender.ExceptionStructure;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Log4NetAppender.Consumer
{
    public class ConsumerRepo
    {
        public ConsumerRepo()
        {
            _connectionFactory = GetFactory();
            _connection = GetConnection();
            _channel = _connection.CreateModel();
        }

        private readonly IConnectionFactory _connectionFactory;
        private readonly IConnection _connection;
        private readonly IModel _channel;

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
                    Password = "guest",
                    RequestedHeartbeat = 0,
                    Port = 5672
                };

            return new ConnectionFactory()
            {
                HostName = rabbitAppenderConfig.HostName ?? "localhost",
                VirtualHost = rabbitAppenderConfig.VirtualHost ?? "/",
                UserName = rabbitAppenderConfig.UserName ?? "guest",
                Password = rabbitAppenderConfig.Password ?? "testtest",
                RequestedHeartbeat = rabbitAppenderConfig.RequestedHeartbeat,
                Port = rabbitAppenderConfig.Port != 0 ? rabbitAppenderConfig.Port : 5672
            };
        }

        private IConnection GetConnection()
        {
            return _connectionFactory.CreateConnection();
        }

        public void DeclareQueue(string queueName, bool willDeleteAfterConnectionClose, IEnumerable<string> routingKeys)
        {
            _channel.ExchangeDeclare("HattrickExchange", "topic", true);
            _channel.QueueDeclare(queueName, true, willDeleteAfterConnectionClose, false,
                new Dictionary<string, object>
                {
                        {"x-queue-mode", "lazy"},
                        {"x-max-length-bytes", 100000000},
                        {"x-message-ttl", 172800000}
                });

            foreach (var routingKey in routingKeys)
                _channel.QueueBind(queueName, "HattrickExchange", routingKey);
        }

        public void ConnectToQueue(string queueToConnectToName, int numberOfThreads=1)
        {
            for (var i = 0; i < numberOfThreads; ++i)
            {
                var channel = _connection.CreateModel();
                var context = new ExceptionContext();
                channel.BasicQos(0, 1, false);
                new Thread(() =>
                {
                    Thread.CurrentThread.IsBackground = true;
                    var consumer = new EventingBasicConsumer(channel);
                    consumer.Received += (model, ea) =>
                    {
                        var message = Encoding.UTF8.GetString(ea.Body);
                        Console.WriteLine("IDE IDE");
                        DeserializeAndConsume(message, context);
                        channel.BasicAck(ea.DeliveryTag, false);
                    };
                    channel.BasicConsume(queueToConnectToName, false, consumer);
                }).Start();
            }
        }

        public void DisconnectFromQueue(string consumerToDisconnectTag)
        {
            _channel.BasicCancel(consumerToDisconnectTag);
        }

        private void DeserializeAndConsume(string messageToConsume, ExceptionContext contextOfThread)
        {
            var deserializedQueueException = JsonConvert.DeserializeObject<QueueException>(messageToConsume);
            //if (deserializedQueueException.Equals(_lastQueueException))
            //    return;
            //_lastQueueException = deserializedQueueException;
            var topLevelException = deserializedQueueException.Exception;
            while (topLevelException.InnerException != null)
            {
                contextOfThread.TransformExceptions.Add(topLevelException);
                topLevelException = topLevelException.InnerException;
            }
            contextOfThread.QueueExceptions.Add(deserializedQueueException);
            contextOfThread.SaveChanges();
        }

        public void DeleteQueue(string queueToDeleteName)
        {
            _channel.QueueDelete(queueToDeleteName, false, true);
        }

        public void CloseConnection()
        {
            _connection.Close();
        }
    }
}
