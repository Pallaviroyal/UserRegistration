// Services/IChatService.cs
using backend.Models;
using backend.Models;
using backend.Models.DTOs;

namespace backend.Services
{
    public interface IChatService
    {
        Task<Message> SendMessage(Guid senderId, Guid? receiverId, Guid? groupId, string content, MessageType type, string? fileUrl = null);
        Task<IEnumerable<MessageDto>> GetPrivateMessages(Guid userId, Guid otherUserId);
        Task<IEnumerable<MessageDto>> GetGroupMessages(Guid groupId);
        Task<Group> CreateGroup(Guid creatorId, string name, string? description, IEnumerable<Guid> memberIds);
        Task AddUserToGroup(Guid groupId, Guid userId, Guid requesterId);
        Task RemoveUserFromGroup(Guid groupId, Guid userId, Guid requesterId);
        Task<IEnumerable<GroupDto>> GetUserGroups(Guid userId);
        Task<IEnumerable<UserDto>> GetGroupMembers(Guid groupId);
    }
}