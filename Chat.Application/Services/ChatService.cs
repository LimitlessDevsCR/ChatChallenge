using Chat.Application.DTOs;
using Chat.Application.Interfaces;
using Chat.Domain.Entities;

namespace Chat.Application.Services
{
    public class ChatService : IChatService
    {
        private const int MaxMessageLength = 500;
        private const int MaxMessagesToLoad = 50;
        private const string StockCommandPrefix = "/stock=";
        private const string BotUserId = "stock-bot";
        private const string BotUserName = "Stock Bot";

        private static readonly IReadOnlyList<ChatRoomDto> Rooms =
        [
            new("general", "General"),
            new("stocks", "Stocks"),
            new("random", "Random")
        ];

        private readonly IMessageRepository _messageRepository;
        private readonly IStockQuoteRequestPublisher _stockQuoteRequestPublisher;

        public ChatService(
            IMessageRepository messageRepository,
            IStockQuoteRequestPublisher stockQuoteRequestPublisher)
        {
            _messageRepository = messageRepository;
            _stockQuoteRequestPublisher = stockQuoteRequestPublisher;
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

        public async Task<ChatMessageDto?> SendUserInputAsync(
            string chatRoomId,
            string userId,
            string userName,
            string content,
            CancellationToken cancellationToken = default)
        {
            var normalizedRoomId = NormalizeRoomId(chatRoomId);
            var normalizedContent = NormalizeContent(content);
            var normalizedUserName = NormalizeUserName(userName);

            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException("User id is required.", nameof(userId));
            }

            if (TryParseStockCommand(normalizedContent, out var stockCode))
            {
                await _stockQuoteRequestPublisher.PublishAsync(
                    new StockQuoteRequestDto(
                        normalizedRoomId,
                        stockCode,
                        userId,
                        normalizedUserName,
                        DateTime.UtcNow),
                    cancellationToken);

                return null;
            }

            return await SendMessageAsync(
                normalizedRoomId,
                userId,
                normalizedUserName,
                normalizedContent,
                cancellationToken);
        }

        public async Task<ChatMessageDto> SendBotMessageAsync(
            string chatRoomId,
            string content,
            CancellationToken cancellationToken = default)
        {
            var normalizedRoomId = NormalizeRoomId(chatRoomId);
            var normalizedContent = NormalizeContent(content);

            var message = new Message
            {
                ChatRoomId = normalizedRoomId,
                UserId = BotUserId,
                UserName = BotUserName,
                Content = normalizedContent,
                CreatedAtUtc = DateTime.UtcNow,
                IsBotMessage = true
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

        private static string NormalizeUserName(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName))
            {
                throw new ArgumentException("User name is required.", nameof(userName));
            }

            return userName.Trim();
        }

        private static bool TryParseStockCommand(string content, out string stockCode)
        {
            stockCode = string.Empty;

            if (!content.StartsWith(StockCommandPrefix, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            stockCode = content[StockCommandPrefix.Length..].Trim().ToLowerInvariant();

            if (string.IsNullOrWhiteSpace(stockCode))
            {
                throw new ArgumentException("Stock code is required.", nameof(content));
            }

            return true;
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
