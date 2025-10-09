using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using MI.CRM.API.DTOs;
using MI.CRM.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MI.CRM.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentsController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _containerName;
        private readonly MicrmContext _context;

        public DocumentsController(IConfiguration configuration, MicrmContext context)
        {
            _configuration = configuration;
            var connectionString = _configuration["AzureStorage:ConnectionString"];
            _containerName = _configuration["AzureStorage:ContainerName"];

            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentNullException(nameof(connectionString), "Azure Storage connection string is missing.");

            _blobServiceClient = new BlobServiceClient(connectionString);
            _context = context;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload([FromForm] IFormFile file, [FromForm] int projectId)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            // 1️⃣ Validate file type (optional but recommended)
            var allowedExtensions = new[] { ".pdf", ".docx", ".xlsx", ".png", ".jpg", ".jpeg" };
            var fileExtension = Path.GetExtension(file.FileName).ToLower();
            if (!allowedExtensions.Contains(fileExtension))
                return BadRequest("File type not allowed.");

            // 2️⃣ Get the container client
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob); // allows public read

            // 3️⃣ Generate a unique file name to avoid overwriting
            var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
            var blobClient = containerClient.GetBlobClient(uniqueFileName);

            // 4️⃣ Upload the file
            using (var stream = file.OpenReadStream())
            {
                var blobHttpHeaders = new Azure.Storage.Blobs.Models.BlobHttpHeaders
                {
                    ContentType = file.ContentType // ensures browser can open the file
                };
                await blobClient.UploadAsync(stream, new Azure.Storage.Blobs.Models.BlobUploadOptions
                {
                    HttpHeaders = blobHttpHeaders,
                });
            }

            var url = blobClient.Uri.ToString();

            // 5️⃣ Save metadata in DB
            var document = new Document
            {
                DocumentName = file.FileName,
                DocumentUrl = url,
                ProjectId = projectId,
                UploadedBy = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId) ? userId : throw new UnauthorizedAccessException("User ID not found in token"), // replace with your user retrieval logic
                UploadedAt = DateTime.UtcNow
            };

            _context.Documents.Add(document);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "File uploaded successfully",
                url,
                documentId = document.Id
            });
        }

        [HttpGet("project/{projectId}")]
        public async Task<ActionResult<IEnumerable<DocumentDto>>> GetDocumentsByProject(int projectId)
        {
            var documents = await _context.Documents
                .Include(d => d.UploadedByNavigation)
                .Where(d => d.ProjectId == projectId && !d.IsDeleted)
                .Select(d => new DocumentDto
                {
                    Id = d.Id,
                    DocumentUrl = d.DocumentUrl,
                    DocumentName = d.DocumentName,
                    ProjectId = d.ProjectId,
                    UploadedById = d.UploadedBy,
                    UploadedByName = d.UploadedByNavigation.Name,
                    UploadedAt = d.UploadedAt
                })
                .ToListAsync();


            return Ok(documents);
        }

        [HttpDelete("delete/{documentId}")]
        public async Task<IActionResult> SoftDeleteDocument(int documentId)
        {
            // Find the document by ID
            var document = await _context.Documents.FirstOrDefaultAsync(d => d.Id == documentId);

            if (document == null)
                return NotFound("Document not found.");

            if (document.IsDeleted)
                return BadRequest("Document is already deleted.");

            // Get current user ID from token
            var userId = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var parsedUserId)
                ? parsedUserId
                : throw new UnauthorizedAccessException("User ID not found in token.");

            // Mark as deleted
            document.IsDeleted = true;
            document.DeletedAt = DateTime.UtcNow;
            document.DeletedBy = userId;

            _context.Documents.Update(document);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Document marked as deleted successfully.",
                documentId = document.Id,
                deletedBy = document.DeletedBy,
                deletedAt = document.DeletedAt
            });
        }


    }
}
