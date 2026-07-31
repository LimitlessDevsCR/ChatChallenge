namespace Chat.Application.DTOs
{
    public sealed record StockQuoteResponseDto(
        string ChatRoomId,
        string StockCode,
        string Content,
        bool IsSuccess,
        DateTime CreatedAtUtc);
}
