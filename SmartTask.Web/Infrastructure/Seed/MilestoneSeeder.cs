/*
| Module      : Infrastructure
| Class       : MilestoneSeeder
| Purpose     : تعریف و کاشت نقاط عطف پیش‌فرض
*/

using SmartTask.Web.Data.Context;
using SmartTask.Web.Models.Entities;
using SmartTask.Web.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace SmartTask.Web.Infrastructure.Seed
{
    public static class MilestoneSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            try
            {
                // Check if milestones already exist
                if (await context.Set<Milestone>().AnyAsync())
                {
                    return;
                }

                var milestones = GetMilestones();
                await context.Set<Milestone>().AddRangeAsync(milestones);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Log error silently
                Console.WriteLine($"Error seeding milestones: {ex.Message}");
            }
        }

        private static List<Milestone> GetMilestones()
        {
            return new List<Milestone>
            {
                // Task Milestones
                new Milestone
                {
                    Name = "10 Tasks Completed",
                    Description = "10 کار را تکمیل کنید",
                    Icon = "📋",
                    Color = "#4CAF50",
                    Type = MilestoneType.TaskCompletion,
                    TargetValue = 10,
                    RewardPoints = 150,
                    RewardExperience = 300,
                    Condition = "TasksCompleted",
                    IsActive = true,
                    DisplayOrder = 1,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = "System",
                    ViewState = true
                },

                new Milestone
                {
                    Name = "50 Tasks Completed",
                    Description = "50 کار را تکمیل کنید",
                    Icon = "📊",
                    Color = "#2196F3",
                    Type = MilestoneType.TaskCompletion,
                    TargetValue = 50,
                    RewardPoints = 350,
                    RewardExperience = 700,
                    Condition = "TasksCompleted",
                    IsActive = true,
                    DisplayOrder = 2,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = "System",
                    ViewState = true
                },

                new Milestone
                {
                    Name = "100 Tasks Completed",
                    Description = "100 کار را تکمیل کنید",
                    Icon = "💯",
                    Color = "#FF9800",
                    Type = MilestoneType.TaskCompletion,
                    TargetValue = 100,
                    RewardPoints = 500,
                    RewardExperience = 1000,
                    Condition = "TasksCompleted",
                    IsActive = true,
                    DisplayOrder = 3,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = "System",
                    ViewState = true
                },

                new Milestone
                {
                    Name = "250 Tasks Completed",
                    Description = "250 کار را تکمیل کنید",
                    Icon = "🌟",
                    Color = "#FF5722",
                    Type = MilestoneType.TaskCompletion,
                    TargetValue = 250,
                    RewardPoints = 800,
                    RewardExperience = 1500,
                    Condition = "TasksCompleted",
                    IsActive = true,
                    DisplayOrder = 4,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = "System",
                    ViewState = true
                },

                new Milestone
                {
                    Name = "500 Tasks Completed",
                    Description = "500 کار را تکمیل کنید",
                    Icon = "🏅",
                    Color = "#9C27B0",
                    Type = MilestoneType.TaskCompletion,
                    TargetValue = 500,
                    RewardPoints = 1200,
                    RewardExperience = 2500,
                    Condition = "TasksCompleted",
                    IsActive = true,
                    DisplayOrder = 5,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = "System",
                    ViewState = true
                },

                // Project Milestones
                new Milestone
                {
                    Name = "3 Projects Completed",
                    Description = "3 پروژه را تکمیل کنید",
                    Icon = "🚀",
                    Color = "#00BCD4",
                    Type = MilestoneType.ProjectCompletion,
                    TargetValue = 3,
                    RewardPoints = 300,
                    RewardExperience = 600,
                    Condition = "ProjectsCompleted",
                    IsActive = true,
                    DisplayOrder = 6,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = "System",
                    ViewState = true
                },

                new Milestone
                {
                    Name = "10 Projects Completed",
                    Description = "10 پروژه را تکمیل کنید",
                    Icon = "🎯",
                    Color = "#673AB7",
                    Type = MilestoneType.ProjectCompletion,
                    TargetValue = 10,
                    RewardPoints = 700,
                    RewardExperience = 1400,
                    Condition = "ProjectsCompleted",
                    IsActive = true,
                    DisplayOrder = 7,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = "System",
                    ViewState = true
                },

                // Sprint Milestones
                new Milestone
                {
                    Name = "5 Sprints Completed",
                    Description = "5 اسپرینت را تکمیل کنید",
                    Icon = "⚡",
                    Color = "#3F51B5",
                    Type = MilestoneType.SprintCompletion,
                    TargetValue = 5,
                    RewardPoints = 250,
                    RewardExperience = 500,
                    Condition = "SprintsCompleted",
                    IsActive = true,
                    DisplayOrder = 8,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = "System",
                    ViewState = true
                },

                new Milestone
                {
                    Name = "20 Sprints Completed",
                    Description = "20 اسپرینت را تکمیل کنید",
                    Icon = "🔥",
                    Color = "#E91E63",
                    Type = MilestoneType.SprintCompletion,
                    TargetValue = 20,
                    RewardPoints = 600,
                    RewardExperience = 1200,
                    Condition = "SprintsCompleted",
                    IsActive = true,
                    DisplayOrder = 9,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = "System",
                    ViewState = true
                }
            };
        }
    }
}
