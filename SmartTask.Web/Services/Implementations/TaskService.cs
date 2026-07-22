using SmartTask.Web.Infrastructure.Interfaces;
using SmartTask.Web.Models.Entities;
using SmartTask.Web.Services.Interfaces;

namespace SmartTask.Web.Services.Implementations
{
    public class TaskService
        : BaseService<TaskItem>, ITaskService
    {
        public TaskService(
            IGenericRepository<TaskItem> repository,
            IUnitOfWork unitOfWork)
            : base(repository, unitOfWork)
        {
        }
    }
}