# Phase 5 Integration Guide

## Quick Start

### Step 1: Database Migration

Create and apply EF Core migration:

```bash
cd E:\taskManager\SmartTaskManagementSystem\SmartTask.Web

# Create migration
dotnet ef migrations add Phase5_AdvancedFeaturesAndAntiaAbuse

# Apply migration
dotnet ef database update
```

### Step 2: Register Services in Dependency Injection

Update `Program.cs` or `Startup.cs`:

```csharp
// Add to services collection
services.AddScoped<IStreakService, StreakService>();
services.AddScoped<ISeasonalEventService, SeasonalEventService>();
services.AddScoped<IAbuseDetectionEngine, AbuseDetectionEngine>();
services.AddScoped<IGamificationAnalyticsService, GamificationAnalyticsService>();
```

### Step 3: Update Background Job Service

Update `Infrastructure/BackgroundJobs/GamificationBackgroundService.cs`:

```csharp
public class GamificationBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<GamificationBackgroundService> _logger;

    public GamificationBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<GamificationBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var streakService = scope.ServiceProvider.GetRequiredService<IStreakService>();
                    var seasonalService = scope.ServiceProvider.GetRequiredService<ISeasonalEventService>();
                    var abuseEngine = scope.ServiceProvider.GetRequiredService<IAbuseDetectionEngine>();

                    // Run daily tasks (once per day at midnight UTC)
                    await streakService.ResetStreaksAsync();
                    
                    // Run seasonal event processing
                    await seasonalService.ProcessSeasonalAwardsAsync();

                    // Scan for abuse (hourly or per transaction)
                    // Note: Can be called per user after rewards awarded
                    
                    _logger.LogInformation("Gamification background tasks completed at {Time}", DateTime.UtcNow);
                }

                // Sleep 1 hour before next run
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in gamification background service");
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }
}
```

### Step 4: Integrate Abuse Detection with Reward Engine

Update `Services/Gamification/RewardEngine.cs`:

```csharp
public class RewardEngine : IRewardEngine
{
    private readonly ApplicationDbContext _context;
    private readonly RewardCalculator _calculator;
    private readonly ILogger<RewardEngine> _logger;
    private readonly IAbuseDetectionEngine _abuseEngine; // Add this

    public RewardEngine(
        ApplicationDbContext context, 
        ILogger<RewardEngine> logger,
        IAbuseDetectionEngine abuseEngine) // Add parameter
    {
        _context = context;
        _calculator = new RewardCalculator();
        _logger = logger;
        _abuseEngine = abuseEngine;
    }

    public async Task AwardRewardAsync(int userId, int points, string description, int? relatedTaskId = null)
    {
        try
        {
            // Check if user is suspended
            var isSuspended = await _abuseEngine.IsUserSuspendedAsync(userId);
            if (isSuspended)
            {
                _logger.LogWarning("Reward denied: User {UserId} is suspended", userId);
                return; // Don't award rewards to suspended users
            }

            var wallet = await _context.Set<UserWallet>()
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (wallet == null)
            {
                _logger.LogWarning("UserWallet for user {UserId} not found", userId);
                return;
            }

            var progression = await _context.Set<UserProgression>()
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (progression == null)
            {
                _logger.LogWarning("UserProgression for user {UserId} not found", userId);
                return;
            }

            wallet.TotalPoints += points;
            wallet.AvailablePoints += points;
            wallet.LastUpdated = DateTime.UtcNow;

            var transaction = new WalletTransaction
            {
                UserWalletId = wallet.Id,
                UserProgressionId = progression.Id,
                Amount = points,
                TransactionType = TransactionType.Earned,
                Description = description,
                RelatedTaskId = relatedTaskId,
                TransactionDate = DateTime.UtcNow,
                CreatedBy = userId.ToString(),
                CreatedDate = DateTime.UtcNow
            };

            _context.Set<WalletTransaction>().Add(transaction);
            await _context.SaveChangesAsync();

            // Scan for abuse after reward
            await _abuseEngine.ScanUserActivityAsync(userId);

            _logger.LogInformation("Awarded {Points} points to user {UserId} for {Description}", points, userId, description);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error awarding reward to user {UserId}", userId);
        }
    }
}
```

### Step 5: Integrate Streak Service with Task Completion

Update task completion handler (in `Controllers/TaskController.cs` or event handler):

```csharp
// After task is marked complete and reward awarded
var streakService = serviceProvider.GetRequiredService<IStreakService>();
var userId = task.AssignedToUserId;
var xpGained = 100; // Get actual XP from reward calculation

// Update streak and check milestones
await streakService.UpdateStreakAsync(userId, xpGained);
var (current, longest, milestonesReached) = await streakService.CheckMilestonesAsync(userId);

if (milestonesReached > 0)
{
    // Award milestone bonus points via RewardEngine
    var milestoneBonusPoints = CalculateMilestoneBonus(current);
    await rewardEngine.AwardRewardAsync(userId, milestoneBonusPoints, $"Streak milestone: {current} days");
}
```

### Step 6: Integrate Seasonal Events

Update event processing in task completion:

```csharp
// After reward awarded
var seasonalService = serviceProvider.GetRequiredService<ISeasonalEventService>();
var activeEvents = await seasonalService.GetActiveEventsAsync();

foreach (var eventData in activeEvents)
{
    int eventId = eventData.Id;
    
    // Check if user participates in this event
    var userProgress = await _context.Set<UserSeasonalEventProgress>()
        .FirstOrDefaultAsync(p => p.UserId == userId && p.SeasonalEventId == eventId);

    if (userProgress != null)
    {
        // Calculate event-specific reward with multiplier
        var eventPoints = (int)(baseReward * eventData.RewardBonusMultiplier + eventData.ExtraPointsPerCompletion);
        
        // Update user progress
        await seasonalService.UpdateUserProgressAsync(userId, eventId, eventPoints);
    }
}
```

### Step 7: Configure Timezone Support

Update user settings to store timezone:

```csharp
// After user login or settings update
var streakService = serviceProvider.GetRequiredService<IStreakService>();
var userTimeZone = user.TimeZone ?? TimeZoneInfo.Local.Id; // Get from user profile
await streakService.SetUserTimeZoneAsync(userId, userTimeZone);
```

### Step 8: Add Admin Authorization

Ensure `Admin` role exists in database:

```csharp
// In seed data or initial setup
var adminRole = new IdentityRole<int> { Name = "Admin", NormalizedName = "ADMIN" };
await roleManager.CreateAsync(adminRole);

// Assign role to admin user
await userManager.AddToRoleAsync(adminUser, "Admin");
```

---

## API Endpoints Reference

### Admin Metrics

```
GET /api/admin/gamification/metrics
Response:
{
  "metrics": {
    "totalXpDistributed": 1000000,
    "totalMomentumCirculating": 50000,
    "averageMomentumPerUser": 1250,
    "purchaseVelocity": 45.5,
    "activeUsersInLastWeek": 800,
    "activeUsersInLastMonth": 2500,
    ...
  },
  "levelDistribution": [...],
  "achievementRates": [...]
}
```

### Abuse Reports

```
GET /api/admin/gamification/abuse-reports?status=Pending
Response: [
  {
    "id": 1,
    "userId": 123,
    "userName": "suspicious_user",
    "reportType": "RapidCompletion",
    "status": "Pending",
    "severityScore": 85,
    "confidenceLevel": 0.95,
    ...
  }
]
```

### User Progression

```
GET /api/admin/gamification/users/{userId}/progression
Response:
{
  "userId": 123,
  "userName": "john_doe",
  "level": 5,
  "totalExperience": 5000,
  "totalPoints": 10000,
  "currentStreak": 15,
  "globalRank": 42,
  "rewardsSuspended": false,
  ...
}
```

### Resolve Abuse Report

```
POST /api/admin/gamification/abuse-reports/{reportId}/resolve
Request:
{
  "status": "Confirmed",
  "notes": "User was rapidly completing low-value tasks",
  "refundAmount": 5000,
  "suspendUntil": "2026-09-29T00:00:00Z"
}
```

---

## Testing Integration

### Manual Testing Checklist

- [ ] Create a test user
- [ ] Complete a task and verify reward awarded
- [ ] Verify streak incremented
- [ ] Verify no abuse report generated (normal activity)
- [ ] Check admin dashboard shows updated metrics
- [ ] Simulate rapid task completion (>50/hour)
- [ ] Verify abuse report created automatically
- [ ] Admin resolves report with refund
- [ ] Verify points refunded in wallet
- [ ] Verify user suspended from earning
- [ ] Create seasonal event
- [ ] Verify user can join event
- [ ] Complete tasks and verify event progress updated
- [ ] Check event leaderboard

### Integration Test Example

```csharp
[Test]
public async Task TestFullGamificationFlow()
{
    // Setup
    var userId = 1;
    var taskId = 100;
    
    // Create user with streak
    var streakService = new StreakService(_context, _logger);
    
    // Award reward
    await _rewardEngine.AwardRewardAsync(userId, 100, "Task completion");
    
    // Update streak
    await streakService.UpdateStreakAsync(userId, 100);
    
    // Check milestone
    var (current, longest, milestones) = await streakService.CheckMilestonesAsync(userId);
    Assert.Greater(current, 0);
    
    // Verify no abuse report (normal activity)
    var reports = await _abuseEngine.GetPendingReportsAsync();
    var userReports = reports.Where(r => r.UserId == userId);
    Assert.IsEmpty(userReports);
    
    // Simulate rapid completion
    for (int i = 0; i < 51; i++)
    {
        await _rewardEngine.AwardRewardAsync(userId, 100, $"Task {i}");
    }
    
    // Scan for abuse
    await _abuseEngine.ScanUserActivityAsync(userId);
    
    // Verify abuse report created
    reports = await _abuseEngine.GetPendingReportsAsync();
    userReports = reports.Where(r => r.UserId == userId);
    Assert.IsNotEmpty(userReports);
    Assert.AreEqual(AbuseReportType.RapidCompletion, userReports.First().ReportType);
}
```

---

## Configuration File

Add to `appsettings.json`:

```json
{
  "Gamification": {
    "Enabled": true,
    "Streaks": {
      "MilestoneBonus3Days": 150,
      "MilestoneBonus7Days": 300,
      "MilestoneBonus14Days": 500,
      "MilestoneBonus30Days": 1000,
      "MilestoneBonus100Days": 5000,
      "ResetHour": 0,
      "DefaultTimeZone": "UTC"
    },
    "AbuseDetection": {
      "Enabled": true,
      "RapidCompletionThreshold": 50,
      "SigmaThreshold": 5.0,
      "ScanIntervalMinutes": 60,
      "AutoSuspendHighSeverity": true,
      "SeverityScoreWeights": {
        "RapidCompletion": 1.0,
        "VelocityAnomaly": 0.8,
        "DuplicateCompletions": 0.6,
        "SystemManipulation": 1.2,
        "LowEstimateFarming": 0.7
      }
    },
    "SeasonalEvents": {
      "Enabled": true,
      "ProcessIntervalMinutes": 1440,
      "MaxConcurrentEvents": 5
    },
    "Analytics": {
      "CacheResultsMinutes": 5,
      "EnableHistoricalTracking": true
    }
  }
}
```

---

## Monitoring Setup

### Key Metrics to Monitor

1. **Abuse Report Queue Size** - Alert if >100 pending
2. **XP Distribution Rate** - Alert if >1M/hour
3. **User Suspension Rate** - Alert if >5% in 24h
4. **Streak Reset Success** - Should be 100%
5. **Event Processing Time** - Should complete in <5 min

### Sample Monitoring Query

```sql
-- Check abuse reports by type
SELECT ReportType, COUNT(*) as ReportCount, AVG(SeverityScore) as AvgSeverity
FROM AbuseReports
WHERE DetectionDate >= DATEADD(day, -7, GETDATE())
  AND Status = 0 -- Pending
GROUP BY ReportType
ORDER BY ReportCount DESC;

-- Check reward distribution
SELECT 
    CAST(TransactionDate AS DATE) as Date,
    COUNT(*) as TransactionCount,
    SUM(Amount) as TotalXP,
    AVG(Amount) as AvgReward
FROM WalletTransactions
WHERE TransactionType = 0 -- Earned
  AND TransactionDate >= DATEADD(day, -30, GETDATE())
GROUP BY CAST(TransactionDate AS DATE)
ORDER BY Date DESC;

-- Check active streaks
SELECT 
    CurrentLevel,
    COUNT(*) as UserCount,
    AVG(CurrentStreak) as AvgStreak,
    MAX(LongestStreak) as MaxStreak
FROM UserStreaks
WHERE CurrentStreak > 0
GROUP BY CurrentLevel
ORDER BY CurrentLevel;
```

---

## Troubleshooting

### Issue: Streaks not resetting

**Solution:** Verify timezone configuration
- Check user's `UserStreak.UserTimeZone` setting
- Verify `GamificationBackgroundService` is running
- Check logs for reset task execution

### Issue: Abuse reports not generating

**Solution:** Check abuse engine configuration
- Verify `IAbuseDetectionEngine` is registered in DI
- Check that `ScanUserActivityAsync` is being called after rewards
- Verify detection thresholds in config match your needs

### Issue: Seasonal event not appearing

**Solution:** Verify event configuration
- Check event `StartDate` and `EndDate` are valid
- Verify `IsActive` is true and `Status` is `Active`
- Run `ProcessSeasonalAwardsAsync` manually to force update

### Issue: Admin dashboard returns 401

**Solution:** Verify authorization
- Confirm user has "Admin" role
- Check token includes role claim
- Verify `[Authorize(Roles = "Admin")]` attribute on controller

---

## Performance Optimization

### Database Indexes to Create

```sql
-- For streak queries
CREATE INDEX IX_UserStreak_UserId_CurrentStreak 
  ON UserStreaks(UserId) INCLUDE (CurrentStreak, LastCompletionDate);

-- For abuse report queries
CREATE INDEX IX_AbuseReport_Status_SeverityScore 
  ON AbuseReports(Status, SeverityScore DESC);

-- For seasonal event queries
CREATE INDEX IX_UserSeasonalProgress_UserId_EventId 
  ON UserSeasonalEventProgresses(UserId, SeasonalEventId);

-- For analytics
CREATE INDEX IX_WalletTransaction_TransactionDate_UserId 
  ON WalletTransactions(TransactionDate DESC, UserId);
```

### Caching Recommendations

- Cache active seasonal events (refresh every 5 minutes)
- Cache top users leaderboard (refresh every hour)
- Cache economy metrics (refresh every 15 minutes)
- Cache user progression (refresh on demand + timeout)

---

## Rollback Plan

If issues occur, rollback procedure:

```bash
# 1. Stop the application
# 2. Revert database migration
dotnet ef migrations remove

# 3. Remove new service registrations from DI
# 4. Remove integration code from RewardEngine
# 5. Restart application
```

All user data is preserved; only new features disabled.

---

**Last Updated**: 2026-08-29
**Status**: Ready for Integration
