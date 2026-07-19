/*
| Module      : Agile
| Entity      : TaskType
| Purpose     : تعیین نوع Task.
*/

namespace SmartTask.Web.Models.Enums
{
    public enum TaskType
    {
        Task = 1,
        Bug = 2,
        Feature = 3,
        Improvement = 4,
        TechnicalDebt = 5
    }
}