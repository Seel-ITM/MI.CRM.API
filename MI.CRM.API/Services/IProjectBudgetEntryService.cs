using MI.CRM.API.Models;

namespace MI.CRM.API.Services;

public interface IProjectBudgetEntryService
{
    Task<List<ProjectBudgetEntry>> GetFilteredByProjectAsync(int? projectId, int? categoryId, int? typeId);
    Task<List<ProjectBudgetEntry>> GetFilteredByAwardNumberAsync(string? awardNumber, int? categoryId, int? typeId);
}
