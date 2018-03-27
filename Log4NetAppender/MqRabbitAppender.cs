using System;
using log4net.Core;
using log4net.Util;
using RabbitMQ.Client;
using System.Diagnostics;
using System.Text;
using log4net.Appender;

namespace Log4NetAppender
{
    public class MqRabbitAppender : AppenderSkeleton
    {
        private ConnectionFactory _connectionFactory;
        private WorkerThread<LoggingEvent> _worker;
        public MqRabbitAppender()
            {
                HostName = "localhost";
                VirtualHost = "/";
                UserName = "guest";
                Password = "guest";
                RequestedHeartbeat = 0;
                Port = 5672;
                RoutingKey = "";
                FlushInterval = 5;
            }

        public string HostName { get; set; }
        public string VirtualHost { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public ushort RequestedHeartbeat { get; set; }
        public int Port { get; set; }
        public string RoutingKey { get; set; }
        public int FlushInterval { get; set; }

        protected override void OnClose()
        {
            _worker.Dispose();
            _worker = null;
        }

        protected override void Append(LoggingEvent loggingEvent)
        {
            loggingEvent.Fix = FixFlags.All;
            _worker.Enqueue(loggingEvent);
        }

        public override void ActivateOptions()
        {
            _connectionFactory = new ConnectionFactory()
            {
                HostName = HostName,
                VirtualHost = VirtualHost,
                UserName = UserName,
                Password = Password,
                RequestedHeartbeat = RequestedHeartbeat,
                Port = Port
            };
            _worker = new WorkerThread<LoggingEvent>("Worker for log4net appender '" + Name + "'", TimeSpan.FromSeconds((double)FlushInterval), Process);
        }

        public void Process(LoggingEvent[] logs)
        {
            var factory = new ConnectionFactory() {HostName = "localhost", UserName = UserName, Password = Password };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare("HattrickExchange", "topic");
                foreach (var log in logs)
                {
                    var body = Encoding.UTF8.GetBytes(log.RenderedMessage);
                    channel.BasicPublish("HattrickExchange", RoutingKey, null, body);
                    Console.WriteLine(" Ruta: '{0}' \nPoruka: '{1}'", RoutingKey, log.RenderedMessage);
                }
            }
        }
    }
}