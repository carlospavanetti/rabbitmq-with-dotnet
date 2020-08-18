using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;

namespace Receive
{
    class Receive
    {
        static void Main()
        {
            new Receive().run();
        }

        public void run()
        {
            WithChannel((channel) =>
            {
                DeclareQueue(channel);
                ConsumeChannel(channel, (model, eventArgs) =>
                {
                    var body = eventArgs.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    Console.WriteLine(" [x] Received {0}", message);
                });
                Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();
            });
        }

        private void WithChannel(Action<IModel> action)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                action(channel);
            }
        }

        private void DeclareQueue(IModel channel)
        {
            channel.QueueDeclare(
                queue: "hello",
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );
        }

        private void ConsumeChannel(IModel channel,
                                    EventHandler<BasicDeliverEventArgs> handler)
        {
            channel.BasicConsume(
                queue: "hello",
                autoAck: true,
                consumer: NewConsumer(channel, handler)
            );
        }

        private DefaultBasicConsumer NewConsumer(
            IModel channel,
            EventHandler<BasicDeliverEventArgs> handler
        )
        {
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += handler;
            return consumer;
        }
    }
}
