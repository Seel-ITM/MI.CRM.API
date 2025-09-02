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
    public class ProjectsController : ControllerBase
    {
        private readonly MicrmContext _context;
        public ProjectsController(MicrmContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Route("GetAll")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var projects = await _context.Projects.AsNoTracking().Select(project => new ProjectDto
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
            }).ToListAsync();
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

        [HttpGet("ProjectBudgetEntriesByCategory/{projectId}/{categoryId}")]
        public async Task<IActionResult> GetProjectBudgetEntriesByCategory(int projectId, int categoryId)
        {
            if(projectId <= 0 || categoryId <= 0)
            {
                return BadRequest("Invalid project ID or category ID.");
            }

            var entries = await _context.ProjectBudgetEntries
                .Where(e => e.ProjectId == projectId && e.CategoryId == categoryId)
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

            return Ok(entries);
        }


        [HttpPost]
        [Route("Overview")]
        public async Task<IActionResult> GetProjectOverview([FromBody] OverviewRequestDto req)
        {
            OverviewResponseDto res = new OverviewResponseDto();

            if (req.ProjectId <= 0)
            {
                return BadRequest("Invalid project ID.");
            }

            var project = await _context.Projects
                .Include(p => p.Tasks)
                .FirstOrDefaultAsync(p => p.ProjectId == req.ProjectId);

            if (project == null) 
            {
                return NotFound("Project not found.");
            }
            
            res.ProjectId = project.ProjectId;
            res.ActiveTasks = project.Tasks.Count(t => t.StatusId != 3);
            res.UpcomingTasks = project.Tasks.Count(t => t.StartDate >= DateTime.Now && t.StartDate <= req.WeekEndDate);
            res.PendingTasks = project.Tasks.Count(t => t.StatusId == 1);

            if(project.TotalApprovedBudget > 0)
            {
                res.BudgetPercentageUsed = (int)(project.TotalDisbursedBudget.GetValueOrDefault() / project.TotalApprovedBudget.GetValueOrDefault() * 100);
            }
            else
            {
                res.BudgetPercentageUsed = 0;
            }

            res.ToDoTasks = project.Tasks.Count(t => t.StatusId == 1);
            res.RemainingTasks = project.Tasks.Count(t => t.StatusId != 3);
            res.CompletedTasks = project.Tasks.Count(t => t.StatusId == 3);

            res.ProgressPercentage = project.Tasks.Count == 0 ? 0 : (int)((double)res.CompletedTasks / project.Tasks.Count * 100);
            res.Notifications = project.Tasks
            .Where(t => t.StartDate >= DateTime.Now && t.StartDate <= req.WeekEndDate)
            .Select(t =>
            {
                DateTime startDate = (DateTime)t.StartDate;
                return new TaskNotificationDto
                {
                    Title = t.Title,
                    ScheduledDate = startDate,
                    TimeRemaining = (t.StartDate.Value.Date - DateTime.Now.Date).Days > 0
                                    ? $"{(t.StartDate.Value.Date - DateTime.Now.Date).Days} days"
                                    : "Due today"
                };
            })
            .ToList();


            return Ok(res);



        }

        [HttpPost("Create")]
        public async Task<IActionResult> CreateProject([FromBody] CreateProjectDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var subContractor = new SubContractor
            {
                Name = dto.SubcontractorDetails.SubcontractorName,
                Email = dto.SubcontractorDetails.Email,
            };

            _context.SubContractors.Add(subContractor);
            await _context.SaveChangesAsync(); // This will populate subContractor.Id

            var currentUserId = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId) ? userId : throw new UnauthorizedAccessException("User ID not found in token"); // Replace with actual logged-in user id if available;
            var subcontractorId = subContractor.SubContractorId;
            int? projectMangerId = _context.ProjectManagers.Where(pm => pm.UserId == currentUserId).Select(pm => pm.ProjectManagerId).FirstOrDefault();
            var project = new Project
            {
                AwardNumber = dto.ProjectDetails.AwardNumber,
                Title = dto.ProjectDetails.Title,
                Category = dto.ProjectDetails.Category,
                Agency = dto.ProjectDetails.Agency,
                Company = dto.ProjectDetails.Company,
                State = dto.ProjectDetails.State,
                ProjectManagerId = projectMangerId,
                SubContractorId = subcontractorId,
            };

            _context.Projects.Add(project);
            await _context.SaveChangesAsync(); // This will populate project.ProjectId

            // Create budget entries
            decimal totalApprovedBudget = 0;
            foreach (var entry in dto.ProjectBudgetInfo)
            {
                if (entry.ApprovedAmount <= 0)
                {
                    return BadRequest("Approved amount must be greater than zero.");
                }
                totalApprovedBudget += entry.ApprovedAmount;
                var approvedBudgetEntry = new ProjectBudgetEntry
                {
                    ProjectId = project.ProjectId,
                    AwardNumber = project.AwardNumber,
                    CategoryId = entry.CategoryId,
                    TypeId = 1,
                    Amount = entry.ApprovedAmount,
                };
                _context.ProjectBudgetEntries.Add(approvedBudgetEntry);

                var disbursedBudgetEntry = new ProjectBudgetEntry
                {
                    ProjectId = project.ProjectId,
                    AwardNumber = project.AwardNumber,
                    CategoryId = entry.CategoryId,
                    TypeId = 2,
                    Amount = 0, // Initially zero
                };
                _context.ProjectBudgetEntries.Add(disbursedBudgetEntry);
                var remainingBudgetEntry = new ProjectBudgetEntry
                {
                    ProjectId = project.ProjectId,
                    AwardNumber = project.AwardNumber,
                    CategoryId = entry.CategoryId,
                    TypeId = 3,
                    Amount = entry.ApprovedAmount, // Initially equal to approved amount
                };
                _context.ProjectBudgetEntries.Add(remainingBudgetEntry);
            }

            project.TotalApprovedBudget = totalApprovedBudget;
            project.TotalDisbursedBudget = 0;
            project.TotalRemainingBudget = totalApprovedBudget;

            _context.Projects.Update(project);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Project created successfully",
                projectId = project.ProjectId
            });
        }




    }
}