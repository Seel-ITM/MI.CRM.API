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
                .Include(p => p.SubContractor)
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

            // Add SubContractor if exists
            if (project.SubContractor != null)
            {
                stakeholders.Add(new StakeHolderDto
                {
                    Subcontractor = new SubcontractorDto
                    {
                        Id = project.SubContractor.SubContractorId,
                        Name = project.SubContractor.Name,
                        Email = project.SubContractor.Email,
                    }
                });
            }

            return Ok(stakeholders);
        }

    }
}
