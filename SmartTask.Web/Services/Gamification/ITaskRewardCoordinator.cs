/*
| Module      : Gamification
| Interface   : ITaskRewardCoordinator
| Purpose     : هماهنگ‌کننده اعطای پاداش، تجربه و دستاورد پس از تکمیل تسک
*/

using SmartTask.Web.Models.Enums;

namespace SmartTask.Web.Services.Gamification
{
    public interface ITaskRewardCoordinator
    {
        /// <summary>
        /// اطمینان از وجود کیف پول و پیشرفت برای کاربر
        /// </summary>
        Task EnsureUserGamificationAsync(int userId);

        /// <summary>
        /// پردازش کامل پاداش تکمیل تسک برای تمام کاربران تخصیص‌داده‌شده
        /// </summary>
        Task HandleTaskCompletedAsync(
            int taskId,
            string taskTitle,
            IEnumerable<int> assigneeIds,
            TaskPriorityType priority,
            int estimate);

        /// <summary>
        /// پردازش پاداش و دستاورد تکمیل اسپرینت برای تمام مشارکت‌کنندگان
        /// </summary>
        Task HandleSprintCompletedAsync(int sprintId);

        /// <summary>
        /// پردازش پاداش و دستاورد تکمیل پروژه برای تمام اعضای پروژه
        /// </summary>
        Task HandleProjectCompletedAsync(int projectId);
    }
}
