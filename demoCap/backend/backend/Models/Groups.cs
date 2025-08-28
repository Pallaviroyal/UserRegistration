// Models/Group.cs
using backend.Models;
using System.ComponentModel.DataAnnotations;

namespace backend.Models
{
    public class Group
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public Guid CreatedById { get; set; }

        public virtual User CreatedBy { get; set; } = null!;

        public virtual ICollection<GroupUser> GroupUsers { get; set; } = new List<GroupUser>();

        public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
    }
}