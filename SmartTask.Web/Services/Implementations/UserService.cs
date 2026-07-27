using Microsoft.EntityFrameworkCore;
using SmartTask.Web.Data.Context;
using SmartTask.Web.Infrastructure.Interfaces;
using SmartTask.Web.Models.Entities;
using SmartTask.Web.Models.ViewModels.Workspace;
using SmartTask.Web.Services.Interfaces;
namespace SmartTask.Web.Services.Implementations
{
    public class UserService
        : BaseService<ApplicationUser>, IUserService
    {
        private readonly ApplicationDbContext _context;
        public UserService(
            IGenericRepository<ApplicationUser> repository,
            IUnitOfWork unitOfWork,
            ApplicationDbContext context)
            : base(repository, unitOfWork)
        {
            _context = context;
        }
        public async Task<List<UserSearchResultViewModel>> SearchUsersAsync(
            string term,
            int workspaceId)
        {
            if (string.IsNullOrWhiteSpace(term))
                return new List<UserSearchResultViewModel>();

            var memberUserIds = await _context.WorkspaceMembers
                .Where(x => x.WorkspaceId == workspaceId && x.ViewState)
                .Select(x => x.ApplicationUserId)
                .ToListAsync();

            return await _context.Users
                .Where(x =>
                    !memberUserIds.Contains(x.Id) &&
                    x.ViewState &&
                    (x.FirstName.Contains(term) ||
                     x.LastName.Contains(term) ||
                     (x.Email != null && x.Email.Contains(term))))
                .OrderBy(x => x.FirstName)
                .Take(10)
                .Select(x => new UserSearchResultViewModel
                {
                    Id = x.Id,
                    FullName = (x.FirstName + " " + x.LastName).Trim(),
                    Email = x.Email!,
                    Avatar = x.Avatar
                })
                .ToListAsync();
        }
    }
}