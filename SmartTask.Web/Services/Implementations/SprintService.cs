using SmartTask.Web.Infrastructure.Interfaces;
using SmartTask.Web.Models.Entities;
using SmartTask.Web.Services.Interfaces;

namespace SmartTask.Web.Services.Implementations
{
    public class SprintService
        : BaseService<Sprint>, ISprintService
    {
        public SprintService(
            IGenericRepository<Sprint> repository,
            IUnitOfWork unitOfWork)
            : base(repository, unitOfWork)
        {
        }
    }
}