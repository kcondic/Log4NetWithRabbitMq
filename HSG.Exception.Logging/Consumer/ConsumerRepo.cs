using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using HSG.Exception.Logging.Appender;
using HSG.Exception.Logging.ExceptionDatabase;
using HSG.Exception.Logging.ExceptionStructure;
using log4net;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace HSG.Exception.Logging.Consumer
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

            if (rabbitAppenderConfig != null)
                return new ConnectionFactory()
                {
                    HostName = rabbitAppenderConfig.HostName,
                    VirtualHost = rabbitAppenderConfig.VirtualHost,
                    UserName = rabbitAppenderConfig.UserName,
                    Password = rabbitAppenderConfig.Password,
                    RequestedHeartbeat = rabbitAppenderConfig.RequestedHeartbeat,
                    Port = rabbitAppenderConfig.Port
                };

            var manualConfig = ConfigurationManager.AppSettings;

            return new ConnectionFactory()
            {
                HostName = manualConfig["HostName"] ?? "localhost",
                VirtualHost = manualConfig["VirtualHost"] ?? "/",
                UserName = manualConfig["UserName"] ?? "guest",
                Password = manualConfig["Password"] ?? "guest",
                RequestedHeartbeat = manualConfig["RequestedHeartBeat"] != null && 
                                     int.TryParse(manualConfig["RequestedHeartBeat"], out var heartBeat) ? (ushort)heartBeat : (ushort)0,
                Port = manualConfig["Port"] != null && int.TryParse(manualConfig["Port"], out var port) ? port : 5672
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

        private static void DeserializeAndConsume(string messageToConsume, ExceptionContext contextOfThread)
        {
            var deserializedQueueException = JsonConvert.DeserializeObject<QueueException>(messageToConsume);
            var alreadyExistingOccurrencesOfException =
                contextOfThread.QueueExceptions.Where(exception =>
                    exception.Tenent == deserializedQueueException.Tenent &&
                    exception.Environment == deserializedQueueException.Environment &&
                    exception.AppName == deserializedQueueException.AppName &&
                    exception.Status == deserializedQueueException.Status &&
                    exception.Exception.StackTrace == deserializedQueueException.Exception.StackTrace);

            if (alreadyExistingOccurrencesOfException.Any() && alreadyExistingOccurrencesOfException.All(exception => exception.IsAlreadyConsumed))
                return;

            if (alreadyExistingOccurrencesOfException.Count() > 3)
            {
                foreach (var exception in alreadyExistingOccurrencesOfException)
                    exception.IsAlreadyConsumed = true;
                contextOfThread.SaveChanges();
                return;
            }

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
