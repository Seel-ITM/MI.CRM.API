using MI.CRM.API.Dtos;
using MI.CRM.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MI.CRM.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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
                ActivityTypeId = dto.ActivityTypeId
            };

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync(); // Save first to get the Task.Id

            // For now, assume the UserId is 1 (hardcoded) — you can replace this with actual user from auth context
            var userId = 3;

            // Log the creation
            var log = new TaskLog
            {
                TaskId = task.Id,
                UserId = userId,
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

            var oldStatus = task.StatusId;
            task.StatusId = dto.StatusId;

            await _context.SaveChangesAsync();

            // Log the status change
            var log = new TaskLog
            {
                TaskId = task.Id,
                UserId = 3,
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
                    UserId = 3, // Replace with actual logged-in user id if available
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
                    UserId = 3, // Replace with actual logged-in user id if available
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
        public async Task<IActionResult> GetTasks([FromQuery] int projectId, [FromQuery] int? statusId = null)
        {
            if (projectId <= 0)
                return BadRequest("Invalid projectId.");

            var tasksQuery = _context.Tasks
                .Where(t => t.ProjectId == projectId);

            if (statusId.HasValue)
            {
                tasksQuery = tasksQuery.Where(t => t.StatusId == statusId.Value);
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
                            StatusId = t.StatusId,
                            StatusName = t.Status.Name,
                            StatusColor = t.Status.Color,
                            ActivityTypeId = t.ActivityTypeId,
                            ActivityTypeName = t.ActivityType.Name
                        })
                        .ToListAsync();


            return Ok(tasks);
        }

    }
}
