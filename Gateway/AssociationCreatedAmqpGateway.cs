namespace Gateway;
using System.Text;
using RabbitMQ.Client;
public class AssociationCreatedAmqpGateway
{
    private readonly ConnectionFactory _factory;
    private readonly IConnection _connection;
    private readonly IModel _channel;
    public AssociationCreatedAmqpGateway()
    {
        _factory = new ConnectionFactory { HostName = "localhost" };
        _connection = _factory.CreateConnection();
        _channel = _connection.CreateModel();
        _channel.ExchangeDeclare(exchange: "associationCreated", type: ExchangeType.Fanout);
    }

    public void Publish(string association)
    {
        var body = Encoding.UTF8.GetBytes(association);
        _channel.BasicPublish(exchange: "associationCreated",
                              routingKey: string.Empty,
                              basicProperties: null,
                              body: body);
    }


}
