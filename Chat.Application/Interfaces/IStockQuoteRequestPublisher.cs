using Chat.Application.DTOs;

namespace Chat.Application.Interfaces
{
    public interface IStockQuoteRequestPublisher
    {
        Task PublishAsync(
            StockQuoteRequestDto request,
            CancellationToken cancellationToken = default);
    }
}
