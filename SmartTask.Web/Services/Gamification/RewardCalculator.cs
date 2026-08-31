/*
| Module      : Gamification
| Class       : RewardCalculator
| Purpose     : محاسبه فرمول‌های پاداش با اصلاح‌کننده‌ها
*/

namespace SmartTask.Web.Services.Gamification
{
    public class RewardCalculator
    {
        private const int BASE_TASK_REWARD = 100;
        private const int BASE_PROJECT_REWARD = 500;
        private const int BASE_SPRINT_REWARD = 300;

        public int CalculateTaskReward(int priority, int complexity, int priorityModifier, int complexityModifier, int streakBonus, int timeBonus)
        {
            int baseReward = BASE_TASK_REWARD;
            int priorityBonus = baseReward * priorityModifier / 100;
            int complexityBonus = baseReward * complexityModifier / 100;

            return baseReward + priorityBonus + complexityBonus + streakBonus + timeBonus;
        }

        public int CalculateProjectReward(int totalTasks)
        {
            if (totalTasks == 0)
                return BASE_PROJECT_REWARD;

            int taskBonus = (totalTasks * 10);
            return BASE_PROJECT_REWARD + taskBonus;
        }

        public int CalculateSprintReward(int completedTasks, int totalTasks)
        {
            if (totalTasks == 0)
                return BASE_SPRINT_REWARD;

            double completionRate = (double)completedTasks / totalTasks;
            int bonus = (int)(BASE_SPRINT_REWARD * completionRate * 0.5);

            return BASE_SPRINT_REWARD + bonus;
        }

        public int GetPriorityModifier(int priority)
        {
            return priority switch
            {
                1 => 10,    // Low
                2 => 25,    // Medium
                3 => 50,    // High
                4 => 75,    // Critical
                _ => 0
            };
        }

        public int GetComplexityModifier(int complexity)
        {
            return complexity switch
            {
                1 => 5,     // Very Easy
                2 => 15,    // Easy
                3 => 30,    // Medium
                4 => 50,    // Hard
                5 => 80,    // Very Hard
                _ => 0
            };
        }

        public int CalculateStreakBonus(int consecutiveTasksCompleted)
        {
            if (consecutiveTasksCompleted < 3)
                return 0;

            int streaks = consecutiveTasksCompleted / 5;
            return streaks * 50;
        }

        public int CalculateTimeBonus(DateTime taskCreatedDate, DateTime completionDate)
        {
            var daysToComplete = (completionDate - taskCreatedDate).TotalDays;

            if (daysToComplete <= 1)
                return 100;  // Completed same day
            else if (daysToComplete <= 3)
                return 50;   // Completed within 3 days
            else if (daysToComplete <= 7)
                return 25;   // Completed within a week
            else
                return 0;    // No bonus after a week
        }
    }
}
