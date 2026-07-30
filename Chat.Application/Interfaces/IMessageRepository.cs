using Chat.Domain.Entities;

namespace Chat.Application.Interfaces
{
    public interface IMessageRepository
    {
        Task<Message> AddAsync(Message message, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<Message>> GetLatestAsync(
            string chatRoomId,
            int count,
            CancellationToken cancellationToken = default);
    }
}
