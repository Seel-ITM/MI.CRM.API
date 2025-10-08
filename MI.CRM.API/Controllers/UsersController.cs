using MI.CRM.API.Dtos;
using MI.CRM.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.PowerBI.Api.Models;
using System.Security.Claims;

namespace MI.CRM.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly MicrmContext _context;
        public UsersController(MicrmContext context)
        {
            _context = context;
        }
        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _context.Users
                .Include(u => u.Role)
                .Select(u => new UserDto
                {
                    UserId = u.UserId,
                    Name = u.Name,
                    Email = u.Email,
                    RoleId = u.RoleId,
                })
                .ToListAsync();
            return Ok(users);
        }
        [HttpGet("get/{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _context.Users
            .Select(u => new UserDto
            {
                UserId = u.UserId,
                Name = u.Name,
                Email = u.Email,
                RoleId = u.RoleId,
                ImageUrl = u.ImageUrl,
            })
            .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null)
                return NotFound(new { message = "User not found" });
            return Ok(user);
        }
        [HttpGet("current")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var currentUserId = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId) ? userId : throw new UnauthorizedAccessException("User ID not found in token"); ;
            
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == currentUserId);
            if (user == null)
                return NotFound(new { message = "User not found" });
            return Ok(new UserDto
            {
                UserId = currentUserId,
                Name = user.Name,
                Email = user.Email,
                RoleId = user.RoleId,
                ImageUrl = user.ImageUrl
            });
        }
    }
}
