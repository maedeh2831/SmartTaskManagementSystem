using Microsoft.EntityFrameworkCore;
using SmartTask.Web.Data.Context;
using SmartTask.Web.Models.Entities;
using SmartTask.Web.Models.Enums;

namespace SmartTask.Web.Infrastructure.Seed
{
    public static class GamificationDataSeeder
    {
        public static async Task SeedAllAsync(ApplicationDbContext context)
        {
            try
            {
                // 1. Ensure wallets exist for all users
                var users = await context.Set<ApplicationUser>().ToListAsync();
                foreach (var user in users)
                {
                    var hasWallet = await context.Set<UserWallet>()
                        .AnyAsync(w => w.UserId == user.Id);
                    if (!hasWallet)
                    {
                        context.Set<UserWallet>().Add(new UserWallet
                        {
                            UserId = user.Id,
                            AvailablePoints = 1500,
                            TotalPoints = 1500,
                            SpentPoints = 0,
                            LastUpdated = DateTime.UtcNow
                        });
                    }
                }

                // 2. Ensure UserProgression exists for all users
                foreach (var user in users)
                {
                    var hasProgression = await context.Set<UserProgression>()
                        .AnyAsync(p => p.UserId == user.Id);
                    if (!hasProgression)
                    {
                        context.Set<UserProgression>().Add(new UserProgression
                        {
                            UserId = user.Id,
                            CurrentLevel = 3,
                            TotalExperience = 2500,
                            ExperienceForNextLevel = 5000,
                            TasksCompleted = 12,
                            ProjectsCompleted = 2,
                            SprintsCompleted = 3,
                            LastProgressUpdate = DateTime.UtcNow,
                            JoinedDate = DateTime.UtcNow
                        });
                    }
                }

                // 3. Ensure ProductivityMetrics exists for current period
                var workspaces = await context.Set<Workspace>()
                    .Where(w => w.ViewState)
                    .ToListAsync();
                foreach (var user in users)
                {
                    foreach (var ws in workspaces)
                    {
                        var hasMetrics = await context.Set<ProductivityMetrics>()
                            .AnyAsync(m => m.UserId == user.Id && m.WorkspaceId == ws.Id && m.IsCurrentPeriod);
                        if (!hasMetrics)
                        {
                            var rng = new Random();
                            context.Set<ProductivityMetrics>().Add(new ProductivityMetrics
                            {
                                UserId = user.Id,
                                WorkspaceId = ws.Id,
                                ProductivityScore = Math.Round(40 + rng.NextDouble() * 50, 2),
                                TaskCompletionRate = Math.Round(30 + rng.NextDouble() * 60, 2),
                                OnTimeDeliveryRate = Math.Round(50 + rng.NextDouble() * 50, 2),
                                ConsistencyRate = Math.Round(20 + rng.NextDouble() * 70, 2),
                                QualityScore = Math.Round(60 + rng.NextDouble() * 40, 2),
                                TotalTasksAssigned = rng.Next(5, 30),
                                TotalTasksCompleted = rng.Next(3, 25),
                                OnTimeTasksCompleted = rng.Next(1, 20),
                                OverdueTasksCompleted = rng.Next(0, 5),
                                TasksReopened = rng.Next(0, 3),
                                WorkedDaysThisPeriod = rng.Next(5, 25),
                                TotalDaysInPeriod = 30,
                                CurrentStreak = rng.Next(1, 15),
                                LongestStreak = rng.Next(5, 20),
                                LastActivityDate = DateTime.UtcNow,
                                CurrentTier = ProductivityTier.Gold,
                                PeriodStartDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1),
                                PeriodEndDate = DateTime.UtcNow,
                                IsCurrentPeriod = true,
                                CreatedDate = DateTime.UtcNow
                            });
                        }
                    }
                }

                // 4. Seed some score history for users
                foreach (var user in users)
                {
                    var metrics = await context.Set<ProductivityMetrics>()
                        .FirstOrDefaultAsync(m => m.UserId == user.Id && m.IsCurrentPeriod);
                    if (metrics != null)
                    {
                        var existingHistory = await context.Set<ProductivityScoreHistory>()
                            .Where(h => h.UserId == user.Id)
                            .CountAsync();
                        if (existingHistory < 7)
                        {
                            for (int i = 1; i <= 7; i++)
                            {
                                var rng = new Random(i * user.Id);
                                context.Set<ProductivityScoreHistory>().Add(new ProductivityScoreHistory
                                {
                                    ProductivityMetricsId = metrics.Id,
                                    UserId = user.Id,
                                    ProductivityScore = Math.Round(35 + rng.NextDouble() * 55, 2),
                                    TaskCompletionRate = Math.Round(30 + rng.NextDouble() * 60, 2),
                                    OnTimeDeliveryRate = Math.Round(40 + rng.NextDouble() * 55, 2),
                                    ConsistencyRate = Math.Round(25 + rng.NextDouble() * 65, 2),
                                    QualityScore = Math.Round(55 + rng.NextDouble() * 45, 2),
                                    TasksCompletedThisPeriod = rng.Next(1, 8),
                                    OnTimeTasksThisPeriod = rng.Next(0, 6),
                                    CurrentStreak = rng.Next(1, 10),
                                    SnapshotDate = DateTime.UtcNow.AddDays(-i),
                                    PeriodType = "Daily",
                                    TierAtSnapshot = (int)ProductivityTier.Gold,
                                    CreatedDate = DateTime.UtcNow.AddDays(-i)
                                });
                            }
                        }
                    }
                }

                // 5. Seed some user achievements
                var achievements = await context.Set<Achievement>().Where(a => a.IsActive).ToListAsync();
                foreach (var user in users)
                {
                    var existingAch = await context.Set<UserAchievement>()
                        .Where(ua => ua.UserId == user.Id)
                        .CountAsync();
                    if (existingAch < 3 && achievements.Count > 0)
                    {
                        // Get user's progression (needed for UserAchievement FK)
                        var progression = await context.Set<UserProgression>()
                            .FirstOrDefaultAsync(p => p.UserId == user.Id);
                        if (progression == null) continue;

                        var rng = new Random(user.Id);
                        var selectedAchievements = achievements
                            .OrderBy(_ => rng.Next())
                            .Take(Math.Min(3, achievements.Count));
                        foreach (var ach in selectedAchievements)
                        {
                            var hasAch = await context.Set<UserAchievement>()
                                .AnyAsync(ua => ua.UserId == user.Id && ua.AchievementId == ach.Id);
                            if (!hasAch)
                            {
                                context.Set<UserAchievement>().Add(new UserAchievement
                                {
                                    UserId = user.Id,
                                    UserProgressionId = progression.Id,
                                    AchievementId = ach.Id,
                                    UnlockedDate = DateTime.UtcNow.AddDays(-rng.Next(1, 30)),
                                    ProgressPercentage = 100,
                                    CreatedDate = DateTime.UtcNow
                                });
                            }
                        }
                    }
                }

                // 6. Seed some leaderboard entries
                foreach (var user in users)
                {
                    var hasLeaderboard = await context.Set<Leaderboard>()
                        .AnyAsync(l => l.UserId == user.Id);
                    if (!hasLeaderboard)
                    {
                        var rng = new Random(user.Id + 100);
                        context.Set<Leaderboard>().Add(new Leaderboard
                        {
                            UserId = user.Id,
                            TotalPoints = rng.Next(500, 5000),
                            TotalExperience = rng.Next(1000, 8000),
                            CurrentLevel = rng.Next(2, 10),
                            TasksCompleted = rng.Next(5, 50),
                            ConsecutiveCompletionDays = rng.Next(1, 15),
                            GlobalRank = rng.Next(1, 20),
                            CreatedDate = DateTime.UtcNow
                        });
                    }
                }

                // 7. Seed some user streaks
                foreach (var user in users)
                {
                    var hasStreak = await context.Set<UserStreak>()
                        .AnyAsync(s => s.UserId == user.Id);
                    if (!hasStreak)
                    {
                        var rng = new Random(user.Id + 200);
                        context.Set<UserStreak>().Add(new UserStreak
                        {
                            UserId = user.Id,
                            CurrentStreak = rng.Next(1, 10),
                            LongestStreak = rng.Next(5, 20),
                            StreakStartDate = DateTime.UtcNow.AddDays(-rng.Next(5, 30)),
                            LastCompletionDate = DateTime.UtcNow,
                            CreatedDate = DateTime.UtcNow
                        });
                    }
                }

                await context.SaveChangesAsync();
                Console.WriteLine("Gamification data seeded successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error seeding gamification data: {ex.Message}");
            }
        }
    }
}
