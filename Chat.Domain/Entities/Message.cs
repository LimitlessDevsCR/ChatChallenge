namespace Chat.Domain.Entities
{
    public class Message
    {
        public int Id { get; set; }
        public string ChatRoomId { get; set; } = "general";
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAtUtc { get; set; }
        public bool IsBotMessage { get; set; }
    }
}
