using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Security.Claims;

namespace backend.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IChatService _chatService;
        private static readonly ConcurrentDictionary<Guid, string> _userConnections = new();

        public ChatHub(IChatService chatService)
        {
            _chatService = chatService;
        }

        public override async Task OnConnectedAsync()
        {
            var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                // Invalid user → disconnect
                Context.Abort();
                return;
            }

            // Thread-safe add/update
            _userConnections.AddOrUpdate(userId, Context.ConnectionId, (key, old) => Context.ConnectionId);

            // TODO: Update user status to "online" in DB via _chatService if needed

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
            {
                _userConnections.TryRemove(userId, out _);

                // TODO: Update user status to "offline" in DB via _chatService if needed
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessageToUser(Guid receiverId, string content, string messageType, string? fileUrl = null)
        {
            var senderId = GetUserId();
            var message = await _chatService.SendMessage(
                senderId,
                receiverId,
                null,
                content,
                Enum.Parse<MessageType>(messageType),
                fileUrl);

            if (_userConnections.TryGetValue(receiverId, out var connectionId))
            {
                await Clients.Client(connectionId).SendAsync("ReceiveMessage", new
                {
                    message.Id,
                    message.Content,
                    message.Timestamp,
                    Type = message.Type.ToString(),
                    message.FileUrl,
                    SenderId = message.SenderId,
                    SenderName = message.Sender.UserName,
                    ReceiverId = message.ReceiverId
                });
            }

            await Clients.Caller.SendAsync("MessageSent", new
            {
                message.Id,
                message.Content,
                message.Timestamp,
                Type = message.Type.ToString(),
                message.FileUrl,
                SenderId = message.SenderId,
                SenderName = message.Sender.UserName,
                ReceiverId = message.ReceiverId
            });
        }

        public async Task SendMessageToGroup(Guid groupId, string content, string messageType, string? fileUrl = null)
        {
            var senderId = GetUserId();
            var message = await _chatService.SendMessage(
                senderId,
                null,
                groupId,
                content,
                Enum.Parse<MessageType>(messageType),
                fileUrl);

            await Clients.Group(groupId.ToString()).SendAsync("ReceiveGroupMessage", new
            {
                message.Id,
                message.Content,
                message.Timestamp,
                Type = message.Type.ToString(),
                message.FileUrl,
                SenderId = message.SenderId,
                SenderName = message.Sender.UserName,
                GroupId = message.GroupId
            });
        }

        public async Task JoinGroup(Guid groupId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupId.ToString());
        }

        public async Task LeaveGroup(Guid groupId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupId.ToString());
        }

        public async Task Typing(Guid receiverId, bool isTyping)
        {
            if (_userConnections.TryGetValue(receiverId, out var connectionId))
            {
                await Clients.Client(connectionId).SendAsync("UserTyping", GetUserId(), isTyping);
            }
        }

        public async Task TypingInGroup(Guid groupId, bool isTyping)
        {
            await Clients.Group(groupId.ToString()).SendAsync("GroupUserTyping", GetUserId(), isTyping);
        }

        private Guid GetUserId()
        {
            var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                throw new HubException("User ID not found");
            }
            return userId;
        }
    }
}
