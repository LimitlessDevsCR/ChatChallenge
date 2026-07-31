using Chat.Application.Interfaces;
using Chat.Infrastructure.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Chat.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddRabbitMqMessaging(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.Configure<RabbitMqOptions>(
                configuration.GetSection(RabbitMqOptions.SectionName));

            services.AddSingleton<RabbitMqConnectionFactory>();
            services.AddScoped<IStockQuoteRequestPublisher, RabbitMqStockQuoteRequestPublisher>();
            services.AddSingleton<IStockQuoteRequestConsumer, RabbitMqStockQuoteRequestConsumer>();

            return services;
        }
    }
}
