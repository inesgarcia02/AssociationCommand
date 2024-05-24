using Application.DTO;
using Application.Services;
using Gateway;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace WebApi.Controllers
{
    public class RabbitMQAssociationPendingConsumerController : IRabbitMQConsumerController
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IConnectionFactory _factory;
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly string _queueName;

        public RabbitMQAssociationPendingConsumerController(IServiceScopeFactory serviceScopeFactory, IConnectionFactory factory)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _factory = factory;
            _connection = _factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare(exchange: "associationPendentResponse", type: ExchangeType.Fanout);
            Console.WriteLine(" [*] Waiting for messages from AssociationPendet.");

            _queueName = "assocPending"; // Nome da fila
            ConfigQueue(_queueName);
        }

        public void ConfigQueue(string queueName)
        {
            _channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
            _channel.QueueBind(queue: queueName, exchange: "associationPendentResponse", routingKey: string.Empty);
        }

        public void StartConsuming()
        {
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                AssociationAmqpDTO associationAmqpDTO = AssociationAmqpDTO.Deserialize(message);

                if (associationAmqpDTO.Status == "Ok")
                {
                    AssociationDTO associationDTO = AssociationAmqpDTO.ToDTO(associationAmqpDTO);

                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        var associationService = scope.ServiceProvider.GetRequiredService<AssociationService>();

                        // Processa a associação
                        await associationService.Update(associationDTO, new List<string>());
                    }
                    // Confirma a mensagem manualmente após o processamento bem-sucedido
                    _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                }
                else if (associationAmqpDTO.Status == "Not Ok")
                {
                    Console.WriteLine("Received 'Not Ok' message. No action required.");
                }
            };

            _channel.BasicConsume(queue: _queueName, autoAck: false, consumer: consumer);
        }
    }
}
