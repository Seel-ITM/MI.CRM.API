using MI.CRM.API.Dtos;
using MI.CRM.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MI.CRM.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectsController : ControllerBase
    {
        private readonly MicrmContext _context;
        public ProjectsController(MicrmContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Route("GetAll")]
        public IActionResult GetAll()
        {
            var projects = _context.Projects.ToList();
            return Ok(projects);
        }

        [HttpGet]
        [Route("GetById/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null)
                return NotFound();

            var dto = new ProjectDto
            {
                ProjectId = project.ProjectId,
                AwardNumber = project.AwardNumber,
                Title = project.Title,
                Category = project.Category,
                Agency = project.Agency,
                Company = project.Company,
                State = project.State,
                ProjectManagerId = project.ProjectManagerId,
                SubContractorId = project.SubContractorId,
                TotalApprovedBudget = project.TotalApprovedBudget,
                TotalDisbursedBudget = project.TotalDisbursedBudget,
                TotalRemainingBudget = project.TotalRemainingBudget
            };

            return Ok(dto);
        }

        [HttpGet]
        [Route("GetByAwardNumber/{awardNumber}")]
        public async Task<IActionResult> GetByAwardNumber(string awardNumber)
        {
            var project = await _context.Projects.FirstOrDefaultAsync(p => p.AwardNumber == awardNumber);
            if (project == null)
                return NotFound();
            var dto = new ProjectDto
            {
                ProjectId = project.ProjectId,
                AwardNumber = project.AwardNumber,
                Title = project.Title,
                Category = project.Category,
                Agency = project.Agency,
                Company = project.Company,
                State = project.State,
                ProjectManagerId = project.ProjectManagerId,
                SubContractorId = project.SubContractorId,
                TotalApprovedBudget = project.TotalApprovedBudget,
                TotalDisbursedBudget = project.TotalDisbursedBudget,
                TotalRemainingBudget = project.TotalRemainingBudget
            };
            return Ok(dto);
        }

        [HttpGet]
        [Route("GetBudgetEntriesByProjectId/{id}")]
        public async Task<IActionResult> GetProjectBudgetEntriesById(int id)
        {
            var entries = await _context.ProjectBudgetEntries
                .Where(e => e.ProjectId == id)
                .Include(e => e.Category)
                .Include(e => e.Type)
                .OrderBy(e => e.CategoryId)
                .ThenBy(e => e.TypeId)
                .Select(e => new ProjectBudgetEntryDto
                {
                    Id = e.Id,
                    ProjectId = e.ProjectId,
                    AwardNumber = e.AwardNumber,
                    CategoryId = e.CategoryId,
                    CategoryName = e.Category.Name,
                    TypeId = e.TypeId,
                    TypeName = e.Type.Name,
                    Amount = e.Amount,
                    Notes = e.Notes
                })
                .ToListAsync();

            if (entries == null || entries.Count == 0)
                return NotFound("No budget entries found for the given project ID.");

            return Ok(entries);
        }


    }
}