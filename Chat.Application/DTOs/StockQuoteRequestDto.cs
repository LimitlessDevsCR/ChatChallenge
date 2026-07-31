namespace Chat.Application.DTOs
{
    public sealed record StockQuoteRequestDto(
        string ChatRoomId,
        string StockCode,
        string RequestedByUserId,
        string RequestedByUserName,
        DateTime RequestedAtUtc);
}
