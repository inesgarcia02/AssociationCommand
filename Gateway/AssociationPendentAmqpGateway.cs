namespace Gateway;
using System.Text;
using RabbitMQ.Client;
public class AssociationPendentAmqpGateway
{
    private readonly IConnectionFactory _factory;
    private readonly IConnection _connection;
    private readonly IModel _channel;
    public AssociationPendentAmqpGateway(IConnectionFactory factory)
    {
        _factory = factory;
        _connection = _factory.CreateConnection();
        _channel = _connection.CreateModel();
        _channel.ExchangeDeclare(exchange: "associationPendent", type: ExchangeType.Fanout);
    }

    public void Publish(string association)
    {
        var body = Encoding.UTF8.GetBytes(association);
        _channel.BasicPublish(exchange: "associationPendent",
                              routingKey: string.Empty,
                              basicProperties: null,
                              body: body);
    }


}
