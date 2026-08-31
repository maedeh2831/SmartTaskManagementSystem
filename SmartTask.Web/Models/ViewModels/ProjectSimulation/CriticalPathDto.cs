namespace SmartTask.Web.Models.ViewModels.ProjectSimulation
{
    public class CriticalPathDto
    {
        public List<int> CriticalPathTaskIds { get; set; } = new();

        public int CriticalPathLengthDays { get; set; }

        public List<TaskSlackDto> TaskSlackTimes { get; set; } = new();

        public DateTime ProjectStartDate { get; set; }

        public DateTime ProjectEndDate { get; set; }

        public int TotalTasksInPath { get; set; }
    }

    public class TaskSlackDto
    {
        public int TaskId { get; set; }

        public string TaskTitle { get; set; } = null!;

        public int SlackTimeDays { get; set; }

        public bool IsOnCriticalPath { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public int EstimateDays { get; set; }
    }
}
