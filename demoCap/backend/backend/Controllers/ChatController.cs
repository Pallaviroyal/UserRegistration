
using backend.Models;
using backend.Models.DTOs;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/chat")]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;

        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
        }

        [HttpGet("private/{userId}")]
        public async Task<IActionResult> GetPrivateMessages(Guid userId)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var messages = await _chatService.GetPrivateMessages(currentUserId, userId);
                return Ok(messages);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
        {
            try
            {
                var senderId = GetCurrentUserId();
                var message = await _chatService.SendMessage(
                    senderId,
                    request.ReceiverId,
                    request.GroupId,
                    request.Content,
                    request.Type,
                    request.FileUrl);

                return Ok(new MessageDto
                {
                    Id = message.Id,
                    Content = message.Content,
                    Timestamp = message.Timestamp,
                    Type = message.Type,
                    FileUrl = message.FileUrl,
                    SenderId = message.SenderId,
                    SenderName = message.Sender.UserName,
                    ReceiverId = message.ReceiverId,
                    GroupId = message.GroupId
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("group/{groupId}")]
        public async Task<IActionResult> GetGroupMessages(Guid groupId)
        {
            try
            {
                var messages = await _chatService.GetGroupMessages(groupId);
                return Ok(messages);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                throw new UnauthorizedAccessException("User ID not found");
            }
            return userId;
        }
    }

    public class SendMessageRequest
    {
        public Guid? ReceiverId { get; set; }
        public Guid? GroupId { get; set; }
        public string Content { get; set; } = string.Empty;
        public MessageType Type { get; set; } = MessageType.Text;
        public string? FileUrl { get; set; }
    }
}
