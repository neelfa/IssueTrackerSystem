using IssueTracker.Data;
using IssueTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace IssueTracker.Services
{
    public interface IAuthService
    {
        Task<User?> AuthenticateAsync(string email, string password);
        Task<User?> GetUserByEmailAsync(string email);
        Task<bool> IsAuthorizedAsync(string email, string requiredRole);
        Task<List<User>> GetUsersByRoleAsync(string role);
    }
    
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        
        public AuthService(AppDbContext context)
        {
            _context = context;
        }
        
        public async Task<User?> AuthenticateAsync(string email, string password)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email && u.Password == password);
        }
        
        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }
        
        public async Task<bool> IsAuthorizedAsync(string email, string requiredRole)
        {
            var user = await GetUserByEmailAsync(email);
            if (user == null) return false;
            
            // Admin can access everything
            if (user.Role == "Admin") return true;
            
            // Check specific role
            return user.Role == requiredRole;
        }
        
        public async Task<List<User>> GetUsersByRoleAsync(string role)
        {
            return await _context.Users
                .Where(u => u.Role == role)
                .OrderBy(u => u.Email)
                .ToListAsync();
        }
    }
}