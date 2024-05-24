using System.Text;
using RabbitMQ.Client;

namespace Gateway
{
    public class HolidayVerificationAmqpGateway
    {
        private readonly IConnectionFactory _factory;
        private readonly IConnection _connection;
        private readonly IModel _channel;
        public HolidayVerificationAmqpGateway(IConnectionFactory factory)
        {
            _factory = factory;
            _connection = _factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.ExchangeDeclare(exchange: "holidayPendentResponse", type: ExchangeType.Fanout);
        }

        public void Publish(string association)
        {
            var body = Encoding.UTF8.GetBytes(association);
            _channel.BasicPublish(exchange: "holidayPendentResponse",
                                  routingKey: string.Empty,
                                  basicProperties: null,
                                  body: body);
        }
    }
}