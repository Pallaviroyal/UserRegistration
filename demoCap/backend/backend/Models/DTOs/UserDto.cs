// Models/DTOs/UserDto.cs
using backend.Models;

namespace backend.Models.DTOs
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public UserStatus Status { get; set; }
        public DateTime? LastLogin { get; set; }
    }
}