using MI.CRM.API.Dtos;
using MI.CRM.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MI.CRM.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DisbursementController : ControllerBase
    {
        private readonly MicrmContext _context;
        public DisbursementController(MicrmContext context)
        {
            _context = context;
        }
        [HttpPost("create")]
        public async Task<IActionResult> CreateDisbursement([FromBody] NewDisbursementDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var entityEntry = _context.DisbursementLogs.Add(new DisbursementLog
            {
                ProjectId = dto.ProjectId,
                CategoryId = dto.CategoryId,
                Description = dto.Description,
                DisbursementDate = dto.DisbursementDate,
                DisbursedAmount = dto.DisbursedAmount,
                DocumentId = dto.DocumentId,
                CreatedOn = DateTime.UtcNow,
                UserId = 3 // TODO: Replace with actual user ID from auth context
            });

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Disbursement created successfully",
                disbursementId = entityEntry.Entity.Id
            });
        }


        [HttpGet("get/{projectId}/{categoryId}")]
        public async Task<IActionResult> GetDisbursementsByProjectAndCategory(int projectId, int categoryId)
        {
            var disbursements = await _context.DisbursementLogs.AsNoTracking().Where(d => d.ProjectId == projectId && d.CategoryId == categoryId).Select(d => new DisbursementDto
            {
                ProjectId = d.ProjectId,
                CategoryId = d.CategoryId,
                Description = d.Description,
                DisbursementDate = d.DisbursementDate,
                DisbursedAmount = d.DisbursedAmount
            }).ToListAsync();
            return Ok(disbursements);
        }
    }
}
