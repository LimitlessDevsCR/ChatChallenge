using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Chat.Infrastructure.Messaging
{
    internal sealed class RabbitMqConnectionFactory
    {
        private readonly RabbitMqOptions _options;

        public RabbitMqConnectionFactory(IOptions<RabbitMqOptions> options)
        {
            _options = options.Value;
        }

        public Task<IConnection> CreateConnectionAsync(CancellationToken cancellationToken = default)
        {
            var factory = new ConnectionFactory
            {
                HostName = _options.HostName,
                Port = _options.Port,
                UserName = _options.UserName,
                Password = _options.Password
            };

            return factory.CreateConnectionAsync(cancellationToken);
        }
    }
}
