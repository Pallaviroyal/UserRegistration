// Services/ChatService.cs
using backend.Models;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;
using backend.Models.DTOs;

namespace backend.Services
{
    public class ChatService : IChatService
    {
        private readonly ApplicationDbContext _context;

        public ChatService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Message> SendMessage(Guid senderId, Guid? receiverId, Guid? groupId, string content, MessageType type, string? fileUrl = null)
        {
            if (receiverId == null && groupId == null)
                throw new Exception("Message must have either a receiver or a group");

            var message = new Message
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                GroupId = groupId,
                Content = content,
                Type = type,
                FileUrl = fileUrl,
                Timestamp = DateTime.UtcNow
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            return message;
        }

        public async Task<IEnumerable<MessageDto>> GetPrivateMessages(Guid userId, Guid otherUserId)
        {
            return await _context.Messages
                .Where(m => (m.SenderId == userId && m.ReceiverId == otherUserId) ||
                           (m.SenderId == otherUserId && m.ReceiverId == userId))
                .OrderBy(m => m.Timestamp)
                .Select(m => new MessageDto
                {
                    Id = m.Id,
                    Content = m.Content,
                    Timestamp = m.Timestamp,
                    Type = m.Type,
                    FileUrl = m.FileUrl,
                    SenderId = m.SenderId,
                    SenderName = m.Sender.UserName,
                    ReceiverId = m.ReceiverId
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<MessageDto>> GetGroupMessages(Guid groupId)
        {
            return await _context.Messages
                .Where(m => m.GroupId == groupId)
                .OrderBy(m => m.Timestamp)
                .Select(m => new MessageDto
                {
                    Id = m.Id,
                    Content = m.Content,
                    Timestamp = m.Timestamp,
                    Type = m.Type,
                    FileUrl = m.FileUrl,
                    SenderId = m.SenderId,
                    SenderName = m.Sender.UserName,
                    GroupId = m.GroupId
                })
                .ToListAsync();
        }

        public async Task<Group> CreateGroup(Guid creatorId, string name, string? description, IEnumerable<Guid> memberIds)
        {
            var group = new Group
            {
                Name = name,
                Description = description,
                CreatedById = creatorId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Groups.Add(group);
            await _context.SaveChangesAsync();

            // Add creator as admin
            var groupUser = new GroupUser
            {
                GroupId = group.Id,
                UserId = creatorId,
                IsAdmin = true,
                JoinedAt = DateTime.UtcNow
            };
            _context.GroupUsers.Add(groupUser);

            // Add other members
            foreach (var memberId in memberIds.Where(id => id != creatorId))
            {
                _context.GroupUsers.Add(new GroupUser
                {
                    GroupId = group.Id,
                    UserId = memberId,
                    IsAdmin = false,
                    JoinedAt = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();
            return group;
        }

        public async Task AddUserToGroup(Guid groupId, Guid userId, Guid requesterId)
        {
            var groupUser = await _context.GroupUsers
                .FirstOrDefaultAsync(gu => gu.GroupId == groupId && gu.UserId == requesterId && gu.IsAdmin);

            if (groupUser == null)
                throw new UnauthorizedAccessException("Only group admins can add users");

            var existingMember = await _context.GroupUsers
                .FirstOrDefaultAsync(gu => gu.GroupId == groupId && gu.UserId == userId);

            if (existingMember != null)
                throw new Exception("User is already in the group");

            _context.GroupUsers.Add(new GroupUser
            {
                GroupId = groupId,
                UserId = userId,
                IsAdmin = false,
                JoinedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
        }

        public async Task RemoveUserFromGroup(Guid groupId, Guid userId, Guid requesterId)
        {
            var requesterGroupUser = await _context.GroupUsers
                .FirstOrDefaultAsync(gu => gu.GroupId == groupId && gu.UserId == requesterId && gu.IsAdmin);

            if (requesterGroupUser == null)
                throw new UnauthorizedAccessException("Only group admins can remove users");

            var userGroupUser = await _context.GroupUsers
                .FirstOrDefaultAsync(gu => gu.GroupId == groupId && gu.UserId == userId);

            if (userGroupUser == null)
                throw new Exception("User is not in the group");

            _context.GroupUsers.Remove(userGroupUser);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<GroupDto>> GetUserGroups(Guid userId)
        {
            return await _context.GroupUsers
                .Where(gu => gu.UserId == userId)
                .Select(gu => new GroupDto
                {
                    Id = gu.Group.Id,
                    Name = gu.Group.Name,
                    Description = gu.Group.Description,
                    CreatedAt = gu.Group.CreatedAt,
                    CreatedById = gu.Group.CreatedById,
                    CreatedByName = gu.Group.CreatedBy.UserName,
                    MemberCount = gu.Group.GroupUsers.Count
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<UserDto>> GetGroupMembers(Guid groupId)
        {
            return await _context.GroupUsers
                .Where(gu => gu.GroupId == groupId)
                .Select(gu => new UserDto
                {
                    Id = gu.User.Id,
                    UserName = gu.User.UserName,
                    Email = gu.User.Email,
                    Status = gu.User.Status,
                    LastLogin = gu.User.LastLogin
                })
                .ToListAsync();
        }
    }
}