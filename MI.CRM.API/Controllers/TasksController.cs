using MI.CRM.API.Dtos;
using MI.CRM.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using System.Security.Claims;

namespace MI.CRM.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TasksController : ControllerBase
    {
        private readonly MicrmContext _context;

        public TasksController(MicrmContext context)
        {
            _context = context;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateTask([FromBody] NewTaskDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            DateTime dateTime = DateTime.UtcNow;
            var currentUserId = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId) ? userId : throw new UnauthorizedAccessException("User ID not found in token"); // Replace with actual logged-in user id if available;

            // Create the new task
            var task = new Models.Task
            {
                ProjectId = dto.ProjectId,
                Title = dto.Title,
                Description = dto.Description,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                AssignedTo = dto.AssignedTo,
                StatusId = dto.StatusId,
                ActivityTypeId = dto.ActivityTypeId,
                DeliverableType = dto.DeliverableType,
                CreatedOn = dateTime,
                CreatedBy = currentUserId,
                CompletedBy = dto.StatusId == 3 ? currentUserId : null,
                CompletedOn = dto.StatusId == 3 ? dateTime : null
            };

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync(); // Save first to get the Task.Id

            // Log the creation
            var log = new TaskLog
            {
                TaskId = task.Id,
                UserId = currentUserId,
                ActionType = "Created",
                FieldChanged = null,
                OldValue = null,
                NewValue = null,
                ActionTimestamp = DateTime.UtcNow
            };

            _context.TaskLogs.Add(log);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Task created successfully", taskId = task.Id });
        }

        [HttpPut("update-status")]
        public async Task<IActionResult> UpdateTaskStatus([FromBody] UpdateTaskStatusDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var task = await _context.Tasks.FindAsync(dto.TaskId);
            if (task == null)
                return NotFound(new { message = "Task not found." });

            var currentUserId = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId) ? userId : throw new UnauthorizedAccessException("User ID not found in token"); // Replace with actual logged-in user id if available;

            var oldStatus = task.StatusId;
            task.StatusId = dto.StatusId;

            if(dto.StatusId == 3)
            {
                task.CompletedBy = currentUserId;
                task.CompletedOn = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();


            // Log the status change
            var log = new TaskLog
            {
                TaskId = task.Id,
                UserId = currentUserId,
                ActionType = "Status Updated",
                FieldChanged = "StatusId",  
                OldValue = oldStatus.ToString(),
                NewValue = dto.StatusId.ToString(),
                ActionTimestamp = DateTime.UtcNow
            };

            _context.TaskLogs.Add(log);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Task status updated successfully." });
        }

        [HttpPut("update-datetime")]
        public async Task<IActionResult> UpdateTaskDateTime([FromBody] UpdateTaskDateTimeDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var task = await _context.Tasks.FindAsync(dto.TaskId);
            if (task == null)
                return NotFound(new { message = "Task not found." });

            var logs = new List<TaskLog>();

            if (task.StartDate != dto.StartDateTime)
            {
                logs.Add(new TaskLog
                {
                    TaskId = task.Id,
                    UserId = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId) ? userId : throw new UnauthorizedAccessException("User ID not found in token"), // Replace with actual logged-in user id if available
                    ActionType = "DateTime Updated",
                    FieldChanged = "StartDate",
                    OldValue = task.StartDate.ToString(),
                    NewValue = dto.StartDateTime.ToString(),
                    ActionTimestamp = DateTime.UtcNow   
                });

                task.StartDate = dto.StartDateTime;
            }

            if (task.EndDate != dto.EndDateTime)
            {
                logs.Add(new TaskLog
                {
                    TaskId = task.Id,
                    UserId = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId) ? userId : throw new UnauthorizedAccessException("User ID not found in token"), // Replace with actual logged-in user id if available
                    ActionType = "DateTime Updated",
                    FieldChanged = "EndDate",
                    OldValue = task.EndDate.ToString(),
                    NewValue = dto.EndDateTime.ToString(),
                    ActionTimestamp = DateTime.UtcNow
                });

                task.EndDate = dto.EndDateTime;
            }


            if (logs.Any())
                _context.TaskLogs.AddRange(logs);

            await _context.SaveChangesAsync();

            return Ok(new { message = "Task datetime updated successfully." });
        }





        [HttpGet("ProjectTasks")]
        public async Task<IActionResult> GetTasks(
        [FromQuery] int projectId,
        [FromQuery] int? statusId = null,
        [FromQuery] string customFilter = "")
        {
            if (projectId <= 0)
                return BadRequest("Invalid projectId.");

            var tasksQuery = _context.Tasks
                .Where(t => t.ProjectId == projectId);

            // apply StatusId filter if explicitly passed
            if (statusId.HasValue)
            {
                tasksQuery = tasksQuery.Where(t => t.StatusId == statusId.Value);
            }

            // apply custom filter
            if (!string.IsNullOrEmpty(customFilter))
            {

                var today = DateTime.Today;
                var endOfWeek = today.AddDays(7 - (int)today.DayOfWeek); // Sunday

                switch (customFilter.ToUpper())
                {
                    case "DUE_TODAY":
                        tasksQuery = tasksQuery.Where(t =>
                            t.EndDate.HasValue &&
                            t.EndDate.Value.Date == today &&
                            t.StatusId != 3);
                        break;

                    case "DUE_THIS_WEEK":
                        tasksQuery = tasksQuery.Where(t =>
                            t.EndDate.HasValue &&
                            t.EndDate.Value.Date >= today &&
                            t.EndDate.Value.Date <= endOfWeek &&
                            t.StatusId != 3);
                        break;

                    case "OVERDUE":
                        tasksQuery = tasksQuery.Where(t =>
                            t.EndDate.HasValue &&
                            t.EndDate.Value.Date < today &&
                            t.StatusId != 3);
                        break;
                }
            }

            var tasks = await tasksQuery
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

            return Ok(tasks);
        }


    }
}
