using System;
using RabbitMQ.Client;
using System.Text;

namespace NewTask
{
    class NewTask
    {
        public static void Main(string[] args)
        {
            new NewTask().run(args);
        }

        public void run(string[] args)
        {
            var sender = new Sender();
            sender.send(GetMessage(args));
        }

        private string GetMessage(string[] args)
        {
            return ((args.Length > 0) ? string.Join(" ", args) : "Hello World!");
        }
    }

    class Sender
    {
        public void send(string message) {
            WithChannel((channel) =>
            {
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
                queue: "task_queue",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );
        }

        private void SendMessage(IModel channel, string message) {
            var body = Encoding.UTF8.GetBytes(message);
            channel.BasicPublish(
                exchange: "",
                routingKey: "task_queue",
                basicProperties: Properties(channel),
                body: body
            );
        }

        private IBasicProperties Properties(IModel channel)
        {
            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;
            return properties;
        }
    }
}
