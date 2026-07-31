using Chat.Application.DTOs;

namespace Chat.Application.Interfaces
{
    public interface IStockQuoteService
    {
        Task<StockQuoteDto> GetQuoteAsync(
            string stockCode,
            CancellationToken cancellationToken = default);
    }
}
