
using backend.Data;
using backend.Models;
using backend.Models.DTOs;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("status")]
        public async Task<IActionResult> GetUsersStatus()
        {
            var users = await _context.Users
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    Email = u.Email,
                    Status = u.Status,
                    LastLogin = u.LastLogin
                })
                .ToListAsync();

            return Ok(users);
        }

        [HttpPut("update-status")]
        public async Task<IActionResult> UpdateUserStatus([FromBody] UpdateStatusRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                var user = await _context.Users.FindAsync(userId);

                if (user == null)
                    return NotFound(new { message = "User not found" });

                user.Status = request.Status;
                await _context.SaveChangesAsync();

                return Ok(new { message = "Status updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("group/create")]
        public async Task<IActionResult> CreateGroup([FromBody] CreateGroupRequest request)
        {
            try
            {
                var creatorId = GetCurrentUserId();
                var chatService = HttpContext.RequestServices.GetRequiredService<IChatService>();
                var group = await chatService.CreateGroup(creatorId, request.Name, request.Description, request.MemberIds);

                return Ok(new GroupDto
                {
                    Id = group.Id,
                    Name = group.Name,
                    Description = group.Description,
                    CreatedAt = group.CreatedAt,
                    CreatedById = group.CreatedById,
                    CreatedByName = group.CreatedBy.UserName,
                    MemberCount = group.GroupUsers.Count
                });
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

    public class UpdateStatusRequest
    {
        public UserStatus Status { get; set; }
    }

    public class CreateGroupRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public List<Guid> MemberIds { get; set; } = new List<Guid>();
    }
}