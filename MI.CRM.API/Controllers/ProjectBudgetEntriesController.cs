using Microsoft.AspNetCore.Mvc;
using MI.CRM.API.Services;
using MI.CRM.API.Dtos;
using AutoMapper;

namespace MI.CRM.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProjectBudgetEntriesController : ControllerBase
{
    private readonly IProjectBudgetEntryService _service;
    private readonly IMapper _mapper;

    public ProjectBudgetEntriesController(IProjectBudgetEntryService service, IMapper mapper)
    {
        _service = service;
        _mapper = mapper;
    }

    [HttpGet("filter-by-project")]
    public async Task<IActionResult> GetFilteredByProject(
    [FromQuery] int? projectId,
    [FromQuery] int? categoryId,
    [FromQuery] int? typeId)
    {
        var entries = await _service.GetFilteredByProjectAsync(projectId, categoryId, typeId);
        var dto = _mapper.Map<IEnumerable<ProjectBudgetEntryDto>>(entries);
        return Ok(dto);
    }

    [HttpGet("filter-by-award")]
    public async Task<IActionResult> GetFilteredByAwardNumber(
        [FromQuery] string? awardNumber,
        [FromQuery] int? categoryId,
        [FromQuery] int? typeId)
    {
        var entries = await _service.GetFilteredByAwardNumberAsync(awardNumber, categoryId, typeId);
        var dto = _mapper.Map<IEnumerable<ProjectBudgetEntryDto>>(entries);
        return Ok(dto);
    }
}
