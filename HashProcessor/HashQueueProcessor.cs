using System.Text;
using HashFlow.Application.Common.Interfaces;
using HashFlow.Domain.DTOs;
using HashFlow.Domain.Models;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace HashProcessor;

public class HashQueueProcessor : BackgroundService
{
    private readonly ILogger<HashQueueProcessor> _logger;
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly SemaphoreSlim _processingSemaphore;
    public HashQueueProcessor(ILogger<HashQueueProcessor> logger, IServiceScopeFactory serviceScopeFactory, MessageBrokerSettingsDto messageBrokerSettings)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
        _processingSemaphore = new SemaphoreSlim(4);

        var factory = new ConnectionFactory()
        { 
            HostName = messageBrokerSettings.Host,
            Port = messageBrokerSettings.Port,
            Password = messageBrokerSettings.Password,
            UserName = messageBrokerSettings.Username,
            ClientProvidedName = "Rabbit Processor App"
        };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _channel.QueueDeclare("hashes",
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine("Hash Processor running");

        var consumer = new EventingBasicConsumer(_channel);

        consumer.Received += async (_, eventArgs) =>
        {
            await _processingSemaphore.WaitAsync(stoppingToken);
            await ProcessMessageAsync(eventArgs)
                .ContinueWith(_ => _processingSemaphore.Release(), stoppingToken);
        };

        _channel.BasicConsume("hashes", true, consumer);

        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Hash queue processor running at: {time}", DateTimeOffset.Now);
            await Task.Delay(5000, stoppingToken);
        }
    }

    private async Task ProcessMessageAsync(BasicDeliverEventArgs eventArgs)
    {
        try
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var hashesRepository = scope.ServiceProvider.GetRequiredService<IHashesRepository>();

            var body = eventArgs.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var hashStrings = JsonConvert.DeserializeObject<List<string>>(message);

            var hashObjects = hashStrings?
                .Select(hashString => new Hash
                {
                    Id = Guid.NewGuid(),
                    Date = DateTime.UtcNow,
                    Sha1 = hashString
                })
                .ToList();

            if (hashObjects is not null && hashObjects.Any())
            {
                await hashesRepository.AddHashRange(hashObjects);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message");
        }
    }

    public override void Dispose()
    {
        _channel.Close();
        _connection.Close();
        base.Dispose();
    }
}