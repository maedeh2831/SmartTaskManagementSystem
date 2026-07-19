/*
| Module      : Agile
| Entity      : StoryStatusType
| Purpose     : تعیین وضعیت User Story.
*/

namespace SmartTask.Web.Models.Enums
{
    public enum StoryStatusType
    {
        New = 1,
        Ready = 2,
        InProgress = 3,
        Done = 4,
        Closed = 5
    }
}