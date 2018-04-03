﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using log4net;
using Log4NetAppender.Appender;
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
        private IModel _channel;

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
            _channel.ExchangeDeclare("HattrickExchange", "topic");
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
            //neće ovako, moraju se otvarat novi channeli, po jedan za svakog consumera
            for (var i = 0; i < numberOfThreads; ++i)
            {
                _channel = _connection.CreateModel();
                new Thread(() =>
                {
                    Thread.CurrentThread.Name = i.ToString();
                    Console.WriteLine($"Postavili smo ime na: {Thread.CurrentThread.Name}");
                    Thread.CurrentThread.IsBackground = true;
                    var consumer = new EventingBasicConsumer(_channel);
                    consumer.Received += (model, ea) =>
                    {
                        var body = ea.Body;
                        var message = Encoding.UTF8.GetString(body);
                        var msgRoutingKey = ea.RoutingKey;
                        Console.WriteLine($"NIT BROJ: {Thread.CurrentThread.Name}\nPRIMLJENO: '{msgRoutingKey}' \nPORUKA: '{message}'");
                        DeserializeAndConsume(message);
                        _channel.BasicAck(ea.DeliveryTag, false);
                    };
                    _channel.BasicConsume(queueToConnectToName, false, consumer);
                }).Start();
            }
        }

        public void DisconnectFromQueue(string consumerToDisconnectTag)
        {
            _channel.BasicCancel(consumerToDisconnectTag);
        }

        private static void DeserializeAndConsume(string messageToConsume)
        {
            return;
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
