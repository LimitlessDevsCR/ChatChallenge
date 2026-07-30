using Chat.Application.Interfaces;
using Chat.Application.Services;
using Chat.Domain.Entities;

namespace Chat.Tests
{
    public class ChatServiceTests
    {
        [Fact]
        public async Task SendMessageAsync_SavesNormalizedMessage()
        {
            var repository = new FakeMessageRepository();
            var service = new ChatService(repository);

            var message = await service.SendMessageAsync(" General ", "user-1", " Test User ", " Hello chat ");

            Assert.Equal("general", message.ChatRoomId);
            Assert.Equal("user-1", message.UserId);
            Assert.Equal("Test User", message.UserName);
            Assert.Equal("Hello chat", message.Content);
            Assert.False(message.IsBotMessage);
            Assert.Single(repository.Messages);
        }

        [Fact]
        public async Task SendMessageAsync_RejectsUnsupportedRoom()
        {
            var service = new ChatService(new FakeMessageRepository());

            await Assert.ThrowsAsync<ArgumentException>(() =>
                service.SendMessageAsync("unsupported", "user-1", "Test User", "Hello chat"));
        }

        [Fact]
        public async Task SendMessageAsync_RejectsEmptyContent()
        {
            var service = new ChatService(new FakeMessageRepository());

            await Assert.ThrowsAsync<ArgumentException>(() =>
                service.SendMessageAsync("general", "user-1", "Test User", "   "));
        }

        [Fact]
        public async Task GetLatestMessagesAsync_CapsRequestedCountAtFifty()
        {
            var repository = new FakeMessageRepository();
            var service = new ChatService(repository);

            await service.GetLatestMessagesAsync("general", 500);

            Assert.Equal(50, repository.LastRequestedCount);
        }

        [Fact]
        public async Task GetLatestMessagesAsync_ReturnsUtcDateTimesFromPersistedMessages()
        {
            var repository = new FakeMessageRepository();
            repository.Messages.Add(new Message
            {
                Id = 1,
                ChatRoomId = "stocks",
                UserId = "user-1",
                UserName = "Test User",
                Content = "Persisted message",
                CreatedAtUtc = new DateTime(2026, 7, 30, 22, 57, 0, DateTimeKind.Unspecified)
            });

            var service = new ChatService(repository);

            var messages = await service.GetLatestMessagesAsync("stocks");

            Assert.Equal(DateTimeKind.Utc, messages.Single().CreatedAtUtc.Kind);
        }

        [Fact]
        public void GetRooms_ReturnsSupportedRooms()
        {
            var service = new ChatService(new FakeMessageRepository());

            var rooms = service.GetRooms();

            Assert.Collection(
                rooms,
                room => Assert.Equal("general", room.Id),
                room => Assert.Equal("stocks", room.Id),
                room => Assert.Equal("random", room.Id));
        }

        private sealed class FakeMessageRepository : IMessageRepository
        {
            public List<Message> Messages { get; } = [];

            public int LastRequestedCount { get; private set; }

            public Task<Message> AddAsync(Message message, CancellationToken cancellationToken = default)
            {
                message.Id = Messages.Count + 1;
                Messages.Add(message);

                return Task.FromResult(message);
            }

            public Task<IReadOnlyList<Message>> GetLatestAsync(
                string chatRoomId,
                int count,
                CancellationToken cancellationToken = default)
            {
                LastRequestedCount = count;

                IReadOnlyList<Message> messages = Messages
                    .Where(message => message.ChatRoomId == chatRoomId)
                    .Take(count)
                    .ToList();

                return Task.FromResult(messages);
            }
        }
    }
}
