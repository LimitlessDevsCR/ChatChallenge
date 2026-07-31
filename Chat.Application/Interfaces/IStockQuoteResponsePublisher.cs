using Chat.Application.DTOs;

namespace Chat.Application.Interfaces
{
    public interface IStockQuoteResponsePublisher
    {
        Task PublishAsync(
            StockQuoteResponseDto response,
            CancellationToken cancellationToken = default);
    }
}
