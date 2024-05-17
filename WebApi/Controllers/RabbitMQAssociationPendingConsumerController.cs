using Application.DTO;
using Application.Services;
using Gateway;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
namespace WebApi.Controllers
{
    public class RabbitMQAssociationPendingConsumerController : IRabbitMQConsumerController
    {
        private List<string> _errorMessages = new List<string>();
        private AssociationCreatedAmqpGateway _associationCreatedAmqpGateway;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ConnectionFactory _factory;
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private string _queueName;

        public RabbitMQAssociationPendingConsumerController(IServiceScopeFactory serviceScopeFactory, AssociationCreatedAmqpGateway associationCreatedAmqpGateway)
        {
            _associationCreatedAmqpGateway = associationCreatedAmqpGateway;
            _serviceScopeFactory = serviceScopeFactory;
            _factory = new ConnectionFactory { HostName = "localhost" };
            _connection = _factory.CreateConnection();
            _channel = _connection.CreateModel();


            _channel.ExchangeDeclare(exchange: "associationPendentResponse", type: ExchangeType.Fanout);

            Console.WriteLine(" [*] Waiting for messages from AssociationPendet.");
        }

        public void ConfigQueue(string queueName)
        {
            _queueName = "assocPending" + queueName;

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

                AssociationAmqpDTO associationAmqpDTO = AssociationAmqpDTO.Deserialize(message);

                if (associationAmqpDTO.Status == "Not Ok")
                {
                    Console.WriteLine("Received 'Not Ok' message. No action required.");
                }
                else if (associationAmqpDTO.Status == "Ok")
                {
                    AssociationDTO associationDTO = AssociationAmqpDTO.ToDTO(associationAmqpDTO);

                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        var associationService = scope.ServiceProvider.GetRequiredService<AssociationService>();
                        List<string> errorMessages = new List<string>();
                        await associationService.Add(associationDTO, errorMessages);

                        if (errorMessages.Any())
                        {
                            Console.WriteLine($"Errors occurred while processing the message: {string.Join(", ", errorMessages)}");
                        }
                        else
                        {
                            string message1 = AssociationAmqpDTO.Serialize(associationDTO);   
                             _associationCreatedAmqpGateway.Publish(message1);
                            Console.WriteLine($"Processed message successfully: {associationDTO}");
                        }
                    }
                }
            };

            _channel.BasicConsume(queue: _queueName,
                                  autoAck: true,
                                  consumer: consumer);
        }
    }
}