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
                ClaimNumber = dto.ClaimNumber,
                CreatedOn = DateTime.UtcNow,
                UserId = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId) ? userId : throw new UnauthorizedAccessException("User ID not found in token"),
                Units = dto.Units,
                Rate = dto.Rate
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
                DisbursementLogId = d.Id,
                ProjectId = d.ProjectId,
                CategoryId = d.CategoryId,
                Description = d.Description,
                DisbursementDate = d.DisbursementDate,
                DisbursedAmount = d.DisbursedAmount,
                Rate = d.Rate,
                Units = d.Units,
                DocumentId = d.DocumentId,
            }).ToListAsync();
            return Ok(disbursements);
        }

        [HttpGet("claims/{projectId}")]
        public async Task<IActionResult> GetClaimsByProjectId(int projectId)
        {
            var claims = await _context.DisbursementLogs.AsNoTracking()
                .Where(d => d.ProjectId == projectId)
                .Select(d => new ClaimDto
                {
                    ProjectId = d.ProjectId,
                    ClaimNumber = d.ClaimNumber ?? 0, // since it's nullable in entity
                    DisbursedAmount = d.DisbursedAmount,
                    Description = d.Description ?? string.Empty,
                    CategoryId = d.CategoryId,
                    CategoryName = d.Category.Name, // navigation property
                    DisbursementDate = d.DisbursementDate
                })
                .ToListAsync();

            if (!claims.Any())
                return NotFound(new { Message = "No claims found for this project." });

            return Ok(claims);
        }

        [HttpGet("ClaimNumbers/{projectId}")]
        public async Task<IActionResult> GetClaimNumbersByProjectId(int projectId)
        {
            var claimNumbers = await _context.DisbursementLogs.AsNoTracking()
                .Where(d => d.ProjectId == projectId && d.ClaimNumber.HasValue) // filter only non-null claim numbers
                .Select(d => d.ClaimNumber.Value) // get the int value
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

           

            return Ok(claimNumbers);
        }

        [HttpGet("DisbursementLog/{id}")]
        public async Task<IActionResult> GetDisbursementLogById(int id)
        {
            if (id < 0)
                return BadRequest("id must be greater than 0.");

            var disbursement = await _context.DisbursementLogs
                .AsNoTracking()
                .Where(d => d.Id == id)
                .Select(d => new DisbursementDto
                {
                    DisbursementLogId = d.Id,
                    ProjectId = d.ProjectId,
                    CategoryId = d.CategoryId,
                    Description = d.Description,
                    DisbursementDate = d.DisbursementDate,
                    DisbursedAmount = d.DisbursedAmount,
                    Rate = d.Rate,
                    Units = d.Units
                })
                .FirstOrDefaultAsync();

            if (disbursement == null)
                return NotFound("No record found for the given id.");

            return Ok(disbursement);
        }

        [HttpPut("update-disbursement")]
        public async Task<IActionResult> UpdateDisbursement([FromBody] DisbursementDto dto)
        {
            if (dto == null) return BadRequest("Invalid data.");
            if (dto.ProjectId < 1 || dto.CategoryId < 1 || dto.DisbursementLogId < 1)
                return BadRequest("Missing or invalid IDs.");

            var disbursementEntry = await _context.DisbursementLogs.FindAsync(dto.DisbursementLogId);
            if (disbursementEntry == null) return NotFound("Disbursement not found.");

            var projectEntry = await _context.Projects.FindAsync(dto.ProjectId);
            if (projectEntry == null) return NotFound("Project not found.");

            var projectBudgetEntriesForCategory = await _context.ProjectBudgetEntries
                .Where(pbe => pbe.ProjectId == dto.ProjectId && pbe.CategoryId == dto.CategoryId)
                .ToListAsync();

            if (projectBudgetEntriesForCategory.Count < 3)
                return BadRequest("Incomplete budget entries for category.");

            var disbursedEntry = projectBudgetEntriesForCategory.FirstOrDefault(x => x.TypeId == 2);
            var remainingEntry = projectBudgetEntriesForCategory.FirstOrDefault(x => x.TypeId == 3);

            if (disbursedEntry == null || remainingEntry == null)
                return BadRequest("Missing expected budget entry types.");

            // Step 1: Revert old
            projectEntry.TotalDisbursedBudget -= disbursementEntry.DisbursedAmount;
            projectEntry.TotalRemainingBudget += disbursementEntry.DisbursedAmount;

            disbursedEntry.Amount -= disbursementEntry.DisbursedAmount;
            remainingEntry.Amount += disbursementEntry.DisbursedAmount;

            // Step 2: Apply new
            projectEntry.TotalDisbursedBudget += dto.DisbursedAmount;
            projectEntry.TotalRemainingBudget -= dto.DisbursedAmount;

            disbursedEntry.Amount += dto.DisbursedAmount;
            remainingEntry.Amount -= dto.DisbursedAmount;

            // Safety check
            if (remainingEntry.Amount < 0 || projectEntry.TotalRemainingBudget < 0)
                return BadRequest("Disbursement exceeds remaining budget.");

            // Update the disbursement record
            disbursementEntry.Description = dto.Description;
            disbursementEntry.DisbursementDate = dto.DisbursementDate;
            disbursementEntry.DisbursedAmount = dto.DisbursedAmount;
            disbursementEntry.ClaimNumber = dto.ClaimNumber;
            disbursementEntry.DocumentId = dto.DocumentId;
            disbursementEntry.Units = dto.Units;
            disbursementEntry.Rate = dto.Rate;

            await _context.SaveChangesAsync();

            // ✅ Return a lightweight DTO instead of the EF entity
            var updatedDto = new DisbursementDto
            {
                DisbursementLogId = disbursementEntry.Id,
                ProjectId = disbursementEntry.ProjectId,
                CategoryId = dto.CategoryId,
                Description = disbursementEntry.Description,
                DisbursementDate = disbursementEntry.DisbursementDate,
                DisbursedAmount = disbursementEntry.DisbursedAmount,
                ClaimNumber = disbursementEntry.ClaimNumber,
                DocumentId = disbursementEntry.DocumentId,
                Units = disbursementEntry.Units,
                Rate = disbursementEntry.Rate
            };

            return Ok(new
            {
                Message = "Disbursement updated successfully.",
                UpdatedDisbursement = updatedDto
            });
        }

        [HttpDelete("delete-disbursement/{disbursementLogId:int}")]
        public async Task<IActionResult> DeleteDisbursement(int disbursementLogId)
        {
            if (disbursementLogId < 1)
                return BadRequest("Invalid disbursement ID.");

            // 🔹 Find disbursement entry
            var disbursementEntry = await _context.DisbursementLogs.FindAsync(disbursementLogId);
            if (disbursementEntry == null)
                return NotFound("Disbursement not found.");

            // 🔹 Validate related project
            var projectEntry = await _context.Projects.FindAsync(disbursementEntry.ProjectId);
            if (projectEntry == null)
                return NotFound("Related project not found.");

            // 🔹 Get category from disbursement
            var categoryId = disbursementEntry.CategoryId;

            // 🔹 Fetch related budget entries for that category
            var projectBudgetEntriesForCategory = await _context.ProjectBudgetEntries
                .Where(pbe => pbe.ProjectId == disbursementEntry.ProjectId && pbe.CategoryId == categoryId)
                .ToListAsync();

            if (projectBudgetEntriesForCategory.Count != 3)
                return BadRequest("Improper budget entries for category.");

            var disbursedEntry = projectBudgetEntriesForCategory.FirstOrDefault(x => x.TypeId == 2);
            var remainingEntry = projectBudgetEntriesForCategory.FirstOrDefault(x => x.TypeId == 3);

            if (disbursedEntry == null || remainingEntry == null)
                return BadRequest("Missing expected budget entry types.");

            // 🔹 Undo disbursement effects
            projectEntry.TotalDisbursedBudget -= disbursementEntry.DisbursedAmount;
            projectEntry.TotalRemainingBudget += disbursementEntry.DisbursedAmount;

            disbursedEntry.Amount -= disbursementEntry.DisbursedAmount;
            remainingEntry.Amount += disbursementEntry.DisbursedAmount;

            if (projectEntry.TotalDisbursedBudget < 0) projectEntry.TotalDisbursedBudget = 0;
            if (remainingEntry.Amount < 0) remainingEntry.Amount = 0;

            // 🔹 Remove the disbursement record
            _context.DisbursementLogs.Remove(disbursementEntry);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Disbursement deleted successfully.",
                DeletedDisbursementId = disbursementLogId
            });
        }




    }
}
