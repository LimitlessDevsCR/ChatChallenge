namespace Chat.Application.DTOs
{
    public sealed record StockQuoteDto(
        string Symbol,
        decimal ClosePrice);
}
