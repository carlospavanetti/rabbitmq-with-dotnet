using System;
using RabbitMQ.Client;
using System.Text;

namespace EmitLog
{
    class EmitLog
    {
        static void Main(string[] args)
        {
            new EmitLog().run(GetMessage(args));
        }

        private static string GetMessage(string[] args)
        {
            return ((args.Length > 0)
                ? string.Join(" ", args)
                : "info: Hello World!");
        }

        public void run(string message)
        {
            WithQueue(publish =>
            {
                var body = Encoding.UTF8.GetBytes(message);
                publish(body);
                Console.WriteLine(" [x] Sent {0}", message);
            });

            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
        }

        private void WithQueue(Action<Action<byte[]>> task)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using(var connection = factory.CreateConnection())
            using(var channel = connection.CreateModel())
            {
                DeclareExchange(channel, name: "logs");
                task(Publish(channel, exchange: "logs"));
            }
        }

        private void DeclareExchange(IModel channel, string name)
        {
            var type = ExchangeType.Fanout;
            channel.ExchangeDeclare(exchange: name, type: type);
        }

        private Action<byte[]> Publish(IModel channel, string exchange)
        {
            return body => channel.BasicPublish(
                exchange: exchange, routingKey: "",
                basicProperties: null, body: body
            );
        }
    }
}
