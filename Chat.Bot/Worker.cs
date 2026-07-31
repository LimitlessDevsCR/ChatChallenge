namespace Chat.Bot
{
    using Chat.Application.DTOs;
    using Chat.Application.Interfaces;

    public class Worker(
        IStockQuoteRequestConsumer stockQuoteRequestConsumer,
        IServiceScopeFactory scopeFactory,
        ILogger<Worker> logger) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("Stock quote bot worker is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await stockQuoteRequestConsumer.StartConsumingAsync(
                        HandleStockQuoteRequestAsync,
                        stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    logger.LogInformation("Stock quote bot worker is stopping.");
                }
                catch (Exception exception)
                {
                    logger.LogWarning(
                        exception,
                        "RabbitMQ is not reachable. Retrying stock quote consumer startup in 5 seconds.");

                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
            }
        }

        private async Task HandleStockQuoteRequestAsync(
            StockQuoteRequestDto request,
            CancellationToken cancellationToken)
        {
            logger.LogInformation(
                "Received stock quote request for {StockCode} in room {ChatRoomId} from {UserName}.",
                request.StockCode,
                request.ChatRoomId,
                request.RequestedByUserName);

            var response = await CreateResponseAsync(request, cancellationToken);
            using var scope = scopeFactory.CreateScope();
            var stockQuoteResponsePublisher = scope.ServiceProvider.GetRequiredService<IStockQuoteResponsePublisher>();

            await stockQuoteResponsePublisher.PublishAsync(response, cancellationToken);
        }

        private async Task<StockQuoteResponseDto> CreateResponseAsync(
            StockQuoteRequestDto request,
            CancellationToken cancellationToken)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var stockQuoteService = scope.ServiceProvider.GetRequiredService<IStockQuoteService>();
                var quote = await stockQuoteService.GetQuoteAsync(request.StockCode, cancellationToken);

                return new StockQuoteResponseDto(
                    request.ChatRoomId,
                    request.StockCode,
                    $"{quote.Symbol} quote is ${quote.ClosePrice:0.##} per share",
                    true,
                    DateTime.UtcNow);
            }
            catch (Exception exception)
            {
                logger.LogWarning(
                    exception,
                    "Failed to get stock quote for {StockCode}.",
                    request.StockCode);

                return new StockQuoteResponseDto(
                    request.ChatRoomId,
                    request.StockCode,
                    $"Stock quote for {request.StockCode.ToUpperInvariant()} is not available.",
                    false,
                    DateTime.UtcNow);
            }
        }
    }
}
