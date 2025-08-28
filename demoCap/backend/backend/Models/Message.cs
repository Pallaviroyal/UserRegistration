using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Text.RegularExpressions;

    namespace backend.Models
    {
        public class Message
        {
            [Key]
            public Guid Id { get; set; } = Guid.NewGuid();

            [Required]
            public string Content { get; set; } = string.Empty;

            public DateTime Timestamp { get; set; } = DateTime.UtcNow;

            public MessageType Type { get; set; } = MessageType.Text;

            public string? FileUrl { get; set; }

            [Required]
            public Guid SenderId { get; set; }

            [ForeignKey("SenderId")]
            public virtual User Sender { get; set; } = null!;

            public Guid? ReceiverId { get; set; }

            [ForeignKey("ReceiverId")]
            public virtual User? Receiver { get; set; }

            public Guid? GroupId { get; set; }

            [ForeignKey("GroupId")]
            public virtual Group? Group { get; set; }
        }

        public enum MessageType
        {
            Text,
            Image,
            File

        }
    }

