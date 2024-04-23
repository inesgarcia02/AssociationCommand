using Application.DTO;
using Application.Services;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
namespace WebApi.Controllers
{
    public class RabbitMQAssociationPendingConsumerController : IRabbitMQAssociationConsumerController
    {
        private List<string> _errorMessages = new List<string>();
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ConnectionFactory _factory;
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private string _queueName;

        public RabbitMQAssociationPendingConsumerController(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _factory = new ConnectionFactory { HostName = "localhost" };
            _connection = _factory.CreateConnection();
            _channel = _connection.CreateModel();


            _channel.ExchangeDeclare(exchange: "associationPendent", type: ExchangeType.Fanout);

            Console.WriteLine(" [*] Waiting for messages from Association.");
        }

        public void ConfigQueue(string queueName)
        {
            _queueName = queueName + "pending";

            _channel.QueueDeclare(queue: _queueName,
                                            durable: true,
                                            exclusive: false,
                                            autoDelete: false,
                                            arguments: null);

            _channel.QueueBind(queue: _queueName,
                  exchange: "associationPendent",
                  routingKey: string.Empty);
        }

        public void StartConsuming()
        {
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                AssociationDTO associationDTO = AssociationAmqpDTO.Deserialize(message);
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var associationService = scope.ServiceProvider.GetRequiredService<AssociationService>();

                    if (message.StartsWith("Ok"))
                    {
                        await associationService.Add(associationDTO, _errorMessages);
                    }
                    else {
                        Console.WriteLine($"Association not approved.");
                    }
                    
                }

                Console.WriteLine($" [x] Received {message}");
            };
            _channel.BasicConsume(queue: _queueName,
                                autoAck: true,
                                consumer: consumer);
        }
    }
}