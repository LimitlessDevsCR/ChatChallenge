using Chat.Application.DTOs;

namespace Chat.Application.Interfaces
{
    public interface IStockQuoteResponseConsumer
    {
        Task StartConsumingAsync(
            Func<StockQuoteResponseDto, CancellationToken, Task> handleResponseAsync,
            CancellationToken cancellationToken = default);
    }
}
