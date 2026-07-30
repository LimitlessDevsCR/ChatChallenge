using Chat.App.ViewModels;
using Chat.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chat.App.Controllers
{
    [Authorize]
    public class ChatController : Controller
    {
        private readonly IChatService _chatService;

        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
        }

        public IActionResult Index(string roomId = "general")
        {
            var rooms = _chatService.GetRooms();
            var currentRoomId = rooms.Any(room => room.Id == roomId) ? roomId : "general";

            var viewModel = new ChatIndexViewModel
            {
                Rooms = rooms,
                CurrentRoomId = currentRoomId
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Messages(string roomId = "general", CancellationToken cancellationToken = default)
        {
            try
            {
                var messages = await _chatService.GetLatestMessagesAsync(roomId, cancellationToken: cancellationToken);
                return Ok(messages);
            }
            catch (ArgumentException exception)
            {
                return BadRequest(new { error = exception.Message });
            }
        }
    }
}
