using System;
using System.Collections.Generic;
using System.Configuration;
using System.Reflection;
using System.Text;
using log4net.Appender;
using log4net.Core;
using Log4NetAppender.ExceptionStructure;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace Log4NetAppender.Appender
{
    public class RabbitMqAppender : AppenderSkeleton
    {
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
            DepthOfLog = 0;
        }

        private ConnectionFactory _connectionFactory;
        private WorkerThread<LoggingEvent> _worker;
        private static readonly FieldInfo LoggingEventDataFieldInfo =
            typeof(LoggingEvent).GetField("m_data", BindingFlags.Instance | BindingFlags.NonPublic);
        private string _routingKey;

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
        public int DepthOfLog { get; set; }

        protected override void OnClose()
        {
            _worker.Dispose();
            _worker = null;
        }

        protected override void Append(LoggingEvent loggingEvent)
        {
            var currentException = (Exception)loggingEvent.MessageObject;
            var exceptionGuid = Guid.NewGuid();

            var exceptionWithInner = new List<TransformException>
            {
                new TransformException(currentException, exceptionGuid, 0)
            };

            for (var i=0; i<DepthOfLog; ++i)
            {
                if (currentException.InnerException == null)
                    break;

                exceptionWithInner.Add(new TransformException(currentException.InnerException, exceptionGuid, i+1));
                currentException = currentException.InnerException;
            }

            for (var i=0; i<exceptionWithInner.Count-1; ++i)
                exceptionWithInner[i].InnerException = exceptionWithInner[i+1];

            var queueException = new QueueException(Tennent, Environment, AppName, loggingEvent.Level.DisplayName, exceptionWithInner[0]);

            var loggingEventData = (LoggingEventData)LoggingEventDataFieldInfo.GetValue(loggingEvent);
            loggingEventData.Message = JsonConvert.SerializeObject(queueException);
            LoggingEventDataFieldInfo.SetValue(loggingEvent, loggingEventData);

            loggingEvent.Fix = FixFlags.All;
            _worker.Enqueue(loggingEvent);
        }

        public override void ActivateOptions()
        {

            _routingKey = string.Join(".", Tennent.Replace(".", "%2E"), Environment.Replace(".", "%2E"), AppName.Replace(".", "%2E"));
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
                    var completeRoutingKey = _routingKey + "." + log.Level.DisplayName;
                    var body = Encoding.UTF8.GetBytes(log.RenderedMessage);
                    channel.BasicPublish("HattrickExchange", completeRoutingKey, null, body);
                    Console.WriteLine(" Ruta: '{0}' \nPoruka: '{1}'", completeRoutingKey, log.RenderedMessage);
                }
            }
        }
    }
}