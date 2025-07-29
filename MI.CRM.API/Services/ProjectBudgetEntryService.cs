using MI.CRM.API.Data;
using MI.CRM.API.Models;
using Microsoft.EntityFrameworkCore;

namespace MI.CRM.API.Services;

public class ProjectBudgetEntryService : IProjectBudgetEntryService
{
    private readonly ApplicationDbContext _context;

    public ProjectBudgetEntryService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProjectBudgetEntry>> GetFilteredByProjectAsync(int? projectId, int? categoryId, int? typeId)
    {
        var query = _context.ProjectBudgetEntries.AsNoTracking()
            .Where(p => p.ProjectId == projectId && (categoryId.HasValue ? p.CategoryId == categoryId:true) && (typeId.HasValue? p.TypeId==typeId:true))
            .Include(p => p.Category)
            .Include(p => p.Type)
            .AsNoTracking()
            .ToListAsync();

        //if (projectId.HasValue)
        //    query = query.Where(p => p.ProjectId == projectId.Value);

        //if (categoryId.HasValue)
        //    query = query.Where(p => p.CategoryId == categoryId.Value);

        //if (typeId.HasValue)
        //    query = query.Where(p => p.TypeId == typeId.Value);

        return await query;
    }

    public async Task<List<ProjectBudgetEntry>> GetFilteredByAwardNumberAsync(string? awardNumber, int? categoryId, int? typeId)
    {
        var query = _context.ProjectBudgetEntries.AsNoTracking()
             .Where(p => p.AwardNumber==awardNumber && (categoryId.HasValue ? p.CategoryId == categoryId : true) && (typeId.HasValue ? p.TypeId == typeId : true))

            .Include(p => p.Category )
            .Include(p => p.Type)
            .AsNoTracking()
            .AsQueryable().ToListAsync();

        //if (!string.IsNullOrEmpty(awardNumber))
        //    query = query.Where(p => p.AwardNumber == awardNumber);

        //if (categoryId.HasValue)
        //    query = query.Where(p => p.CategoryId == categoryId.Value);

        //if (typeId.HasValue)
        //    query = query.Where(p => p.TypeId == typeId.Value);

        return await query;
    }
}
