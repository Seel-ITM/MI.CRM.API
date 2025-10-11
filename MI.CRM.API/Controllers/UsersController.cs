using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Isopoh.Cryptography.Argon2;
using MI.CRM.API.Dtos;
using MI.CRM.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.PowerBI.Api.Models;
using System;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace MI.CRM.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly MicrmContext _context;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _containerName;
        public UsersController(MicrmContext context, IConfiguration configuration)
        {
            _context = context;

            var connectionString = configuration["AzureStorage:ConnectionString"];
            _containerName = configuration["AzureStorage:ContainerName"];
            _blobServiceClient = new BlobServiceClient(connectionString);
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

        [HttpPut("update")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateUser([FromForm] UserUpdateDto dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest("Invalid request data.");

                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == dto.Id);
                if (user == null)
                    return NotFound("User not found.");

                // Check if container exists (do NOT create)
                var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
                if (!await containerClient.ExistsAsync())
                    return StatusCode(StatusCodes.Status500InternalServerError, "Azure blob container does not exist.");

                // ✅ Handle new image upload
                if (dto.ImageFile != null)
                {
                    var allowedExtensions = new[] { ".png", ".jpg", ".jpeg", ".webp" };
                    var fileExtension = Path.GetExtension(dto.ImageFile.FileName).ToLower();

                    if (!allowedExtensions.Contains(fileExtension))
                        return BadRequest("File type not allowed.");

                    var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
                    var blobClient = containerClient.GetBlobClient(uniqueFileName);

                    // Upload new image
                    using (var stream = dto.ImageFile.OpenReadStream())
                    {
                        var blobHttpHeaders = new BlobHttpHeaders
                        {
                            ContentType = dto.ImageFile.ContentType
                        };

                        await blobClient.UploadAsync(stream, new BlobUploadOptions
                        {
                            HttpHeaders = blobHttpHeaders
                        });
                    }

                    var newImageUrl = blobClient.Uri.ToString();

                    // Delete old image if exists
                    if (!string.IsNullOrEmpty(user.ImageUrl))
                    {
                        try
                        {
                            var oldBlobUri = new Uri(user.ImageUrl);
                            var oldBlobName = Path.GetFileName(oldBlobUri.LocalPath);
                            await containerClient.DeleteBlobIfExistsAsync(oldBlobName);
                        }
                        catch (Exception ex)
                        {
                            // Log internally but do not stop user update
                            Console.WriteLine($"[Warning] Failed to delete old blob: {ex.Message}");
                        }
                    }

                    user.ImageUrl = newImageUrl;
                }

                // ✅ Update basic fields
                user.Name = dto.Name ?? user.Name;
                user.Email = dto.Email ?? user.Email;
                user.RoleId = dto.RoleId != 0 ? dto.RoleId : user.RoleId;

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "User updated successfully",
                    user = new UserDto
                    {
                        UserId = user.UserId,
                        Name = user.Name,
                        Email = user.Email,
                        RoleId = user.RoleId,
                        ImageUrl = user.ImageUrl
                    }
                });
            }
            catch (DbUpdateException dbEx)
            {
                Console.WriteLine($"[DB ERROR] {dbEx.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, "Database update failed. Please try again.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred while updating the user.");
            }
        }

        [HttpPost("verify-password")]
        public async Task<IActionResult> VerifyPassword([FromBody] VerifyPasswordDto dto)
        {
            try
            {
                var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == dto.UserId);
                if (user == null)
                    return NotFound("User not found.");

                bool isValid = Argon2.Verify(user.Password, dto.CurrentPassword);
                return Ok(isValid);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error verifying password: {ex.Message}");
            }
        }

        [HttpPut("update-password")]
        public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordDto dto)
        {
            try
            {
                var user = await _context.Users.FindAsync(dto.UserId);
                if (user == null)
                    return NotFound("User not found.");

                // 1️⃣ Generate salt
                var salt = new byte[16];
                using (var rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(salt);
                }

                // 2️⃣ Setup Argon2 config (same as registration)
                var config = new Argon2Config
                {
                    Type = Argon2Type.HybridAddressing,
                    Version = Argon2Version.Nineteen,
                    TimeCost = 2,          // Iterations
                    MemoryCost = 1 << 15,  // 32 MB
                    Lanes = 1,
                    Threads = 1,
                    Salt = salt,
                    HashLength = 32,
                    Password = Encoding.UTF8.GetBytes(dto.NewPassword)
                };

                string newHash = Argon2.Hash(config);
                user.Password = newHash;

                await _context.SaveChangesAsync();

                return Ok(new { message = "Password updated successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error updating password: {ex.Message}");
            }
        }

    }
}
