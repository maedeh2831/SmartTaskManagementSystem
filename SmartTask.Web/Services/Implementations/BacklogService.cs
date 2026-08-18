using Microsoft.EntityFrameworkCore;
using SmartTask.Web.Data.Context;
using SmartTask.Web.Infrastructure.Interfaces;
using SmartTask.Web.Models.Entities;
using SmartTask.Web.Services.Interfaces;

namespace SmartTask.Web.Services.Implementations;

public class BacklogService : BaseService<Backlog>, IBacklogService
{
    private readonly ApplicationDbContext _context;
    private const string DefaultBacklogName = "Product Backlog";

    public BacklogService(
        IGenericRepository<Backlog> repository,
        IUnitOfWork unitOfWork,
        ApplicationDbContext context)
        : base(repository, unitOfWork)
    {
        _context = context;
    }

    public async Task<Backlog> GetOrCreateAsync(int projectId)
    {
        var backlog = await _context.Backlogs
            .FirstOrDefaultAsync(x => x.ProjectId == projectId && x.ViewState);

        if (backlog != null)
            return backlog;

        backlog = new Backlog
        {
            ProjectId = projectId,
            Name = DefaultBacklogName,
            CreatedDate = DateTime.Now,
            ViewState = true
        };

        await _context.Backlogs.AddAsync(backlog);
        await _context.SaveChangesAsync();

        return backlog;
    }
}
