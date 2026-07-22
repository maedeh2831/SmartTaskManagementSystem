using SmartTask.Web.Infrastructure.Interfaces;
using SmartTask.Web.Models.Entities;
using SmartTask.Web.Services.Interfaces;

namespace SmartTask.Web.Services.Implementations
{
    public class UserStoryService
        : BaseService<UserStory>, IUserStoryService
    {
        public UserStoryService(
            IGenericRepository<UserStory> repository,
            IUnitOfWork unitOfWork)
            : base(repository, unitOfWork)
        {
        }
    }
}