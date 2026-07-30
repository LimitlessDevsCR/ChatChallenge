using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Domain.Entities
{
    public class Message
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAtUtc { get; set; }
        public bool IsBotMessage { get; set; }

    }
}
