// Services/IAuthService.cs
using backend.Models;
using backend.Models;
using backend.Models.DTOs;

namespace backend.Services
{
    public interface IAuthService
    {
        Task<User> Register(RegisterRequest request);
        Task<string> Login(LoginRequest request);
        Task<bool> UserExists(string email);
        Task<User?> GetUserById(Guid id);
    }
}