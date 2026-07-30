using Chat.Application.DTOs;

namespace Chat.App.ViewModels
{
    public class ChatIndexViewModel
    {
        public IReadOnlyList<ChatRoomDto> Rooms { get; set; } = [];

        public string CurrentRoomId { get; set; } = "general";
    }
}
