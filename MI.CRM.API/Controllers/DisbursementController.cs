using MI.CRM.API.Dtos;
using MI.CRM.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MI.CRM.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
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

            var project = await _context.Projects.FindAsync(dto.ProjectId);

            if (project == null)
                return NotFound(new { message = "Project not found" });

            var budgetEntries = await _context.ProjectBudgetEntries
                .Where(e => e.ProjectId == dto.ProjectId && e.CategoryId == dto.CategoryId)
                .OrderBy(e => e.CategoryId)
                .ThenBy(e => e.TypeId)
                .ToListAsync();

            if (budgetEntries.Count == 0)
                return NotFound(new { message = "No budget entries found for the specified project and category" });

            if(budgetEntries.Count != 3)
                return BadRequest(new { message = "There should be exactly 3 budget entries for a category"});

            // Validate disbursement amount against budget entries
            var categoryApprovedAmount = budgetEntries[0].Amount;
            var categoryDisbursedAmount = budgetEntries[1].Amount;
            var categoryRemainingAmount = budgetEntries[2].Amount;

            var newCategoryDisbursedAmount = categoryDisbursedAmount + dto.DisbursedAmount;
            if (newCategoryDisbursedAmount > categoryApprovedAmount)
            {
                return BadRequest(new { message = "Disbursement exceeds approved budget for the category" });
            }

            var newCategoryRemainingAmount = categoryApprovedAmount - newCategoryDisbursedAmount;

            // Update budget entries
            budgetEntries[1].Amount = newCategoryDisbursedAmount;
            budgetEntries[2].Amount = newCategoryRemainingAmount;



            decimal totalDisbursed = (project.TotalDisbursedBudget ?? 0) + dto.DisbursedAmount;
            decimal totalApproved = project.TotalApprovedBudget ?? 0;
            decimal totalRemaining = totalApproved - totalDisbursed;
            if (totalRemaining < 0)
            {
                return BadRequest(new { message = "Disbursement exceeds approved budget" });
            }

            // Update project budget
            project.TotalDisbursedBudget = totalDisbursed;
            project.TotalRemainingBudget = totalRemaining;

            // Update budget entry

            var entityEntry = _context.DisbursementLogs.Add(new DisbursementLog
            {
                ProjectId = dto.ProjectId,
                CategoryId = dto.CategoryId,
                Description = dto.Description,
                DisbursementDate = dto.DisbursementDate,
                DisbursedAmount = dto.DisbursedAmount,
                DocumentId = dto.DocumentId,
                CreatedOn = DateTime.UtcNow,
                UserId = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId) ? userId : throw new UnauthorizedAccessException("User ID not found in token")
                // TODO: Replace with actual user ID from auth context
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
