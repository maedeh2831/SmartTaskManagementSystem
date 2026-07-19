/*
| Module      : Agile
| Entity      : SprintStatusType
| Purpose     : تعیین وضعیت چرخه عمر Sprint.
*/

namespace SmartTask.Web.Models.Enums
{
    public enum SprintStatusType
    {
        Planned = 1,
        Active = 2,
        Completed = 3,
        Cancelled = 4
    }
}