# SmartTask Momentum - Gamification Architecture

## Overview

SmartTask Momentum is a comprehensive gamification system designed to increase user engagement through a multi-layered reward economy. The system spans 5 phases of implementation, with Phase 5 introducing advanced features and anti-abuse detection.

## Architecture Components

### Phase 1-4: Foundation (Existing)
- **User Progression System**: Tracks XP, levels, and user advancement
- **Reward Engine**: Calculates rewards based on task complexity, priority, and streaks
- **Achievement System**: Unlock-based achievements with rarity tiers
- **Leaderboards**: Global and workspace-level rankings
- **Marketplace**: Virtual economy with purchasable items
- **Milestones**: Long-term progression goals

### Phase 5: Advanced Features & Anti-Abuse

#### 1. Streak System
Tracks consecutive days of user activity with timezone-aware resets.

**Key Features:**
- Daily task completion tracking
- Automatic reset at midnight (user timezone)
- Milestone bonuses at 3, 7, 14, 30, and 100 days
- Configurable timezone support

**Milestone Bonuses:**
```
3 days   → 150 XP
7 days   → 300 XP
14 days  → 500 XP
30 days  → 1,000 XP
100 days → 5,000 XP
```

**Database Entity: UserStreak**
```
- CurrentStreak: Active streak counter
- LongestStreak: Historical maximum
- LastCompletionDate: Last activity timestamp
- UserTimeZone: Configurable timezone
- MilestoneX: Boolean flags for claimed bonuses
```

#### 2. Seasonal Events System
Time-limited events with custom reward multipliers and leaderboards.

**Features:**
- Event status management (Scheduled → Active → Ended)
- Configurable bonus multipliers for achievements and rewards
- Participant tracking and caps
- Event-specific leaderboards
- Automatic activation/deactivation background job

**Event Configuration:**
```json
{
  "name": "Summer Sprint Challenge",
  "startDate": "2026-06-01",
  "endDate": "2026-08-31",
  "achievementBonusMultiplier": 1.5,
  "rewardBonusMultiplier": 2.0,
  "extraPointsPerCompletion": 50,
  "maxParticipants": 1000,
  "hasEventLeaderboard": true
}
```

#### 3. Abuse Detection Engine
Multi-rule system to identify and flag suspicious activity patterns.

**Detection Rules:**

##### Rule 1: Rapid Completion
- **Trigger**: >50 tasks completed per hour
- **Severity**: High
- **Evidence**: Task count, timestamp analysis
- **Action**: Flag for review, potential suspension

##### Rule 2: Velocity Anomaly (5σ Detection)
- **Trigger**: XP gains >5 standard deviations above user average
- **Calculation**: Statistical analysis of 30-day history
- **Severity**: Medium-High
- **Evidence**: Z-score, historical baseline, current spike

##### Rule 3: Duplicate Completions
- **Trigger**: Same task marked complete multiple times
- **Timeframe**: 24-hour window
- **Severity**: Medium
- **Evidence**: Task IDs, completion timestamps

##### Rule 4: System Manipulation
- **Trigger**: Timestamp mismatches (completion before creation)
- **Detection**: Temporal inconsistencies
- **Severity**: High
- **Evidence**: Task timestamps, discrepancies

##### Rule 5: Low-Estimate Task Farming
- **Trigger**: >100 tasks ≤1 hour estimate in 30 days
- **Pattern**: Systematic completion of minimal-effort items
- **Severity**: Medium
- **Evidence**: Task count, estimate distribution

**Report Status Flow:**
```
Pending → UnderReview → Confirmed/False
          ↓
          Resolved/Dismissed
```

**Actions Available:**
- Refund suspicious rewards
- Suspend reward earning temporarily
- Block marketplace access
- Reset user stats if necessary

#### 4. Admin Dashboard & Analytics

**Metrics Dashboard:**
- Total XP distributed (lifetime)
- Active users (7-day, 30-day)
- Level distribution
- Achievement unlock rates
- Marketplace transaction velocity
- Momentum circulating

**User Progression View:**
- Level, XP, points, streaks
- Task/project completion counts
- Achievement count
- Global rank
- Suspension status
- Abuse report history

**Abuse Management:**
- Pending reports list (sorted by severity)
- Detailed report view with evidence
- Resolution workflow
- Refund and suspension controls
- Audit trail

**Marketplace Analytics:**
- Item sales ranking
- Revenue per item
- Category performance
- Rating trends

#### 5. Gamification Analytics Service

Comprehensive metrics for system health and performance monitoring.

**Key Metrics:**
```
Economy Metrics:
  - Total XP distributed
  - Total momentum in circulation
  - Average points per user
  - Purchase velocity (transactions/day)
  - Achievement unlock rate

User Metrics:
  - Daily active users
  - Average XP per active user
  - Level distribution
  - Streak distribution

Achievement Metrics:
  - Unlock count per achievement
  - Unlock percentage (% of users)
  - Rarity distribution

Marketplace Metrics:
  - Sales by item/category
  - Revenue trends
  - User engagement rates
```

## Integration Points

### Event Publishing System
All gamification events are triggered by domain events:

```csharp
// Task Completion
TaskCompletedEvent → RewardEngine → StreakService → SeasonalEventService
                  ↓
            AbuseDetectionEngine

// Project Completion
ProjectCompletedEvent → RewardEngine (project bonus)

// Sprint Completion
SprintCompletedEvent → RewardEngine (sprint bonus)
```

### Database Integration
New entities added to ApplicationDbContext:
```csharp
public DbSet<UserStreak> UserStreaks { get; set; }
public DbSet<SeasonalEvent> SeasonalEvents { get; set; }
public DbSet<UserSeasonalEventProgress> UserSeasonalEventProgresses { get; set; }
public DbSet<AbuseReport> AbuseReports { get; set; }
```

### Background Jobs
Updated GamificationBackgroundService runs:
1. **Streak Reset**: Daily at midnight (per timezone)
2. **Seasonal Event Processing**: Check start/end conditions
3. **Abuse Detection Scan**: Hourly or on-demand
4. **Analytics Aggregation**: Daily/weekly

## Reward Formulas

### Task Completion Reward
```
BaseReward = 100
PriorityModifier = Low:0.5x, Normal:1x, High:1.5x, Critical:2x
ComplexityModifier = Simple:0.5x, Normal:1x, Complex:1.5x, Very Complex:2x
StreakBonus = min(CurrentStreak * 5, 100)
TimeBonus = If completed within 24h of creation: 20%, otherwise: 0%

TotalReward = BaseReward * PriorityModifier * ComplexityModifier 
            + StreakBonus + TimeBonus
```

### Project Completion Reward
```
ProjectReward = BaseReward * TaskCount * CompletionPercentage
              + (MilestoneBonus if all tasks completed)
```

### Sprint Completion Reward
```
SprintReward = BaseReward * (CompletedTasks / TotalTasks) * 3
             + (SprintBonus if all tasks completed)
```

### Seasonal Event Boost
```
EventReward = BaseReward * event.RewardBonusMultiplier
            + event.ExtraPointsPerCompletion
```

## Security & Anti-Abuse Strategy

### Prevention Layers
1. **Real-time Detection**: Abuse engine scans on each transaction
2. **Anomaly Detection**: Statistical analysis flags unusual patterns
3. **Audit Trail**: All transactions logged with metadata
4. **Admin Review**: Manual verification before penalties applied
5. **Reversibility**: All actions can be undone with proper authorization

### Penalty Framework
```
Severity | Threshold | Automatic Action | Manual Review
---------|-----------|-----------------|---------------
Low      | 0-30      | None            | Optional
Medium   | 31-60     | Flag             | Recommended
High     | 61-100    | Flag + Temp Hold | Required
Critical | >100      | Suspend Rewards  | Immediate
```

### Refund & Recovery
- Refunded points immediately returned to user
- Transactions logged with "Refunded" type
- Audit trail shows reason and reviewer
- Users can appeal through support

## API Endpoints

### Admin API

#### Metrics
```
GET /api/admin/gamification/metrics
GET /api/admin/gamification/daily-active-users?days=30
GET /api/admin/gamification/marketplace-metrics
```

#### Abuse Management
```
GET /api/admin/gamification/abuse-reports?status=Pending
GET /api/admin/gamification/abuse-reports/{id}
POST /api/admin/gamification/abuse-reports/{id}/resolve
```

#### User Management
```
GET /api/admin/gamification/users/{userId}/progression
GET /api/admin/gamification/top-users?limit=20
POST /api/admin/gamification/users/{userId}/refund-reward
POST /api/admin/gamification/users/{userId}/suspend-rewards
```

#### System Management
```
POST /api/admin/gamification/streaks/reset
POST /api/admin/gamification/seasonal-events
GET /api/admin/gamification/seasonal-events
```

## Configuration

### Streak Configuration
```csharp
// In appsettings.json
"Gamification": {
  "Streaks": {
    "MilestoneBonus3Days": 150,
    "MilestoneBonus7Days": 300,
    "MilestoneBonus14Days": 500,
    "MilestoneBonus30Days": 1000,
    "MilestoneBonus100Days": 5000
  },
  "AbuseDetection": {
    "RapidCompletionThreshold": 50,
    "SigmaThreshold": 5,
    "ScanIntervalMinutes": 60
  }
}
```

### Timezone Support
```csharp
// Supported timezones
TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time")
TimeZoneInfo.FindSystemTimeZoneById("Central European Time")
// ... IANA timezone identifiers
```

## Testing Strategy

### Unit Tests
- Streak calculation and milestone detection
- Reward formula calculations
- Abuse detection rule logic
- Analytics aggregation

### Integration Tests
- Full task-to-reward flow
- Event publishing and handling
- Database persistence
- Background job execution

### Performance Tests
- Leaderboard calculation (1M+ users)
- Abuse scan performance (large datasets)
- Concurrent reward processing
- Analytics query optimization

## Monitoring & Alerts

### Key Metrics to Monitor
- Abuse report volume (spike detection)
- Reward distribution anomalies
- User progression velocity
- System manipulation attempts
- Seasonal event participation

### Alert Thresholds
```
Critical:
  - >100 abuse reports/hour
  - >1M XP distributed in 5 minutes
  - >10% user base flagged for abuse

Warning:
  - >50 abuse reports/hour
  - >500K XP distributed in 5 minutes
  - >5% user base flagged for abuse
```

## Future Enhancements

1. **Machine Learning Classification**: Improved anomaly detection using historical patterns
2. **Behavioral Clustering**: Identify exploit patterns across user groups
3. **Dynamic Difficulty**: Adjust reward formulas based on user expertise
4. **Social Gamification**: Team streaks, collaborative challenges
5. **Temporal Analytics**: Identify peak engagement periods
6. **Predictive Churn**: Use progression data to identify at-risk users

## Maintenance & Operations

### Daily Tasks
- Monitor abuse report queue
- Check system health metrics
- Review failed transactions
- Verify streak resets completed

### Weekly Tasks
- Analyze economy trends
- Review milestone unlock patterns
- Validate analytics accuracy
- Performance optimization review

### Monthly Tasks
- Season event planning
- Reward formula adjustments
- User segment analysis
- Security audit

## Compliance & GDPR

- All personal data encrypted in transit
- Audit trail maintained for all actions
- Users can request data export (includes progression)
- Right to be forgotten: Data anonymization process
- Abuse reports stored securely with access controls

---

**Version**: 5.0  
**Last Updated**: 2026-08-29  
**Status**: Production Ready
