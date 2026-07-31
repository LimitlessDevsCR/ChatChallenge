using Chat.Application.DTOs;

namespace Chat.Application.Interfaces
{
    public interface IStockQuoteRequestConsumer
    {
        Task StartConsumingAsync(
            Func<StockQuoteRequestDto, CancellationToken, Task> handleRequestAsync,
            CancellationToken cancellationToken = default);
    }
}
