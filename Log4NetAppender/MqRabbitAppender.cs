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
        private XmlMessageBuilder _messageBuilder;
        private WorkerThread<LoggingEvent> _worker;
        public MqRabbitAppender()
            {
                HostName = "localhost";
                VirtualHost = "/";
                UserName = "guest";
                Password = "guest";
                RequestedHeartbeat = (ushort)0;
                Port = 5672;
                Exchange = "logs";
                RoutingKey = "";
                FlushInterval = 5;
            }

        public string HostName { get; set; }
        public string VirtualHost { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public ushort RequestedHeartbeat { get; set; }
        public int Port { get; set; }
        public string Exchange { get; set; }
        public string RoutingKey { get; set; }
        public int FlushInterval { get; set; }

        protected override void OnClose()
        {
            this._worker.Dispose();
            this._worker = (WorkerThread<LoggingEvent>)null;
        }

        protected override void Append(LoggingEvent loggingEvent)
        {
            loggingEvent.Fix = FixFlags.All;
            this._worker.Enqueue(loggingEvent);
        }

        public override void ActivateOptions()
        {
            this._messageBuilder = new XmlMessageBuilder();
            this._messageBuilder.ActivateOptions();
            this._connectionFactory = new ConnectionFactory()
            {
                HostName = this.HostName,
                VirtualHost = this.VirtualHost,
                UserName = this.UserName,
                Password = this.Password,
                RequestedHeartbeat = this.RequestedHeartbeat,
                Port = this.Port
            };
            this._worker = new WorkerThread<LoggingEvent>("Worker for log4net appender '" + this.Name + "'", TimeSpan.FromSeconds((double)this.FlushInterval), new Action<LoggingEvent[]>(this.Process));
        }

        public void Process(LoggingEvent[] logs)
        {
            var factory = new ConnectionFactory() {HostName = "localhost"};
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: Exchange, type: "topic");
                var message = logs[0].RenderedMessage;
                var body = Encoding.UTF8.GetBytes(message);
                channel.BasicPublish(exchange: Exchange,
                    routingKey: RoutingKey,
                    basicProperties: null,
                    body: body);
                Console.WriteLine(" [x] Sent '{0}':'{1}'", RoutingKey, message);
            }
        }
    }
}