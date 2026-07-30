using Chat.Application.Interfaces;
using Chat.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Chat.Infrastructure.Persistence.Repositories
{
    public class MessageRepository : IMessageRepository
    {
        private readonly ChatDbContext _dbContext;

        public MessageRepository(ChatDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Message> AddAsync(Message message, CancellationToken cancellationToken = default)
        {
            _dbContext.Messages.Add(message);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return message;
        }

        public async Task<IReadOnlyList<Message>> GetLatestAsync(
            string chatRoomId,
            int count,
            CancellationToken cancellationToken = default)
        {
            var messages = await _dbContext.Messages
                .AsNoTracking()
                .Where(message => message.ChatRoomId == chatRoomId)
                .OrderByDescending(message => message.CreatedAtUtc)
                .Take(count)
                .ToListAsync(cancellationToken);

            messages.Reverse();

            return messages;
        }
    }
}
