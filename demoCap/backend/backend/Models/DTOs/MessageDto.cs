using backend.Models;

namespace backend.Models.DTOs
{
    public class MessageDto
    {
        public Guid Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public MessageType Type { get; set; }
        public string? FileUrl { get; set; }
        public Guid SenderId { get; set; }
        public string SenderName { get; set; } = string.Empty;
        public Guid? ReceiverId { get; set; }
        public Guid? GroupId { get; set; }
    }
}