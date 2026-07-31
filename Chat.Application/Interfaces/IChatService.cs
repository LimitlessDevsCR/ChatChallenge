using Chat.Application.DTOs;

namespace Chat.Application.Interfaces
{
    public interface IChatService
    {
        IReadOnlyList<ChatRoomDto> GetRooms();

        Task<IReadOnlyList<ChatMessageDto>> GetLatestMessagesAsync(
            string chatRoomId,
            int count = 50,
            CancellationToken cancellationToken = default);

        Task<ChatMessageDto> SendMessageAsync(
            string chatRoomId,
            string userId,
            string userName,
            string content,
            CancellationToken cancellationToken = default);

        Task<ChatMessageDto?> SendUserInputAsync(
            string chatRoomId,
            string userId,
            string userName,
            string content,
            CancellationToken cancellationToken = default);
    }
}
