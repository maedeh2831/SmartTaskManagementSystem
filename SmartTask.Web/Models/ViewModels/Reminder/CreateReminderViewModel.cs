using System.ComponentModel.DataAnnotations;

namespace SmartTask.Web.Models.ViewModels.Reminder
{
    public class CreateReminderViewModel
    {
        [Required(ErrorMessage = "انتخاب Task الزامی است.")]
        [Display(Name = "Task")]
        public int TaskItemId { get; set; }

        [Required(ErrorMessage = "عنوان یادآوری الزامی است.")]
        [StringLength(250, ErrorMessage = "عنوان نباید بیشتر از 250 کاراکتر باشد.")]
        [Display(Name = "عنوان یادآوری")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "تاریخ و ساعت یادآوری الزامی است.")]
        [Display(Name = "تاریخ و ساعت یادآوری")]
        public DateTime ReminderDate { get; set; }

        public List<TaskOptionViewModel> AvailableTasks { get; set; } = new();
    }
}