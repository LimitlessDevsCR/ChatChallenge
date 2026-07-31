using Chat.Application.Interfaces;
using Chat.Application.Services;
using Chat.Application.DTOs;
using Chat.Domain.Entities;
using Chat.Infrastructure.StockApi;

namespace Chat.Tests
{
    public class ChatServiceTests
    {
        [Fact]
        public async Task SendMessageAsync_SavesNormalizedMessage()
        {
            var repository = new FakeMessageRepository();
            var service = new ChatService(repository, new FakeStockQuoteRequestPublisher());

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
            var service = new ChatService(new FakeMessageRepository(), new FakeStockQuoteRequestPublisher());

            await Assert.ThrowsAsync<ArgumentException>(() =>
                service.SendMessageAsync("unsupported", "user-1", "Test User", "Hello chat"));
        }

        [Fact]
        public async Task SendMessageAsync_RejectsEmptyContent()
        {
            var service = new ChatService(new FakeMessageRepository(), new FakeStockQuoteRequestPublisher());

            await Assert.ThrowsAsync<ArgumentException>(() =>
                service.SendMessageAsync("general", "user-1", "Test User", "   "));
        }

        [Fact]
        public async Task GetLatestMessagesAsync_CapsRequestedCountAtFifty()
        {
            var repository = new FakeMessageRepository();
            var service = new ChatService(repository, new FakeStockQuoteRequestPublisher());

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

            var service = new ChatService(repository, new FakeStockQuoteRequestPublisher());

            var messages = await service.GetLatestMessagesAsync("stocks");

            Assert.Equal(DateTimeKind.Utc, messages.Single().CreatedAtUtc.Kind);
        }

        [Fact]
        public void GetRooms_ReturnsSupportedRooms()
        {
            var service = new ChatService(new FakeMessageRepository(), new FakeStockQuoteRequestPublisher());

            var rooms = service.GetRooms();

            Assert.Collection(
                rooms,
                room => Assert.Equal("general", room.Id),
                room => Assert.Equal("stocks", room.Id),
                room => Assert.Equal("random", room.Id));
        }

        [Fact]
        public async Task SendUserInputAsync_PublishesStockCommandWithoutSavingMessage()
        {
            var repository = new FakeMessageRepository();
            var publisher = new FakeStockQuoteRequestPublisher();
            var service = new ChatService(repository, publisher);

            var message = await service.SendUserInputAsync(
                "Stocks",
                "user-1",
                "Test User",
                "/stock=AAPL.US");

            Assert.Null(message);
            Assert.Empty(repository.Messages);
            var request = Assert.Single(publisher.Requests);
            Assert.Equal("stocks", request.ChatRoomId);
            Assert.Equal("aapl.us", request.StockCode);
            Assert.Equal("user-1", request.RequestedByUserId);
            Assert.Equal("Test User", request.RequestedByUserName);
            Assert.Equal(DateTimeKind.Utc, request.RequestedAtUtc.Kind);
        }

        [Fact]
        public async Task SendBotMessageAsync_SavesBotMessage()
        {
            var repository = new FakeMessageRepository();
            var service = new ChatService(repository, new FakeStockQuoteRequestPublisher());

            var message = await service.SendBotMessageAsync("general", "AAPL.US quote is $93.42 per share");

            Assert.Equal("general", message.ChatRoomId);
            Assert.Equal("stock-bot", message.UserId);
            Assert.Equal("Stock Bot", message.UserName);
            Assert.Equal("AAPL.US quote is $93.42 per share", message.Content);
            Assert.True(message.IsBotMessage);
            Assert.Single(repository.Messages);
        }

        [Fact]
        public void ParseQuote_ReturnsSymbolAndClosePrice()
        {
            const string csv = """
                Date,Open,High,Low,Close,Volume
                2026-07-30,213.9,215.2,212.4,214.55,123456
                """;

            var quote = StooqStockQuoteService.ParseQuote(csv, "aapl.us");

            Assert.Equal("AAPL.US", quote.Symbol);
            Assert.Equal(214.55m, quote.ClosePrice);
        }

        [Fact]
        public void ParseQuote_RejectsMissingQuote()
        {
            const string csv = """
                Date,Open,High,Low,Close,Volume
                N/D,N/D,N/D,N/D,N/D,N/D
                """;

            Assert.Throws<InvalidOperationException>(() =>
                StooqStockQuoteService.ParseQuote(csv, "aapl.us"));
        }

        [Fact]
        public void ParseQuote_ReturnsClosePriceFromAssignmentCsvShape()
        {
            const string csv = """
                Symbol,Date,Time,Open,High,Low,Close,Volume
                AAPL.US,2026-07-31,22:00:08,213.9,215.2,212.4,214.55,123456
                """;

            var quote = StooqStockQuoteService.ParseQuote(csv, "aapl.us");

            Assert.Equal("AAPL.US", quote.Symbol);
            Assert.Equal(214.55m, quote.ClosePrice);
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

        private sealed class FakeStockQuoteRequestPublisher : IStockQuoteRequestPublisher
        {
            public List<StockQuoteRequestDto> Requests { get; } = [];

            public Task PublishAsync(
                StockQuoteRequestDto request,
                CancellationToken cancellationToken = default)
            {
                Requests.Add(request);

                return Task.CompletedTask;
            }
        }
    }
}
