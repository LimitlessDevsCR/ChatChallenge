namespace Chat.Bot
{
    using Chat.Application.DTOs;
    using Chat.Application.Interfaces;

    public class Worker(
        IStockQuoteRequestConsumer stockQuoteRequestConsumer,
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

        private Task HandleStockQuoteRequestAsync(
            StockQuoteRequestDto request,
            CancellationToken cancellationToken)
        {
            logger.LogInformation(
                "Received stock quote request for {StockCode} in room {ChatRoomId} from {UserName}.",
                request.StockCode,
                request.ChatRoomId,
                request.RequestedByUserName);

            return Task.CompletedTask;
        }
    }
}
