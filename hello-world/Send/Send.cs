﻿using System;
using RabbitMQ.Client;
using System.Text;

namespace Send
{
    class Send
    {
        static void Main()
        {
          new Send().run();
        }

        public void run()
        {
            WithChannel((channel) =>
            {
                string message = "Hello World!";
                DeclareQueue(channel);
                SendMessage(channel, message);
                Console.WriteLine(" [x] Sent {0}", message);
            });
            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
        }

        private void WithChannel(Action<IModel> method)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                method(channel);
            }
        }

        private void DeclareQueue(IModel channel) {
            channel.QueueDeclare(
                queue: "hello",
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );
        }

        private void SendMessage(IModel channel, string message) {
            var body = Encoding.UTF8.GetBytes(message);
            channel.BasicPublish(
                exchange: "",
                routingKey: "hello",
                basicProperties: null,
                body: body
            );
        }
    }
}
