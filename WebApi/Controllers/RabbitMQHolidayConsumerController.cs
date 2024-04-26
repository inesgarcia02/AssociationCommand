using System.Text;
using Application.DTO;
using Application.Services;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace WebApi.Controllers
{
    public class RabbitMQHolidayConsumerController : IRabbitMQConsumerController
    {
        private List<string> _errorMessages = new List<string>();
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ConnectionFactory _factory;
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private string _queueName;

        public RabbitMQHolidayConsumerController(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _factory = new ConnectionFactory { HostName = "localhost" };
            _connection = _factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare(exchange: "holidayToValidate", type: ExchangeType.Fanout);

            Console.WriteLine(" [*] Waiting for messages from Holiday.");
        }
        public void ConfigQueue(string queueName)
        {
            _queueName = "holiday" + queueName;

            _channel.QueueDeclare(queue: _queueName,
                                            durable: true,
                                            exclusive: false,
                                            autoDelete: false,
                                            arguments: null);

            _channel.QueueBind(queue: _queueName,
                  exchange: "holidayToValidate",
                  routingKey: string.Empty);
        }

        public void StartConsuming()
        {
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                HolidayAmqpDTO holidayDTO = HolidayAmqpDTO.Deserialize(message);
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var holidayService = scope.ServiceProvider.GetRequiredService<HolidayService>();
                    await holidayService.Validations(holidayDTO);
                }

                Console.WriteLine($" [x] Received {message}");
            };
            _channel.BasicConsume(queue: _queueName,
                                autoAck: true,
                                consumer: consumer);
        }
    }
}