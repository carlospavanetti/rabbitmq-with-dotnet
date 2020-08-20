using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace ReceiveLogs
{
    class ReceiveLogs
    {
        static void Main(string[] args)
        {
            new ReceiveLogs().run();
        }

        private void run()
        {
            WithQueue(body =>
            {
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine(" [x] {0}", message);
            });
        }

        private void WithQueue(Action<byte[]> receive)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using(var connection = factory.CreateConnection())
            using(var channel = connection.CreateModel())
            {
                DeclareExchange(channel, name: "logs");
                var queue = DeclareQueue(channel, exchange: "logs");

                Console.WriteLine(" [*] Waiting for logs.");
                ConsumeChannel(channel, queue, receive);

                Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();
            }
        }

        private void DeclareExchange(IModel channel, string name)
        {
            var type = ExchangeType.Fanout;
            channel.ExchangeDeclare(exchange: name, type: type);
        }

        private string DeclareQueue(IModel channel, string exchange)
        {
            var name = channel.QueueDeclare().QueueName;
            channel.QueueBind(queue: name, exchange: exchange, routingKey: "");
            return name;
        }

        private void ConsumeChannel(IModel channel, string queue,
                                    Action<byte[]> receive)
        {
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) => receive(ea.Body.ToArray());
            channel.BasicConsume(queue: queue, autoAck: true,
                                 consumer: consumer);
        }
    }
}
