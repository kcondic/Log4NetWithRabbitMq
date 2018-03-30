using System;
using System.Text;
using log4net.Appender;
using log4net.Core;
using RabbitMQ.Client;

namespace Log4NetAppender.Appender
{
    public class RabbitMqAppender : AppenderSkeleton
    {
        private ConnectionFactory _connectionFactory;
        private WorkerThread<LoggingEvent> _worker;
        public RabbitMqAppender()
        {
            HostName = "localhost";
            VirtualHost = "/";
            UserName = "guest";
            Password = "guest";
            RequestedHeartbeat = 0;
            Port = 5672;
            FlushInterval = 5;
            Tennent = "";
            Environment = "";
            AppName = "";
        }

        public string HostName { get; set; }
        public string VirtualHost { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public ushort RequestedHeartbeat { get; set; }
        public int Port { get; set; }
        public int FlushInterval { get; set; }
        public string Tennent { get; set; }
        public string Environment { get; set; }
        public string AppName { get; set; }
        public string RoutingKey { get; set; }

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

            RoutingKey = string.Join(".", Tennent.Replace(".", "%2E"), Environment.Replace(".", "%2E"), AppName.Replace(".", "%2E"));
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
            using (var connection = _connectionFactory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare("HattrickExchange", "topic");
                foreach (var log in logs)
                {
                    var completeRoutingKey = RoutingKey + "." + log.Level.DisplayName;
                    var body = Encoding.UTF8.GetBytes(log.RenderedMessage);
                    channel.BasicPublish("HattrickExchange", completeRoutingKey, null, body);
                    Console.WriteLine(" Ruta: '{0}' \nPoruka: '{1}'", completeRoutingKey, log.RenderedMessage);
                }
            }
        }
    }
}