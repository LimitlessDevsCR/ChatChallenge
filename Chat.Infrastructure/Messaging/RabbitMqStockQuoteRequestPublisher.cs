using System.Text;
using System.Text.Json;
using Chat.Application.DTOs;
using Chat.Application.Interfaces;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace Chat.Infrastructure.Messaging
{
    internal sealed class RabbitMqStockQuoteRequestPublisher : IStockQuoteRequestPublisher
    {
        private readonly RabbitMqConnectionFactory _connectionFactory;
        private readonly RabbitMqOptions _options;

        public RabbitMqStockQuoteRequestPublisher(
            RabbitMqConnectionFactory connectionFactory,
            IOptions<RabbitMqOptions> options)
        {
            _connectionFactory = connectionFactory;
            _options = options.Value;
        }

        public async Task PublishAsync(
            StockQuoteRequestDto request,
            CancellationToken cancellationToken = default)
        {
            try
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

                var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(request));

                await channel.BasicPublishAsync(
                    exchange: string.Empty,
                    routingKey: _options.StockRequestQueue,
                    mandatory: false,
                    body: body,
                    cancellationToken: cancellationToken);
            }
            catch (BrokerUnreachableException exception)
            {
                throw new InvalidOperationException("Stock quote service is not available. RabbitMQ is not reachable.", exception);
            }
        }
    }
}
