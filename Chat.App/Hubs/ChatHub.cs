using System.Security.Claims;
using Chat.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Chat.App.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IChatService _chatService;

        public ChatHub(IChatService chatService)
        {
            _chatService = chatService;
        }

        public async Task JoinRoom(string roomId)
        {
            ValidateRoom(roomId);

            await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
        }

        public async Task LeaveRoom(string roomId)
        {
            ValidateRoom(roomId);

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);
        }

        public async Task SendMessage(string roomId, string content)
        {
            try
            {
                var message = await _chatService.SendUserInputAsync(
                    roomId,
                    GetCurrentUserId(),
                    GetCurrentUserName(),
                    content,
                    Context.ConnectionAborted);

                if (message is not null)
                {
                    await Clients.Group(message.ChatRoomId).SendAsync(
                        "ReceiveMessage",
                        message,
                        Context.ConnectionAborted);
                }
            }
            catch (ArgumentException exception)
            {
                throw new HubException(exception.Message);
            }
            catch (InvalidOperationException exception)
            {
                throw new HubException(exception.Message);
            }
        }

        private void ValidateRoom(string roomId)
        {
            if (!_chatService.GetRooms().Any(room => room.Id == roomId))
            {
                throw new HubException("Chat room is not supported.");
            }
        }

        private string GetCurrentUserId()
        {
            return Context.User?.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new HubException("Authenticated user id was not found.");
        }

        private string GetCurrentUserName()
        {
            return Context.User?.Identity?.Name
                ?? Context.User?.FindFirstValue(ClaimTypes.Email)
                ?? "Unknown user";
        }
    }
}
