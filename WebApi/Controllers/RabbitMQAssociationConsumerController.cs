using Application.DTO;
using Application.Services;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace WebApi.Controllers
{
    public class RabbitMQAssociationConsumerController : IRabbitMQConsumerController
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ConnectionFactory _factory;
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly string _queueName;

        public RabbitMQAssociationConsumerController(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _factory = new ConnectionFactory { HostName = "localhost" };
            _connection = _factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.ExchangeDeclare(exchange: "associationCreated", type: ExchangeType.Fanout);
            Console.WriteLine(" [*] Waiting for messages from Association.");
            _queueName = "association";
            ConfigQueue(_queueName);
        }

        public void ConfigQueue(string queueName)
        {
            _channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
            _channel.QueueBind(queue: queueName, exchange: "associationCreated", routingKey: string.Empty);
        }

        public void StartConsuming()
        {
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                AssociationAmqpDTO associationAmqpDTO = AssociationAmqpDTO.Deserialize(message);
                AssociationDTO associationDTO = AssociationAmqpDTO.ToDTO(associationAmqpDTO);

                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var associationService = scope.ServiceProvider.GetRequiredService<AssociationService>();

                    // Processa a associação
                    await associationService.Add(associationDTO, new List<string>());
                     Console.WriteLine($"Processed message successfully: {associationDTO}");
                }

                // Confirma a mensagem manualmente após o processamento bem-sucedido
                _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            };

            _channel.BasicConsume(queue: _queueName, autoAck: false, consumer: consumer);
        }
    }
}
