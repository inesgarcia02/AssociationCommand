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


            _channel.ExchangeDeclare(exchange: "associationPendentResponse", type: ExchangeType.Fanout);

            Console.WriteLine(" [*] Waiting for messages from Association.");
        }

        public void ConfigQueue(string queueName)
        {
            _queueName = "pending" + queueName;

            _channel.QueueDeclare(queue: _queueName,
                                            durable: true,
                                            exclusive: false,
                                            autoDelete: false,
                                            arguments: null);

            _channel.QueueBind(queue: _queueName,
                  exchange: "associationPendentResponse",
                  routingKey: string.Empty);
        }

        public void StartConsuming()
        {
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                if (message.StartsWith("Not Ok"))
                {
                    Console.WriteLine("Received 'Not Ok' message. No action required.");
                }
                else if (message.StartsWith("Ok"))
                {
                    // Remove o prefixo "Ok" da mensagem
                    var jsonMessage = message.Substring(2);

                    // Desserializar o JSON restante
                    AssociationDTO associationDTO = AssociationAmqpDTO.Deserialize(jsonMessage);

                    // Processa o DTO
                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        var associationService = scope.ServiceProvider.GetRequiredService<AssociationService>();
                        await associationService.Add(associationDTO, _errorMessages);
                    }

                    Console.WriteLine($"Received 'Ok' message and processed it: {jsonMessage}");
                }
            };

            _channel.BasicConsume(queue: _queueName,
                                  autoAck: true,
                                  consumer: consumer);
        }

    }
}