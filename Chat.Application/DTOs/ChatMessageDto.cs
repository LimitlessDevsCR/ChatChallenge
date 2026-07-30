namespace Chat.Application.DTOs
{
    public sealed record ChatMessageDto(
        int Id,
        string ChatRoomId,
        string UserId,
        string UserName,
        string Content,
        DateTime CreatedAtUtc,
        bool IsBotMessage);
}
