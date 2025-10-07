using Azure.Storage.Blobs;
using MI.CRM.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MI.CRM.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentsController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _containerName;

        public DocumentsController(IConfiguration configuration)
        {
            _configuration = configuration;
            var connectionString = _configuration["AzureStorage:ConnectionString"];
            _containerName = _configuration["AzureStorage:ContainerName"];

            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentNullException(nameof(connectionString), "Azure Storage connection string is missing.");

            _blobServiceClient = new BlobServiceClient(connectionString);
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload([FromForm] IFormFile file, [FromForm] int projectId)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            await containerClient.CreateIfNotExistsAsync();

            var blobClient = containerClient.GetBlobClient(file.FileName);

            using (var stream = file.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, overwrite: true);
            }

            var url = blobClient.Uri.ToString();

            // TODO: Save url + metadata (projectId, userId, etc.) in DB here

            return Ok(new { message = "File uploaded successfully", url });
        }
    }
}
