namespace SmartTask.Web.Models.ViewModels.Activity
{
    public class ActivityItemViewModel
    {
        public int Id { get; set; }
        public string Action { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime ActivityDate { get; set; }
        public int? TaskItemId { get; set; }
        public string? TaskTitle { get; set; }

        public string IconClass => Action switch
        {
            "ایجاد Task" => "fa-solid fa-plus",
            "ویرایش Task" => "fa-solid fa-pen",
            "حذف Task" => "fa-solid fa-trash",
            "تغییر وضعیت Task" => "fa-solid fa-arrows-rotate",
            "تخصیص Task" => "fa-solid fa-user-check",
            "حذف تخصیص Task" => "fa-solid fa-user-xmark",
            "ثبت نظر" => "fa-solid fa-comment",
            "ویرایش نظر" => "fa-solid fa-comment-dots",
            "حذف نظر" => "fa-solid fa-comment-slash",
            "شروع تایمر" => "fa-solid fa-play",
            "توقف تایمر" => "fa-solid fa-stop",
            "ثبت زمان دستی" => "fa-solid fa-clock",
            "حذف زمان ثبت‌شده" => "fa-solid fa-trash",
            _ => "fa-solid fa-circle-info"
        };
    }
}