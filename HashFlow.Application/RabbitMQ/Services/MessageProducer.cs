using System.Text;
using System.Text.Json;
using HashFlow.Application.Common.Interfaces;
using RabbitMQ.Client;

namespace HashFlow.Application.RabbitMQ.Services;

public class MessageProducer : IMessageProducer, IDisposable
{
    private readonly IModel _channel;

    public MessageProducer()
    {
        var factory = new ConnectionFactory()
        {
            Uri = new Uri("amqp://hashesAdmin:hashesPassword@rabbitmq:5672/"),
            ClientProvidedName = "Rabbit Sender App"
        };

        var connection = factory.CreateConnection();
        _channel = connection.CreateModel();

        _channel.QueueDeclare("hashes",
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);
    }
    
    public void SendBatchToQueueAsync(IEnumerable<string> batch)
    {
        var batchJson = JsonSerializer.Serialize(batch);
        var body = Encoding.UTF8.GetBytes(batchJson);

        _channel.BasicPublish(exchange: "", routingKey: "hashes", basicProperties: null, body: body);
    }

    public void Dispose()
    {
        _channel.Dispose();
    }
}