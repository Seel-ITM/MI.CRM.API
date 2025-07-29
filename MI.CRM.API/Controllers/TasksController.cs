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
