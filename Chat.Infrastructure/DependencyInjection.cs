using Chat.Application.Interfaces;
using Chat.Infrastructure.Messaging;
using Chat.Infrastructure.StockApi;
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
            services.AddScoped<IStockQuoteResponsePublisher, RabbitMqStockQuoteResponsePublisher>();
            services.AddSingleton<IStockQuoteRequestConsumer, RabbitMqStockQuoteRequestConsumer>();
            services.AddSingleton<IStockQuoteResponseConsumer, RabbitMqStockQuoteResponseConsumer>();

            services.AddHttpClient<IStockQuoteService, StooqStockQuoteService>(client =>
            {
                client.BaseAddress = new Uri("https://stooq.com");
            });

            return services;
        }
    }
}
