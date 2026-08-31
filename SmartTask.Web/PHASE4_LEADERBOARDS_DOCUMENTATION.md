/*
| Module      : Gamification - Phase 4
| Document   : Leaderboard System Documentation
| Purpose     : توضیح منطق محاسبه رتبه‌بندی و معماری سیستم
*/

# Phase 4: Leaderboards & Competition System

## Overview
Phase 4 implements a comprehensive leaderboard and competition system for SmartTask Momentum. The system tracks user rankings globally and within workspaces, manages team competitions, and provides real-time ranking updates with caching strategies.

## Architecture

### 1. Database Schema

#### Leaderboard Entity
Tracks individual user rankings at multiple levels:

```
Table: Leaderboards
├── UserId (FK) → ApplicationUser
├── WorkspaceId (FK) → Workspace (nullable for global entries)
├── GlobalRank: Current rank in global leaderboard
├── WorkspaceRank: Current rank in workspace leaderboard
├── TotalPoints: Total experience points accumulated
├── CurrentLevel: User's current progression level
├── TotalExperience: Cumulative experience
├── TasksCompleted: Count of completed tasks
├── ProjectsCompleted: Count of completed projects
├── AchievementsUnlocked: Count of acquired achievements
├── ConsecutiveCompletionDays: Streak tracking
├── WeeklyPoints: Points earned this week
├── MonthlyPoints: Points earned this month
├── LastUpdated: Last calculation timestamp
├── CalculatedAt: When ranking was calculated
├── RankChangeFromPrevious: Position change (+/-)
└── View indices on: GlobalRank, WorkspaceRank, TotalPoints, UserId
```

#### TeamLeaderboard Entity
Tracks team rankings and performance metrics:

```
Table: TeamLeaderboards
├── TeamId (FK) → Team
├── WorkspaceId (FK) → Workspace
├── TeamRank: Rank among all teams in workspace
├── TotalTeamPoints: Sum of all member points
├── AverageTeamLevel: Average member level
├── TotalTeamExperience: Sum of member experience
├── TasksCompleted: Cumulative team task completions
├── ProjectsCompleted: Cumulative team project completions
├── AchievementsUnlocked: Total team achievements
├── ActiveMembersThisWeek: Count of active contributors
├── AverageCompletionRate: % of tasks completed
├── AverageProductivity: Points per member per day
├── WeeklyPoints / MonthlyPoints: Time-range metrics
├── TopMembers: Serialized top 3 members (denormalized)
└── Indexes on: WorkspaceId+TeamRank, TeamId, TotalTeamPoints
```

### 2. Ranking Calculation Logic

#### Global & Workspace Ranking Algorithm

**Sort Order** (Primary to Secondary):
1. **TotalExperience** (Descending) - Primary metric
2. **CurrentLevel** (Descending) - Tiebreaker
3. **TasksCompleted** (Descending) - Secondary tiebreaker
4. **LastProgressUpdate** (Descending) - Activity recency

**Calculation Process**:
```
1. Query all UserProgression records
2. Sort by (TotalExperience DESC, CurrentLevel DESC)
3. Assign sequential ranks (1, 2, 3, ...)
4. Calculate RankChangeFromPrevious = PreviousRank - NewRank
   - Positive value = rank improved (moved up)
   - Negative value = rank declined (moved down)
   - Zero = no change
5. Update LastUpdated timestamp
```

**Time-Range Points**:
- **Weekly Points**: Sum of transactions from last 7 days
  - Reset every Sunday (UTC)
- **Monthly Points**: Sum of transactions from last 30 days
  - Reset on 1st of month (UTC)

#### Team Ranking Algorithm

**Sort Order**:
1. **TotalTeamPoints** (Descending) - Sum of all member points
2. **AverageTeamLevel** (Descending) - Average level of team
3. **ProjectsCompleted** (Descending) - Team velocity

**Calculation Process**:
```
1. For each team in workspace:
   a. Get all team members
   b. Calculate aggregates:
      - TotalTeamPoints = SUM(member.TotalExperience)
      - AverageTeamLevel = AVG(member.CurrentLevel)
      - TasksCompleted = SUM(member.TasksCompleted)
      - ProjectsCompleted = SUM(member.ProjectsCompleted)
      - AchievementsUnlocked = COUNT(member achievements, WHERE UnlockedDate != null)
      - ActiveMembersThisWeek = COUNT(members with progress in last 7 days)
   c. Calculate productivity metrics:
      - AverageProductivity = TotalTeamPoints / TeamMemberCount / 7
      - AverageCompletionRate = CompletedTasks / (CompletedTasks + OverdueTasks) * 100
   d. Sort all teams by (TotalTeamPoints DESC, AverageTeamLevel DESC)
   e. Assign sequential ranks
   f. Calculate rank change
```

**Top Members** (Denormalized):
- Query top 3 team members by TotalPoints
- Store username, level, and rank for quick display
- Updated during leaderboard recalculation

### 3. Recalculation Strategy

#### Background Job: GamificationBackgroundService

**Schedule**: Every hour (configurable)

**Process Flow**:
```
Timer fires every 1 hour (after initial 5-minute delay)
├── Call RecalculateAllLeaderboardsAsync()
├── Step 1: RecalculateGlobalLeaderboard()
│   └── Sorts all users by progression metrics
│   └── Assigns global ranks
├── Step 2: For each Workspace
│   ├── RecalculateWorkspaceLeaderboard()
│   │   └── Sorts workspace members
│   │   └── Assigns workspace-scoped ranks
│   ├── RecalculateTeamLeaderboard()
│   │   └── Calculates team aggregates
│   │   └── Assigns team ranks within workspace
├── Step 3: Update time-range points (weekly/monthly)
├── Step 4: Clear cache entries
└── Log completion with timestamps
```

**Performance Optimization**:
- Uses batch operations via SaveChangesAsync()
- Indexes on frequently queried columns
- Time-range calculations use efficient date filtering
- Workspace-scoped processing reduces dataset size

### 4. Caching Strategy

#### Cache Keys Format
```
Cache Layer: IMemoryCache
├── Global Leaderboard
│   └── Key: "leaderboard_global_{page}_{timeRange}"
│   └── TTL: 60 minutes
├── Workspace Leaderboard
│   └── Key: "leaderboard_workspace_{workspaceId}_{page}_{timeRange}"
│   └── TTL: 60 minutes
├── Team Leaderboard
│   └── Key: "leaderboard_teams_{workspaceId}_{timeRange}"
│   └── TTL: 60 minutes
└── User Context
    └── Key: "leaderboard_user_{userId}_{timeRange}"
    └── TTL: 60 minutes
```

#### Cache Invalidation
- Automatic expiration: 60 minutes
- Manual clear on leaderboard recalculation
- Consider Redis for distributed scenarios

### 5. API Endpoints

```
GET  /api/gamification/leaderboards/global
     ?page=1&pageSize=50&timeRange=all|week|month
     → Returns paginated global rankings
     
GET  /api/gamification/leaderboards/workspace/{workspaceId}
     ?page=1&pageSize=50&timeRange=all|week|month
     → Returns workspace member rankings (requires workspace membership)
     
GET  /api/gamification/leaderboards/teams/{workspaceId}
     ?timeRange=all|week|month
     → Returns team rankings with metrics (requires workspace membership)
     
GET  /api/gamification/leaderboards/user/{userId}
     ?timeRange=all|week|month
     → Returns user's rank + 2 neighbors above/below (requires auth)
     
GET  /api/gamification/leaderboards/user/{userId}/team-rank
     → Returns user's team rank and total teams
     
GET  /api/gamification/leaderboards/user-entry/{userId}
     ?workspaceId=null
     → Returns single user's leaderboard entry
```

### 6. Data Flow

#### User Progression Update
```
1. User completes task
   ↓
2. RewardEngine.AwardRewardAsync()
   ├── Update UserWallet (points)
   ├── Update UserProgression (level, experience, counts)
   ├── Create WalletTransaction record
   ↓
3. On next hourly cycle:
   ├── GamificationBackgroundService timer fires
   ├── RecalculateAllLeaderboardsAsync() executes
   ├── Leaderboard entries recalculated
   ├── Rankings updated
   ├── Cache cleared
   ↓
4. Next API request gets fresh data
```

#### Team Formation
```
1. Team created or member added
   ↓
2. LeaderboardInitializationService.InitializeTeamLeaderboardAsync()
   ├── Create TeamLeaderboard record with initial values
   ↓
3. On next hourly recalculation:
   ├── Team's aggregate metrics calculated
   ├── Rank assigned based on points
```

### 7. Performance Considerations

#### Query Optimization
- **Indexes**: GlobalRank, WorkspaceRank, TotalPoints, LastUpdated
- **Join optimization**: Include User/Team relations in single query
- **Pagination**: Always page large result sets (max 100 per page)

#### Scalability
- **Workspace-scoped calculation**: Reduces dataset per calculation
- **Batch updates**: Single SaveChangesAsync() for all rank assignments
- **Denormalization**: TopMembers array avoids N+1 queries
- **Time-series data**: Weekly/Monthly points reset maintain data freshness

#### Caching Benefits
- 60-minute TTL reduces database queries by ~98% (for stable rankings)
- Page-level caching: Different cache entries per page
- Time-range caching: Separate cache per time filter

### 8. Edge Cases & Handling

#### New Users
- LeaderboardInitializationService creates placeholder entry
- GlobalRank initially set to int.MaxValue (sorted to bottom)
- First recalculation assigns proper rank

#### Inactive Users
- Included in rankings (historical data preserved)
- Can be filtered via ConsecutiveCompletionDays = 0
- LastProgressUpdate shows inactivity

#### Workspace Removal
- Cascade delete on WorkspaceId foreign key
- WorkspaceMember removal automatically excludes user from workspace ranking
- TeamLeaderboard entries deleted with workspace

#### Ranking Stability
- RankChangeFromPrevious shows movement history
- Same points maintain relative rank (stable sort)
- Historical tracking via WalletTransaction records

### 9. Future Enhancements

**Phase 5 Planned Features**:
- Real-time ranking updates via SignalR
- Leaderboard notifications (when user moves up/down)
- Seasonal leaderboards (reset monthly/quarterly)
- Leaderboard achievements (reach top 10, etc.)
- Team performance bonuses/penalties
- Customizable ranking criteria per workspace

**Scalability Enhancements**:
- Migrate IMemoryCache → Redis for distributed systems
- Event-driven recalculation (instead of timer-based)
- Materialized views for large workspaces
- Eventual consistency model for non-critical updates

## Testing Strategy

### Unit Tests
- Ranking algorithm correctness
- Tie-breaking behavior
- Time-range reset logic
- Rank change calculations

### Integration Tests
- Full recalculation workflow
- Cache population and expiration
- API response formatting
- Permission checks

### Performance Tests
- Recalculation time for 10K+ users
- Query performance with indexes
- Cache hit rate monitoring
- Concurrent API request handling

## Files Created

### Entities
- `Models/Entities/Leaderboard.cs`
- `Models/Entities/TeamLeaderboard.cs`

### Services
- `Services/Gamification/ILeaderboardService.cs`
- `Services/Gamification/LeaderboardService.cs`
- `Services/Gamification/LeaderboardInitializationService.cs`

### Background Jobs
- `Infrastructure/BackgroundJobs/GamificationBackgroundService.cs`

### API
- Controller endpoints in `Controllers/GamificationController.cs`

### Views
- `Views/Gamification/Leaderboards.cshtml`
- `Views/Shared/_UserProfileBadge.cshtml`

### Database
- `Data/Configurations/LeaderboardConfiguration.cs`
- `Data/Configurations/TeamLeaderboardConfiguration.cs`
- Migration: `20260829_Phase4_Leaderboards.cs`

### DTOs
- `Models/ViewModels/Gamification/LeaderboardEntryDto.cs`
- `Models/ViewModels/Gamification/TeamLeaderboardDto.cs`
- `Models/ViewModels/Gamification/LeaderboardUserContextDto.cs`
