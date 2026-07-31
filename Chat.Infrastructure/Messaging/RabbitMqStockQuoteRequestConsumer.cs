using System.Text;
using System.Text.Json;
using Chat.Application.DTOs;
using Chat.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Chat.Infrastructure.Messaging
{
    internal sealed class RabbitMqStockQuoteRequestConsumer : IStockQuoteRequestConsumer
    {
        private readonly RabbitMqConnectionFactory _connectionFactory;
        private readonly ILogger<RabbitMqStockQuoteRequestConsumer> _logger;
        private readonly RabbitMqOptions _options;

        public RabbitMqStockQuoteRequestConsumer(
            RabbitMqConnectionFactory connectionFactory,
            IOptions<RabbitMqOptions> options,
            ILogger<RabbitMqStockQuoteRequestConsumer> logger)
        {
            _connectionFactory = connectionFactory;
            _options = options.Value;
            _logger = logger;
        }

        public async Task StartConsumingAsync(
            Func<StockQuoteRequestDto, CancellationToken, Task> handleRequestAsync,
            CancellationToken cancellationToken = default)
        {
            await using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
            await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

            await channel.QueueDeclareAsync(
                queue: _options.StockRequestQueue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null,
                cancellationToken: cancellationToken);

            await channel.BasicQosAsync(
                prefetchSize: 0,
                prefetchCount: 1,
                global: false,
                cancellationToken: cancellationToken);

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (_, eventArgs) =>
            {
                try
                {
                    var json = Encoding.UTF8.GetString(eventArgs.Body.Span);
                    var request = JsonSerializer.Deserialize<StockQuoteRequestDto>(json);

                    if (request is null)
                    {
                        _logger.LogWarning("Received an empty stock quote request message.");
                        await channel.BasicAckAsync(eventArgs.DeliveryTag, multiple: false, cancellationToken);
                        return;
                    }

                    await handleRequestAsync(request, cancellationToken);
                    await channel.BasicAckAsync(eventArgs.DeliveryTag, multiple: false, cancellationToken);
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, "Failed to process stock quote request message.");
                    await channel.BasicNackAsync(
                        eventArgs.DeliveryTag,
                        multiple: false,
                        requeue: false,
                        cancellationToken);
                }
            };

            await channel.BasicConsumeAsync(
                queue: _options.StockRequestQueue,
                autoAck: false,
                consumer: consumer,
                cancellationToken: cancellationToken);

            await Task.Delay(Timeout.Infinite, cancellationToken);
        }
    }
}
