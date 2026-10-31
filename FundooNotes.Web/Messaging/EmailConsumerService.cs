using BusinessLayer.Interface;
using ModelLayer.Models.Email;
using NLog.Config;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace FundooNotes.Web.Messaging
{
    public class EmailConsumerService : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        private readonly ILogger<EmailConsumerService> _logger;
        public EmailConsumerService(IConfiguration configuration, IEmailService emailService, ILogger<EmailConsumerService> logger)
        {
            _configuration = configuration;
            _emailService = emailService;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            var connectionFactory = new ConnectionFactory()
            {
                HostName = _configuration["RabbitMQ:HostName"]!,
                Port = int.Parse(_configuration["RabbitMQ:Port"] ?? "5672"),
                UserName = _configuration["RabbitMQ:UserName"]!,
                Password = _configuration["RabbitMQ:Password"]!
            };

            await using var connection = await connectionFactory.CreateConnectionAsync();   
            await using var channel = await connection.CreateChannelAsync();

            var queueName = _configuration["RabbitMQ:QueueName"] ?? "fundoo-email-queue";
            await channel.QueueDeclareAsync(queue: queueName, durable: true, exclusive: false, autoDelete: false);

            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.ReceivedAsync += async (_, ea) =>
            {
                try
                {
                    var message = Encoding.UTF8.GetString(ea.Body.Span);
                    var emailMessage = JsonSerializer.Deserialize<EmailMessage>(message);

                    if (emailMessage is null ||
                    string.IsNullOrWhiteSpace(emailMessage.To) ||
                    string.IsNullOrWhiteSpace(emailMessage.Subject))
                    {
                        throw new JsonException("Invalid email message");
                    }


                    await _emailService.SendEmailAsync(emailMessage.To, emailMessage.Subject, emailMessage.Body);

                    await channel.BasicAckAsync(ea.DeliveryTag, false);

                    _logger.LogInformation("Email sent successfully to {Email}", emailMessage.To);

                } catch(Exception e)
                {
                    _logger.LogError(e, "Failed to process email message");

                    await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false);
                }
            };

            await channel.BasicConsumeAsync(queueName, false, consumer, cancellationToken);
            _logger.LogInformation("RabbitMQ email consumer started");

            await Task.Delay(
            Timeout.Infinite,
            cancellationToken);
        }
    }
}
