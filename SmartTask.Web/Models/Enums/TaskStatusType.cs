/*
| Module      : Agile
| Entity      : TaskStatusType
| Purpose     : تعیین وضعیت Taskها.
*/

namespace SmartTask.Web.Models.Enums
{
    public enum TaskStatusType
    {
        ToDo = 1,
        InProgress = 2,
        InReview = 3,
        Done = 4,
        Cancelled = 5
    }
}