using MI.CRM.API.Dtos;
using MI.CRM.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MI.CRM.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class StakeHoldersController : ControllerBase
    {

        private readonly MicrmContext _context;
        public StakeHoldersController(MicrmContext context)
        {
            _context = context;
        }
        
        
        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAllStakeHolders()
        {
            var projectManagers = await _context.ProjectManagers
                .Include(pm => pm.User)
                .ToListAsync();

            var subcontractors = await _context.SubContractors.ToListAsync();

            var stakeholders = new List<StakeHolderDto>();
            
            foreach (var pm in projectManagers)
            {
                stakeholders.Add(new StakeHolderDto
                {
                    ProjectManageer = new ProjectMangerDto
                    {
                        Id = pm.ProjectManagerId,
                        UserId = pm.User?.UserId,
                        Name = pm.User?.Name,
                        Email = pm.User?.Email,
                    },
                });
            }

            foreach (var sc in subcontractors)
            {
                stakeholders.Add(new StakeHolderDto
                {
                    Subcontractor = new SubcontractorDto
                    {
                        Id = sc.SubContractorId,
                        Name = sc.Name,
                        Email = sc.Email,
                    }
                });
            }   

            return Ok(stakeholders);
        }

        [HttpGet("GetByProject/{projectId}")]
        public async Task<IActionResult> GetStakeHoldersByProject(int projectId)
        {
            var project = await _context.Projects
                .Include(p => p.ProjectManager)
                    .ThenInclude(pm => pm.User)
                .FirstOrDefaultAsync(p => p.ProjectId == projectId);

            if (project == null)
            {
                return NotFound(new { Message = "Project not found" });
            }

            var stakeholders = new List<StakeHolderDto>();

            // Add Project Manager if exists
            if (project.ProjectManager != null)
            {
                stakeholders.Add(new StakeHolderDto
                {
                    ProjectManageer = new ProjectMangerDto
                    {
                        Id = project.ProjectManager.ProjectManagerId,
                        UserId = project.ProjectManager.User?.UserId,
                        Name = project.ProjectManager.User?.Name,
                        Email = project.ProjectManager.User?.Email,
                    }
                });
            }

            // 🔹 Fetch all subcontractors via the mapping table
            var subcontractors = await _context.ProjectSubcontractorMappings
                .Where(m => m.ProjectId == projectId)
                .Include(m => m.Subcontractor)
                .Select(m => m.Subcontractor)
                .ToListAsync();

            foreach (var sub in subcontractors)
            {
                stakeholders.Add(new StakeHolderDto
                {
                    Subcontractor = new SubcontractorDto
                    {
                        Id = sub.SubContractorId,
                        Name = sub.Name,
                        Email = sub.Email
                    }
                });
            }

            return Ok(stakeholders);
        }


    }
}
