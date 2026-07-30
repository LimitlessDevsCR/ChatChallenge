using Chat.Application.DTOs;
using Chat.Application.Interfaces;
using Chat.Domain.Entities;

namespace Chat.Application.Services
{
    public class ChatService : IChatService
    {
        private const int MaxMessageLength = 500;
        private const int MaxMessagesToLoad = 50;

        private static readonly IReadOnlyList<ChatRoomDto> Rooms =
        [
            new("general", "General"),
            new("stocks", "Stocks"),
            new("random", "Random")
        ];

        private readonly IMessageRepository _messageRepository;

        public ChatService(IMessageRepository messageRepository)
        {
            _messageRepository = messageRepository;
        }

        public IReadOnlyList<ChatRoomDto> GetRooms()
        {
            return Rooms;
        }

        public async Task<IReadOnlyList<ChatMessageDto>> GetLatestMessagesAsync(
            string chatRoomId,
            int count = MaxMessagesToLoad,
            CancellationToken cancellationToken = default)
        {
            var normalizedRoomId = NormalizeRoomId(chatRoomId);
            var messageCount = Math.Clamp(count, 1, MaxMessagesToLoad);

            var messages = await _messageRepository.GetLatestAsync(
                normalizedRoomId,
                messageCount,
                cancellationToken);

            return messages.Select(ToDto).ToList();
        }

        public async Task<ChatMessageDto> SendMessageAsync(
            string chatRoomId,
            string userId,
            string userName,
            string content,
            CancellationToken cancellationToken = default)
        {
            var normalizedRoomId = NormalizeRoomId(chatRoomId);
            var normalizedContent = NormalizeContent(content);

            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException("User id is required.", nameof(userId));
            }

            if (string.IsNullOrWhiteSpace(userName))
            {
                throw new ArgumentException("User name is required.", nameof(userName));
            }

            var message = new Message
            {
                ChatRoomId = normalizedRoomId,
                UserId = userId,
                UserName = userName.Trim(),
                Content = normalizedContent,
                CreatedAtUtc = DateTime.UtcNow,
                IsBotMessage = false
            };

            var savedMessage = await _messageRepository.AddAsync(message, cancellationToken);

            return ToDto(savedMessage);
        }

        private static string NormalizeRoomId(string chatRoomId)
        {
            var normalizedRoomId = string.IsNullOrWhiteSpace(chatRoomId)
                ? "general"
                : chatRoomId.Trim().ToLowerInvariant();

            if (!Rooms.Any(room => room.Id == normalizedRoomId))
            {
                throw new ArgumentException("Chat room is not supported.", nameof(chatRoomId));
            }

            return normalizedRoomId;
        }

        private static string NormalizeContent(string content)
        {
            var normalizedContent = content?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(normalizedContent))
            {
                throw new ArgumentException("Message content is required.", nameof(content));
            }

            if (normalizedContent.Length > MaxMessageLength)
            {
                throw new ArgumentException($"Message content cannot exceed {MaxMessageLength} characters.", nameof(content));
            }

            return normalizedContent;
        }

        private static ChatMessageDto ToDto(Message message)
        {
            return new ChatMessageDto(
                message.Id,
                message.ChatRoomId,
                message.UserId,
                message.UserName,
                message.Content,
                EnsureUtc(message.CreatedAtUtc),
                message.IsBotMessage);
        }

        private static DateTime EnsureUtc(DateTime value)
        {
            return value.Kind == DateTimeKind.Utc
                ? value
                : DateTime.SpecifyKind(value, DateTimeKind.Utc);
        }
    }
}
