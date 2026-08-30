/*
| Module      : Infrastructure
| Class       : AchievementSeeder
| Purpose     : تعریف و کاشت دستاوردهای پیش‌فرض
*/

using SmartTask.Web.Data.Context;
using SmartTask.Web.Models.Entities;
using SmartTask.Web.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace SmartTask.Web.Infrastructure.Seed
{
    public static class AchievementSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            try
            {
                // Check if achievements already exist
                if (await context.Set<Achievement>().AnyAsync())
                {
                    return;
                }

                var achievements = GetAchievements();
                await context.Set<Achievement>().AddRangeAsync(achievements);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Log error silently
                Console.WriteLine($"Error seeding achievements: {ex.Message}");
            }
        }

        private static List<Achievement> GetAchievements()
        {
            return new List<Achievement>
            {
                new Achievement
                {
                    Name = "First Task",
                    Description = "اولین کار خود را تکمیل کنید",
                    Icon = "🎯",
                    Color = "#4CAF50",
                    Rarity = AchievementRarity.Common,
                    Category = AchievementCategory.TaskCompletion,
                    RewardPoints = 50,
                    RewardExperience = 100,
                    Condition = "TasksCompleted",
                    ConditionValue = 1,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = "System",
                    ViewState = true
                },

                new Achievement
                {
                    Name = "Getting Started",
                    Description = "5 کار را تکمیل کنید",
                    Icon = "⭐",
                    Color = "#2196F3",
                    Rarity = AchievementRarity.Common,
                    Category = AchievementCategory.TaskCompletion,
                    RewardPoints = 100,
                    RewardExperience = 250,
                    Condition = "TasksCompleted",
                    ConditionValue = 5,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = "System",
                    ViewState = true
                },

                // Task Milestones
                new Achievement
                {
                    Name = "Productive",
                    Description = "25 کار را تکمیل کنید",
                    Icon = "💪",
                    Color = "#FF9800",
                    Rarity = AchievementRarity.Uncommon,
                    Category = AchievementCategory.TaskCompletion,
                    RewardPoints = 250,
                    RewardExperience = 500,
                    Condition = "TasksCompleted",
                    ConditionValue = 25,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = "System",
                    ViewState = true
                },

                new Achievement
                {
                    Name = "Task Master",
                    Description = "100 کار را تکمیل کنید",
                    Icon = "🏆",
                    Color = "#FF5722",
                    Rarity = AchievementRarity.Rare,
                    Category = AchievementCategory.TaskCompletion,
                    RewardPoints = 500,
                    RewardExperience = 1000,
                    Condition = "TasksCompleted",
                    ConditionValue = 100,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = "System",
                    ViewState = true
                },

                new Achievement
                {
                    Name = "Legendary",
                    Description = "500 کار را تکمیل کنید",
                    Icon = "👑",
                    Color = "#9C27B0",
                    Rarity = AchievementRarity.Epic,
                    Category = AchievementCategory.TaskCompletion,
                    RewardPoints = 1000,
                    RewardExperience = 2500,
                    Condition = "TasksCompleted",
                    ConditionValue = 500,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = "System",
                    ViewState = true
                },

                // Project Achievements
                new Achievement
                {
                    Name = "Project Pioneer",
                    Description = "اولین پروژه خود را تکمیل کنید",
                    Icon = "🚀",
                    Color = "#00BCD4",
                    Rarity = AchievementRarity.Common,
                    Category = AchievementCategory.ProjectCompletion,
                    RewardPoints = 200,
                    RewardExperience = 400,
                    Condition = "ProjectsCompleted",
                    ConditionValue = 1,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = "System",
                    ViewState = true
                },

                new Achievement
                {
                    Name = "Project Master",
                    Description = "5 پروژه را تکمیل کنید",
                    Icon = "🎊",
                    Color = "#673AB7",
                    Rarity = AchievementRarity.Rare,
                    Category = AchievementCategory.ProjectCompletion,
                    RewardPoints = 600,
                    RewardExperience = 1200,
                    Condition = "ProjectsCompleted",
                    ConditionValue = 5,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = "System",
                    ViewState = true
                },

                // Sprint Achievements
                new Achievement
                {
                    Name = "Sprint Starter",
                    Description = "اولین اسپرینت خود را تکمیل کنید",
                    Icon = "⚡",
                    Color = "#3F51B5",
                    Rarity = AchievementRarity.Common,
                    Category = AchievementCategory.SprintExecution,
                    RewardPoints = 150,
                    RewardExperience = 300,
                    Condition = "SprintsCompleted",
                    ConditionValue = 1,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = "System",
                    ViewState = true
                },

                new Achievement
                {
                    Name = "Sprint Master",
                    Description = "10 اسپرینت را تکمیل کنید",
                    Icon = "🔥",
                    Color = "#E91E63",
                    Rarity = AchievementRarity.Rare,
                    Category = AchievementCategory.SprintExecution,
                    RewardPoints = 500,
                    RewardExperience = 1000,
                    Condition = "SprintsCompleted",
                    ConditionValue = 10,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = "System",
                    ViewState = true
                }
            };
        }
    }
}
