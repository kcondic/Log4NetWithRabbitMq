using System.Collections.Generic;
using System.Collections.Specialized;
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
            _rabbitAppenderConfig = LogManager.GetRepository()
                                              .GetAppenders()
                                              .SingleOrDefault(appender => appender.Name == "RabbitMqAppender") as RabbitMqAppender;
            _manualConfig = ConfigurationManager.AppSettings;
            _connectionFactory = GetFactory();
            _connection = GetConnection();
            _channel = _connection.CreateModel();
        }

        private readonly RabbitMqAppender _rabbitAppenderConfig;
        private readonly NameValueCollection _manualConfig;
        private readonly IConnectionFactory _connectionFactory;
        private readonly IConnection _connection;
        private readonly IModel _channel;

        private IConnectionFactory GetFactory()
        {
            if (_rabbitAppenderConfig != null)
                return new ConnectionFactory()
                {
                    HostName = _rabbitAppenderConfig.HostName,
                    VirtualHost = _rabbitAppenderConfig.VirtualHost,
                    UserName = _rabbitAppenderConfig.UserName,
                    Password = _rabbitAppenderConfig.Password,
                    RequestedHeartbeat = _rabbitAppenderConfig.RequestedHeartbeat,
                    Port = _rabbitAppenderConfig.Port
                };

            return new ConnectionFactory()
            {
                HostName = _manualConfig["HostName"] ?? "localhost",
                VirtualHost = _manualConfig["VirtualHost"] ?? "/",
                UserName = _manualConfig["UserName"] ?? "guest",
                Password = _manualConfig["Password"] ?? "guest",
                RequestedHeartbeat = _manualConfig["RequestedHeartBeat"] != null && 
                                     int.TryParse(_manualConfig["RequestedHeartBeat"], out var heartBeat) ? (ushort)heartBeat : (ushort)0,
                Port = _manualConfig["Port"] != null && int.TryParse(_manualConfig["Port"], out var port) ? port : 5672
            };
        }

        private IConnection GetConnection()
        {
            return _connectionFactory.CreateConnection();
        }

        public void DeclareQueue(string queueName, bool willDeleteAfterConnectionClose, IEnumerable<string> routingKeys)
        {
            var rabbitAppenderConfig = LogManager.GetRepository()
                                                 .GetAppenders()
                                                 .SingleOrDefault(appender => appender.Name == "RabbitMqAppender") as RabbitMqAppender;
            var manualConfig = ConfigurationManager.AppSettings;
            var exchangeName = rabbitAppenderConfig != null ? rabbitAppenderConfig.ExchangeName :  manualConfig["ExchangeName"] ?? "HattrickExchange";
            _channel.ExchangeDeclare(exchangeName, "topic", true);
            _channel.QueueDeclare(queueName, true, willDeleteAfterConnectionClose, false,
                new Dictionary<string, object>
                {
                        {"x-queue-mode", "lazy"},
                        {"x-max-length-bytes", 100000000},
                        {"x-message-ttl", 172800000}
                });

            foreach (var routingKey in routingKeys)
                _channel.QueueBind(queueName, exchangeName, routingKey);
        }

        public void ConnectToQueue(string queueToConnectToName, int numberOfThreads=1)
        {
            for (var i = 0; i < numberOfThreads; i++)
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
            var deserializedDirtyQueueException = JsonConvert.DeserializeObject<DirtyQueueException>(messageToConsume);
            var topLevelException = deserializedDirtyQueueException.DirtyException;
            while (topLevelException.DirtyInnerException != null)
            {
                contextOfThread.DirtyTransformExceptions.Add(topLevelException);
                topLevelException = topLevelException.DirtyInnerException;
            }
            contextOfThread.DirtyQueueExceptions.Add(deserializedDirtyQueueException);
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
