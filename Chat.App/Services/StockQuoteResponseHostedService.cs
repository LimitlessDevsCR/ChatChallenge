using Chat.App.Hubs;
using Chat.Application.DTOs;
using Chat.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace Chat.App.Services
{
    public sealed class StockQuoteResponseHostedService : BackgroundService
    {
        private readonly ILogger<StockQuoteResponseHostedService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public StockQuoteResponseHostedService(
            IServiceScopeFactory scopeFactory,
            ILogger<StockQuoteResponseHostedService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Stock quote response listener is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var consumer = scope.ServiceProvider.GetRequiredService<IStockQuoteResponseConsumer>();

                    await consumer.StartConsumingAsync(
                        HandleStockQuoteResponseAsync,
                        stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation("Stock quote response listener is stopping.");
                }
                catch (Exception exception)
                {
                    _logger.LogWarning(
                        exception,
                        "RabbitMQ is not reachable. Retrying stock quote response listener startup in 5 seconds.");

                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
            }
        }

        private async Task HandleStockQuoteResponseAsync(
            StockQuoteResponseDto response,
            CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var chatService = scope.ServiceProvider.GetRequiredService<IChatService>();
            var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<ChatHub>>();

            var message = await chatService.SendBotMessageAsync(
                response.ChatRoomId,
                response.Content,
                cancellationToken);

            await hubContext.Clients
                .Group(message.ChatRoomId)
                .SendAsync("ReceiveMessage", message, cancellationToken);
        }
    }
}
