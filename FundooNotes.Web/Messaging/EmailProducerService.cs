using RabbitMQ.Client;
using System.Text.Json;
using System.Text;
using ModelLayer.Models.Email;
using BusinessLayer.Interface;
namespace FundooNotes.Web.Messaging
{
    public class EmailProducerService : IEmailProducerService
    {
        private IConnection? _connection;
        private IChannel? _channel;
        private readonly IConfiguration _configuration;
        public EmailProducerService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<IChannel> GetOrCreateChannelAsync(ConnectionFactory connectionFactory)
        {
            if (_connection is null) _connection = await connectionFactory.CreateConnectionAsync();
            if (_channel is null) _channel = await _connection.CreateChannelAsync();

            return _channel;
        }

        public async Task EnqueueAsync(EmailMessage message)
        {
            var connectionFactory = new ConnectionFactory()
            {
                HostName = _configuration["RabbitMQ:HostName"]!,
                Port = int.Parse(_configuration["RabbitMQ:Port"] ?? "5672"),
                UserName = _configuration["RabbitMQ:UserName"]!,
                Password = _configuration["RabbitMQ:Password"]!
            };

            //await using var connection = await connectionFactory.CreateConnectionAsync();
            //await using var channel = await connection.CreateChannelAsync();

            var channel = await GetOrCreateChannelAsync(connectionFactory);

            var queueName = _configuration["RabbitMQ:QueueName"] ?? "fundoo-email-queue";
            await channel.QueueDeclareAsync(queue: queueName, durable: true, exclusive: false, autoDelete: false);

            var jsonBody = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(jsonBody);

            var properties = new BasicProperties()
            {
                Persistent = true,
                ContentType = "application/json"
            };

            
            await channel.BasicPublishAsync(
                exchange: string.Empty,
                routingKey: queueName,
                mandatory: true,
                basicProperties: properties,
                body: body
            );
        }
    }
}
