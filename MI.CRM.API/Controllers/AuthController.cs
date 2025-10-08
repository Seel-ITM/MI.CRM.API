using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Isopoh.Cryptography.Argon2;
using MI.CRM.API.Dtos;
using MI.CRM.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace MI.CRM.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class AuthController : ControllerBase
    {
        private readonly MicrmContext _context;
        private readonly IConfiguration _configuration;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _containerName;

        public AuthController(MicrmContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;

            var connectionString = _configuration["AzureStorage:ConnectionString"];
            _containerName = _configuration["AzureStorage:ContainerName"];

            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentNullException(nameof(connectionString), "Azure Storage connection string is missing.");

            _blobServiceClient = new BlobServiceClient(connectionString);
        }

        [HttpPost("register")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Register([FromForm] RegisterDto dto)
        {
            // 1. Check if email already exists
            if (await _context.Users.AsNoTracking().AnyAsync(u => u.Email == dto.Email))
                return BadRequest("Email already exists.");

            string? url = null;

            if(dto.ImageFile != null)
            {
                // 1️⃣ Validate file type (optional but recommended)
                var allowedExtensions = new[] { ".png", ".jpg", ".jpeg", ".webp" };
                var fileExtension = Path.GetExtension(dto.ImageFile.FileName).ToLower();
                if (!allowedExtensions.Contains(fileExtension))
                    return BadRequest("File type not allowed.");

                // 2️⃣ Get the container client
                var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
                await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob); // allows public read

                // 3️⃣ Generate a unique file name to avoid overwriting
                var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
                var blobClient = containerClient.GetBlobClient(uniqueFileName);

                // 4️⃣ Upload the file
                using (var stream = dto.ImageFile.OpenReadStream())
                {
                    var blobHttpHeaders = new Azure.Storage.Blobs.Models.BlobHttpHeaders
                    {
                        ContentType = dto.ImageFile.ContentType // ensures browser can open the file
                    };
                    await blobClient.UploadAsync(stream, new Azure.Storage.Blobs.Models.BlobUploadOptions
                    {
                        HttpHeaders = blobHttpHeaders,
                    });
                }

                url = blobClient.Uri.ToString();


            }


            // 2. Generate salt
            var salt = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            // 3. Configure Argon2
            string password = dto.Password;
            var config = new Argon2Config
            {
                Type = Argon2Type.HybridAddressing,
                Version = Argon2Version.Nineteen,
                TimeCost = 2,             // Iterations
                MemoryCost = 1 << 15,     // 32 MB
                Lanes = 1,
                Threads = 1,
                Salt = salt,
                HashLength = 32,
                Password = System.Text.Encoding.UTF8.GetBytes(password),
            };

            // Use the static method to hash the password
            string hash = Argon2.Hash(config);
            //using (var hasher = new Argon2(config))
            //{
            //    hash = A.Hash(dto.Password);
            //}

            // 4. Store user
            var user = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                RoleId = dto.RoleId,
                Password = hash,
                CreatedOn = DateTime.UtcNow,
                ImageUrl = url
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            int projectMangerRoleId = 1002; // Assuming 1002 is the Project Manager role ID
            if (user.RoleId == projectMangerRoleId)
            {
                _context.ProjectManagers.Add(new ProjectManager
                {
                    UserId = user.UserId,
                });

                await _context.SaveChangesAsync();
            }

            return Ok(new { message = "User registered successfully" });

        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null)
                return Unauthorized("Invalid email or password");

            // Extract parts of stored hash (includes salt internally)
            if (!Argon2.Verify(user.Password, dto.Password))
                return Unauthorized("Invalid email or password");

            // Create JWT token
            var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Name, user.Name),
                    new Claim(ClaimTypes.Role, user.RoleId.ToString())
                };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(12),
                signingCredentials: creds
            );

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token)
                //expires = token.ValidTo,
                //user = new { user.UserId, user.Name, user.Email, user.RoleId}
            });
        }

    }
}
