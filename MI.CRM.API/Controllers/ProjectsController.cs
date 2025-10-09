using MI.CRM.API.Dtos;
using MI.CRM.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

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
                SubContractorName = project.SubContractor != null ? project.SubContractor.Name : string.Empty,
                TotalApprovedBudget = project.TotalApprovedBudget,
                TotalDisbursedBudget = project.TotalDisbursedBudget,
                TotalRemainingBudget = project.TotalRemainingBudget,
                Status = project.Status,
                StartDate = project.StartDate,
                EndDate = project.EndDate,
                BilledNotPaid = project.BilledNotPaid,
                ProjectStatus = project.ProjectStatus
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
                SubContractorName = project.SubContractor != null ? project.SubContractor.Name : string.Empty,
                TotalApprovedBudget = project.TotalApprovedBudget,
                TotalDisbursedBudget = project.TotalDisbursedBudget,
                TotalRemainingBudget = project.TotalRemainingBudget,
                Status = project.Status,
                StartDate = project.StartDate,
                EndDate = project.EndDate,
                BilledNotPaid = project.BilledNotPaid,
                ProjectStatus = project.ProjectStatus
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
                SubContractorName = project.SubContractor != null ? project.SubContractor.Name : string.Empty,
                TotalApprovedBudget = project.TotalApprovedBudget,
                TotalDisbursedBudget = project.TotalDisbursedBudget,
                TotalRemainingBudget = project.TotalRemainingBudget,
                Status = project.Status,
                StartDate = project.StartDate,
                EndDate = project.EndDate,
                BilledNotPaid = project.BilledNotPaid,
                ProjectStatus = project.ProjectStatus

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

            var project = await _context.Projects.AsNoTracking()
                                                .Include(p => p.Tasks)
                                                .Include(p => p.ProjectManager)
                                                    .ThenInclude(pm => pm.User)
                                                .Include(p => p.SubContractor)
                                                .FirstOrDefaultAsync(p => p.ProjectId == req.ProjectId);

            if (project == null) 
            {
                return NotFound("Project not found.");
            }
            
            res.Project = new ProjectDto {
                ProjectId = project.ProjectId,
                AwardNumber = project.AwardNumber,
                Title = project.Title,
                Category = project.Category,
                Agency = project.Agency,
                Company = project.Company,
                State = project.State,
                ProjectManagerId = project.ProjectManagerId,
                SubContractorId = project.SubContractorId,
                SubContractorName = project.SubContractor != null ? project.SubContractor.Name : string.Empty,
                ProjectManagerName = project?.ProjectManager?.User?.Name ?? string.Empty,
                TotalApprovedBudget = project?.TotalApprovedBudget,
                TotalDisbursedBudget = project?.TotalDisbursedBudget,
                TotalRemainingBudget = project?.TotalRemainingBudget,
                Status = project?.Status,
                StartDate = project?.StartDate,
                EndDate = project?.EndDate,
                BilledNotPaid = project?.BilledNotPaid,
                ProjectStatus = project?.ProjectStatus

            };
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
                ProjectStatus = dto.ProjectDetails.ProjectStatus,
                StartDate = dto.ProjectDetails.StartDate,
                EndDate = dto.ProjectDetails.EndDate
            };

            _context.Projects.Add(project);
            await _context.SaveChangesAsync(); // This will populate project.ProjectId

            _context.ProjectSubcontractorMappings.Add(new ProjectSubcontractorMapping
            {
                ProjectId = project.ProjectId,
                SubcontractorId = subcontractorId
            });

            await _context.SaveChangesAsync();


            // Create budget entries
            decimal totalApprovedBudget = 0;
            foreach (var entry in dto.ProjectBudgetInfo)
            {
                if (entry.ApprovedAmount < 0)
                {
                    return BadRequest("Approved amount must be greater than or equal to zero.");
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

        [HttpGet("OperationsSummary/{projectId}")]
        public async Task<IActionResult> GetOperationsSummary(int projectId)
        {
            var project = await _context.Projects
                .Where(p => p.ProjectId == projectId)
                .Select(p => new ProjectDto
                {
                    ProjectId = p.ProjectId,
                    AwardNumber = p.AwardNumber,
                    Title = p.Title,
                    Category = p.Category,
                    Agency = p.Agency,
                    Company = p.Company,
                    State = p.State,
                    ProjectManagerId = p.ProjectManagerId,
                    SubContractorId = p.SubContractorId,
                    SubContractorName = p.SubContractor != null ? p.SubContractor.Name : string.Empty,
                    TotalApprovedBudget = p.TotalApprovedBudget,
                    TotalDisbursedBudget = p.TotalDisbursedBudget,
                    TotalRemainingBudget = p.TotalRemainingBudget,
                    BilledNotPaid = p.BilledNotPaid,
                    Status = p.Status,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    ProjectStatus = p.ProjectStatus
                })
                .FirstOrDefaultAsync();

            if (project == null) return NotFound();

            var allTasks = await _context.Tasks
                .Where(t => t.ProjectId == projectId)
                .Select(t => new TaskDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    StartDate = t.StartDate,
                    EndDate = t.EndDate,
                    AssignedTo = t.AssignedTo,
                    AssigneeName = t.AssignedToNavigation != null ? t.AssignedToNavigation.Name : string.Empty,
                    StatusId = t.StatusId,
                    StatusName = t.Status.Name,
                    StatusColor = t.Status.Color,
                    ActivityTypeId = t.ActivityTypeId,
                    ActivityTypeName = t.ActivityType.Name,
                    DeliverableType = t.DeliverableType,
                    CompletedOn = t.CompletedOn,
                    CompletedByName = t.CompletedByNavigation != null ? t.CompletedByNavigation.Name : string.Empty,
                })
                .ToListAsync();

            var totalTasks = allTasks.Count;
            var completedTasks = allTasks.Count(t => t.StatusId == 3);

            var deliverableTypes = allTasks
                .Where(t => !string.IsNullOrEmpty(t.DeliverableType))
                .Select(t => t.DeliverableType!)
                .Distinct()
                .ToList();

            var latestTodoTasks = allTasks
                .Where(t => t.CreatedOn >= DateTime.UtcNow.AddDays(-7))
                .OrderByDescending(t => t.CreatedOn)
                .ToList();

            var latestFinishedTasks = allTasks
                .Where(t => t.StatusId == 3 && t.CompletedOn >= DateTime.UtcNow.AddDays(-7))
                .OrderByDescending(t => t.CompletedOn)
                .ToList();

            var dto = new OperationsSummaryDto
            {
                Project = project,
                TotalNumberOfEvents = totalTasks,
                BudgetedNumberOfEvents = completedTasks,
                RemainingNumberOfEvents = totalTasks - completedTasks,
                DeliverableTypes = deliverableTypes,
                TasksCompletedPercent = totalTasks == 0 ? 0 : (decimal)completedTasks / totalTasks * 100,
                LatestTodoTasks = latestTodoTasks,
                LatestFinishedTasks = latestFinishedTasks,
                Tasks = allTasks
            };

            return Ok(dto);
        }

        [HttpGet("MainPageData")]
        public async Task<IActionResult> GetMainPageData()
        {
            // Get all projects
            var projectsQuery = _context.Projects;

            var projects = await projectsQuery
                .Select(p => new ProjectDto
                {
                    ProjectId = p.ProjectId,
                    AwardNumber = p.AwardNumber,
                    Title = p.Title,
                    Category = p.Category,
                    Agency = p.Agency,
                    Company = p.Company,
                    State = p.State,
                    ProjectManagerId = p.ProjectManagerId,
                    SubContractorId = p.SubContractorId,
                    SubContractorName = p.SubContractor != null ? p.SubContractor.Name : string.Empty,
                    TotalApprovedBudget = p.TotalApprovedBudget,
                    TotalDisbursedBudget = p.TotalDisbursedBudget,
                    TotalRemainingBudget = p.TotalRemainingBudget,
                    BilledNotPaid = p.BilledNotPaid,
                    Status = p.Status,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    ProjectStatus = p.ProjectStatus
                })
                .ToListAsync();

            var dto = new MainPageDataDto
            {
                Projects = projects,
                TotalProjects = projects.Count,
                States = projects.Select(p => p.State).Distinct().Count(),   // count of unique states
                TotalApprovedBudget = projects.Sum(p => p.TotalApprovedBudget ?? 0)
            };

            return Ok(dto);
        }


        [HttpPut("UpdateProjectStatusDescription")]
        public async Task<IActionResult> UpdateProjectStatusDescription([FromBody] UpdateProjectStatusDescriptionDto dto)
        {
            if (dto == null)
                return BadRequest("Empty DTO sent.");

            try
            {
                var project = await _context.Projects.FindAsync(dto.ProjectId);

                if (project == null)
                    return NotFound($"Project with ID {dto.ProjectId} not found.");

                if (string.IsNullOrWhiteSpace(dto.Status))
                    return BadRequest("Status cannot be empty.");

                project.Status = dto.Status;

                _context.Projects.Update(project);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Project status updated successfully.", project });
            }
            catch (DbUpdateException dbEx)
            {
                // Database-related issues (constraint, connection, etc.)
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "Database update failed.", error = dbEx.Message });
            }
            catch (Exception ex)
            {
                // Catch-all for unexpected issues
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An unexpected error occurred.", error = ex.Message });
            }
        }

        [HttpPut("OverviewEdit")]
        public async Task<IActionResult> OverviewEdit([FromBody] OverviewEditDto dto)
        {
            var project = await _context.Projects.FindAsync(dto.ProjectId);

            if (project is null)
            {
                return NotFound(new { message = "Project not found" });
            }

            switch (dto.Field)
            {
                case "title":
                    if (dto.Value.ValueKind == JsonValueKind.String)
                    {
                        string newTitle = dto.Value.GetString() ?? string.Empty;
                        await _context.Projects
                            .Where(p => p.ProjectId == dto.ProjectId)
                            .ExecuteUpdateAsync(p => p.SetProperty(x => x.Title, newTitle));
                    }
                    break;

                case "awardNumber":
                    if (dto.Value.ValueKind == JsonValueKind.String)
                    {
                        string strAward = dto.Value.GetString() ?? string.Empty;

                        // update child first
                        await _context.ProjectBudgetEntries
                            .Where(pbe => pbe.ProjectId == dto.ProjectId)
                            .ExecuteUpdateAsync(pbe => pbe.SetProperty(p => p.AwardNumber, strAward));

                        // then update parent
                        await _context.Projects
                            .Where(p => p.ProjectId == dto.ProjectId)
                            .ExecuteUpdateAsync(p => p.SetProperty(p => p.AwardNumber, strAward));
                    }
                    break;

                case "category":
                    if (dto.Value.ValueKind == JsonValueKind.String)
                    {
                        string newCategory = dto.Value.GetString() ?? string.Empty;
                        await _context.Projects
                            .Where(p => p.ProjectId == dto.ProjectId)
                            .ExecuteUpdateAsync(p => p.SetProperty(x => x.Category, newCategory));
                    }
                    break;

                case "agency":
                    if (dto.Value.ValueKind == JsonValueKind.String)
                    {
                        string newAgency = dto.Value.GetString() ?? string.Empty;
                        await _context.Projects
                            .Where(p => p.ProjectId == dto.ProjectId)
                            .ExecuteUpdateAsync(p => p.SetProperty(x => x.Agency, newAgency));
                    }
                    break;

                case "company":
                    if (dto.Value.ValueKind == JsonValueKind.String)
                    {
                        string newCompany = dto.Value.GetString() ?? string.Empty;
                        await _context.Projects
                            .Where(p => p.ProjectId == dto.ProjectId)
                            .ExecuteUpdateAsync(p => p.SetProperty(x => x.Company, newCompany));
                    }
                    break;

                case "state":
                    if (dto.Value.ValueKind == JsonValueKind.String)
                    {
                        string newState = dto.Value.GetString() ?? string.Empty;
                        await _context.Projects
                            .Where(p => p.ProjectId == dto.ProjectId)
                            .ExecuteUpdateAsync(p => p.SetProperty(x => x.State, newState));
                    }
                    break;

                case "status":
                    if (dto.Value.ValueKind == JsonValueKind.String)
                    {
                        string newStatus = dto.Value.GetString() ?? string.Empty;
                        await _context.Projects
                            .Where(p => p.ProjectId == dto.ProjectId)
                            .ExecuteUpdateAsync(p => p.SetProperty(x => x.Status, newStatus));
                    }
                    break;
                case "startDate":
                    if (dto.Value.ValueKind == JsonValueKind.String &&
                        DateTime.TryParse(dto.Value.GetString(), out var startDate))
                    {
                        await _context.Projects
                            .Where(p => p.ProjectId == dto.ProjectId)
                            .ExecuteUpdateAsync(p => p.SetProperty(x => x.StartDate, startDate));
                    }
                    break;

                case "endDate":
                    if (dto.Value.ValueKind == JsonValueKind.String &&
                        DateTime.TryParse(dto.Value.GetString(), out var endDate))
                    {
                        await _context.Projects
                            .Where(p => p.ProjectId == dto.ProjectId)
                            .ExecuteUpdateAsync(p => p.SetProperty(x => x.EndDate, endDate));
                    }
                    break;
                case "projectStatus":
                    if (dto.Value.ValueKind == JsonValueKind.String)
                    {
                        string status = dto.Value.GetString() ?? string.Empty;

                        await _context.Projects
                            .Where(p => p.ProjectId == dto.ProjectId)
                            .ExecuteUpdateAsync(p => p.SetProperty(x => x.ProjectStatus, status));
                    }
                    break;


                default:
                    return BadRequest(new { message = $"Unknown field: {dto.Field}" });
            }

            return Ok(new { message = "Done" });
        }

        [HttpPost("AddSubcontractor")]
        public async Task<IActionResult> AddSubcontractor([FromBody] NewSubcontractorDto dto)
        {
            SubContractor subContractor = new SubContractor
            {
                Name = dto.Name,
                Email = dto.Email
            };

            await _context.SubContractors.AddAsync(subContractor);

            await _context.SaveChangesAsync();

            await _context.ProjectSubcontractorMappings.AddAsync(new ProjectSubcontractorMapping
            {
                ProjectId = dto.ProjectId,
                SubcontractorId = subContractor.SubContractorId
            });

            await _context.SaveChangesAsync();

            return Ok(new {message = "Subcontractor added successfully", projectId = dto.ProjectId, subcontractorId = subContractor.SubContractorId});

        }

    }
}