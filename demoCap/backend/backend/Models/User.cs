using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
//using backend.Models.Message;


    namespace backend.Models
    {
        public class User
        {
            [Key]
            public Guid Id { get; set; } = Guid.NewGuid();

            [Required]
            [MaxLength(50)]
            public string UserName { get; set; } = string.Empty;

            [Required]
            [EmailAddress]
            public string Email { get; set; } = string.Empty;

            [JsonIgnore]
            public string PasswordHash { get; set; } = string.Empty;

            public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

            public DateTime? LastLogin { get; set; }

            public UserStatus Status { get; set; } = UserStatus.Offline;

            [JsonIgnore]
            public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

            [JsonIgnore]
            public virtual ICollection<GroupUser> GroupUsers { get; set; } = new List<GroupUser>();
        }

        public enum UserStatus
        {
            Online,
            Offline,
            Busy,
            Available
        }
    }