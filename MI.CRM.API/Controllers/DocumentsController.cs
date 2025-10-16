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
        public async Task<IActionResult> Upload([FromForm] List<IFormFile> files, [FromForm] int projectId)
        {
            if (files == null || files.Count == 0)
                return BadRequest("No files uploaded.");

            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

            var allowedExtensions = new[] { ".pdf", ".docx", ".xlsx", ".png", ".jpg", ".jpeg", ".zip", ".rar", ".csv" };
            var uploadedDocs = new List<object>();

            foreach (var file in files)
            {
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(fileExtension))
                    continue; // skip disallowed file types

                var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
                var blobClient = containerClient.GetBlobClient(uniqueFileName);

                using var stream = file.OpenReadStream();
                await blobClient.UploadAsync(stream, new BlobUploadOptions
                {
                    HttpHeaders = new BlobHttpHeaders { ContentType = file.ContentType }
                });

                var url = blobClient.Uri.ToString();

                var document = new Document
                {
                    DocumentName = file.FileName,
                    DocumentUrl = url,
                    ProjectId = projectId,
                    UploadedBy = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0"),
                    UploadedAt = DateTime.UtcNow
                };

                _context.Documents.Add(document);
                uploadedDocs.Add(new { document.Id, document.DocumentName, url });
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = $"{uploadedDocs.Count} file(s) uploaded successfully.",
                documents = uploadedDocs
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
