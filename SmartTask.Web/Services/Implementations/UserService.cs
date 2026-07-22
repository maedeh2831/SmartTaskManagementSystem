using SmartTask.Web.Infrastructure.Interfaces;
using SmartTask.Web.Models.Entities;
using SmartTask.Web.Services.Interfaces;

namespace SmartTask.Web.Services.Implementations
{
    public class UserService
        : BaseService<ApplicationUser>, IUserService
    {
        public UserService(
            IGenericRepository<ApplicationUser> repository,
            IUnitOfWork unitOfWork)
            : base(repository, unitOfWork)
        {
        }
    }
}