using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;

namespace RPC_Server
{
    class Program
    {
        public static void Main()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "rpc",
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);
                channel.BasicQos(prefetchSize:0,
                                 prefetchCount:1,
                                 global:false);
                var consumer = new EventingBasicConsumer(channel);
                channel.BasicConsume(queue: "rpc",
                                     autoAck:false,
                                     consumer:consumer);
                Console.WriteLine("Please Waiting For a RPC request....");

                consumer.Received += (model, ea) =>
                 {
                     string response = null;
                     var body = ea.Body;
                     var prop = ea.BasicProperties;
                     var replyprop = channel.CreateBasicProperties();
                     replyprop.CorrelationId = prop.CorrelationId;

                     try
                     {
                         var message = Encoding.UTF8.GetString(body);
                         int n = int.Parse(message);
                         Console.WriteLine("[.] fibonacci({0})", message);
                         response = fibonacci(n).ToString();
                     }
                     catch (Exception e)
                     {
                         Console.WriteLine("", e.Message);
                         response = "";
                     }
                     finally
                     {
                         var resBytes = Encoding.UTF8.GetBytes(response);
                         channel.BasicPublish(exchange: "",
                                              routingKey: prop.ReplyTo,
                                              basicProperties: replyprop,
                                              body: resBytes);
                         channel.BasicAck(deliveryTag: ea.DeliveryTag,
                                          multiple: false);
                             
                     }

                 };
                Console.WriteLine("Press Enter if u sure to Exit");
                Console.ReadLine();
             }
            
        }

        private static int fibonacci(int n)
        {
            if(n==0 || n==1)
            {
                return n;
            }
            return fibonacci(n - 1) + fibonacci(n - 2);
            
        }
    }
}
