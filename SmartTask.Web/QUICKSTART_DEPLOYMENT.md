# SmartTask – Quick Start Guide

## ⚠️ Current Issue

**Error:** `Invalid object name 'MarketplaceItems'`

**Cause:** Database hasn't been migrated yet. The table doesn't exist in SQL Server.

**Solution:** Run the migration immediately.

---

## 🚀 Quick Deployment Steps (In Order)

### Step 1: Database Migration (REQUIRED FIRST)
```bash
cd E:\taskManager\SmartTaskManagementSystem\SmartTask.Web
dotnet ef database update
```

This will:
- Create all 16 new entities tables
- Create indexes
- Seed initial data (Achievements, Milestones, Marketplace Items)

**Expected Output:**
```
Build started...
Build succeeded.
Applying migration '20260829170404_Phase1_GamificationFoundation'...
Applying migration '20260829_Phase2_Achievements'...
Applying migration '20260829_Phase3_Marketplace'...
Applying migration '20260829_Phase4_Leaderboards'...
Applying migration '20260829_Phase5_Advanced'...
Applying migration '20260829_AddProductivityMetrics'...
Applying migration '20260829_ProjectSimulation'...
Done. All migrations applied successfully.
```

### Step 2: Verify Seeding

Query your SQL Server database:
```sql
-- Verify tables exist
SELECT COUNT(*) as AchievementCount FROM Achievements;         -- Expected: 10+
SELECT COUNT(*) as MarketplaceCount FROM MarketplaceItems;     -- Expected: 37
SELECT COUNT(*) as MilestoneCount FROM Milestones;             -- Expected: 9+

-- Verify leaderboard table
SELECT COUNT(*) as LeaderboardCount FROM Leaderboards;         -- Expected: 0 (will populate on first run)

-- Verify productivity tables
SELECT COUNT(*) as ProductivityCount FROM ProductivityMetrics; -- Expected: 0 (will populate hourly)
```

### Step 3: Start Application

```bash
dotnet run
```

Application will:
- Start Kestrel web server
- Initialize background services
- Begin hourly timers for:
  - Leaderboard recalculation
  - Streak resets
  - Seasonal events
  - Abuse detection
  - Productivity metrics

### Step 4: Test Endpoints

```bash
# Test basic endpoint
curl http://localhost:5000/api/gamification/achievements

# Should return: 10+ achievements in JSON
```

### Step 5: Test Key Workflows

**Workflow 1: Complete Task & Get Reward**
1. Go to any project
2. Create a task or find existing one
3. Mark as complete
4. Observe: UserWallet updates, WalletTransaction created, XP awarded

**Workflow 2: Check Productivity Score**
1. Go to `/api/productivity/user/{userId}`
2. Should return: Score, tier, rates, history

**Workflow 3: Simulate Project Delay**
1. Go to `/api/simulation/project/{projectId}/what-if`
2. POST with `{ taskId: X, delayDays: 5 }`
3. Should return: Affected tasks, project delay, ripple effects

---

## 🔧 If Migration Fails

### Issue: "Cannot find migration 20260829..."
**Solution:** Ensure all migration files exist in `Migrations/` folder

### Issue: "The database already exists with a different schema"
**Solution:** 
```bash
# Option 1: Drop and recreate (development only)
dotnet ef database drop -f
dotnet ef database update

# Option 2: Create new migration for schema mismatch
dotnet ef migrations add FixSchemaIssue
dotnet ef database update
```

### Issue: "Seed data not loading"
**Solution:** Check `Infrastructure/Seed/` for seeder files and ensure they're called in `Program.cs`

---

## 📊 After Migration

### Database Tables Created (16 total)

**Gamification Tables:**
- Achievements
- UserAchievements
- Milestones
- UserMilestoneProgress
- UserProgression
- UserWallet
- WalletTransactions
- MarketplaceItems
- UserInventories
- MarketplaceTransactions
- Leaderboards
- TeamLeaderboards
- UserStreaks
- SeasonalEvents
- AbuseReports

**Productivity Tables:**
- ProductivityMetrics
- ProductivityScoreHistory

**Simulation Tables:**
- ProjectSimulations
- SimulationScenarios

### Background Jobs Started

✅ **Leaderboard Service** — Updates every hour
✅ **Streak Service** — Checks daily at user timezone midnight
✅ **Seasonal Event Service** — Updates every 6 hours
✅ **Abuse Detection** — Scans hourly
✅ **Productivity Metrics** — Recalculates hourly

---

## ✅ Verification Checklist

After running `dotnet ef database update`:

- [ ] Build completed without errors
- [ ] All migration files applied
- [ ] 16 new tables created in SQL Server
- [ ] Seed data loaded (verify counts above)
- [ ] Application starts without errors
- [ ] Background jobs are running (check Application Insights logs)
- [ ] API endpoints return data
- [ ] No "Invalid object name" errors

---

## 🎯 You Are Here

```
Migration Required ← YOU ARE HERE
       ↓
Verify Seeding
       ↓
Start Application
       ↓
Test Endpoints
       ↓
✅ Production Ready
```

---

## Next Command to Run

```bash
dotnet ef database update
```

**This single command will:**
1. Build the project
2. Apply all pending migrations
3. Create all tables
4. Seed initial data
5. Fix the "Invalid object name" error

**Then run:**
```bash
dotnet run
```

**That's it!** The system will be fully operational.

---

**If you encounter any errors after running `dotnet ef database update`, let me know the error message and I'll fix it immediately.**
