using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Threading;

namespace Worker
{
    class Worker
    {
        public static void Main()
        {
            new Receiver().listen(message => new Worker().run(message));
        }

        public void run(string message)
        {
            Console.WriteLine(" [x] Received {0}", message);
            int dots = message.Split('.').Length - 1;
            Thread.Sleep(dots * 1000);

            Console.WriteLine(" [x] Done");
        }

    }

    class Receiver
    {
        public void listen(Action<string> work)
        {
            WithChannel((channel) =>
            {
                DeclareQueue(channel);
                channel.BasicQos(prefetchSize: 0, prefetchCount: 1,
                                 global: false);
                Console.WriteLine(" [*] Waiting for messages.");
                ConsumeChannel(channel, (model, eventArgs) =>
                {
                    work(MessageFromArgs(eventArgs));
                    channel.BasicAck(deliveryTag: eventArgs.DeliveryTag,
                                     multiple: false);
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
                queue: "task_queue",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );
        }

        private void ConsumeChannel(IModel channel,
                                    EventHandler<BasicDeliverEventArgs> handler)
        {
            channel.BasicConsume(
                queue: "task_queue",
                autoAck: false,
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

        private string MessageFromArgs(BasicDeliverEventArgs eventArgs)
        {
            var body = eventArgs.Body.ToArray();
            return Encoding.UTF8.GetString(body);
        }
    }
}
