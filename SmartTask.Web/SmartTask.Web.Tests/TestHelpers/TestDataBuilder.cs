using SmartTask.Web.Data.Context;
using SmartTask.Web.Models.Entities;
using SmartTask.Web.Models.Enums;

namespace SmartTask.Web.Tests.TestHelpers;

/// <summary>
/// Fluent helper that seeds realistic entities into the in-memory context.
/// Tracks created ids so tests can reference them without magic numbers.
/// </summary>
public class TestDataBuilder
{
    private readonly ApplicationDbContext _context;
    private int _nextId = 1;

    public int WorkspaceId { get; private set; }
    public int OwnerUserId { get; private set; }
    public int MemberUserId { get; private set; }
    public int ProjectId { get; private set; }
    public int BacklogId { get; private set; }
    public int UserStoryId { get; private set; }
    public int SprintId { get; private set; }
    public int TaskId { get; private set; }

    public ApplicationDbContext Context => _context;

    public TestDataBuilder(ApplicationDbContext context)
    {
        _context = context;
    }

    public TestDataBuilder SeedBase()
    {
        OwnerUserId = _nextId++;
        MemberUserId = _nextId++;

        var owner = new ApplicationUser
        {
            Id = OwnerUserId,
            UserName = "owner@test.com",
            Email = "owner@test.com",
            FirstName = "Owner",
            LastName = "Test",
            PasswordHash = "Test-Password-Hash",
            EmailConfirmed = true
        };

        var member = new ApplicationUser
        {
            Id = MemberUserId,
            UserName = "member@test.com",
            Email = "member@test.com",
            FirstName = "Member",
            LastName = "Test",
            PasswordHash = "Test-Password-Hash",
            EmailConfirmed = true
        };

        _context.Users.AddRange(owner, member);

        WorkspaceId = _nextId++;
        var workspace = new Workspace
        {
            Id = WorkspaceId,
            Name = "Test Workspace",
            OwnerId = OwnerUserId,
            Owner = owner,
            ViewState = true
        };
        _context.Workspaces.Add(workspace);

        _context.WorkspaceMembers.AddRange(
            new WorkspaceMember
            {
                Id = _nextId++,
                WorkspaceId = WorkspaceId,
                Workspace = workspace,
                ApplicationUserId = OwnerUserId,
                ApplicationUser = owner,
                Role = WorkspaceRoleType.Owner,
                ViewState = true
            },
            new WorkspaceMember
            {
                Id = _nextId++,
                WorkspaceId = WorkspaceId,
                Workspace = workspace,
                ApplicationUserId = MemberUserId,
                ApplicationUser = member,
                Role = WorkspaceRoleType.Developer,
                ViewState = true
            });

        ProjectId = _nextId++;
        var project = new Project
        {
            Id = ProjectId,
            WorkspaceId = WorkspaceId,
            Name = "Test Project",
            Key = "TP",
            Workspace = workspace,
            ViewState = true
        };
        _context.Projects.Add(project);

        BacklogId = _nextId++;
        var backlog = new Backlog
        {
            Id = BacklogId,
            ProjectId = ProjectId,
            Name = "Product Backlog",
            Project = project,
            ViewState = true
        };
        _context.Backlogs.Add(backlog);

        UserStoryId = _nextId++;
        var story = new UserStory
        {
            Id = UserStoryId,
            ProjectId = ProjectId,
            BacklogId = BacklogId,
            Title = "Sample story",
            Project = project,
            Backlog = backlog,
            ViewState = true
        };
        _context.UserStories.Add(story);

        SprintId = _nextId++;
        var sprint = new Sprint
        {
            Id = SprintId,
            ProjectId = ProjectId,
            Name = "Test Sprint",
            Status = SprintStatusType.Active,
            StartDate = DateTime.Today.AddDays(-7),
            EndDate = DateTime.Today.AddDays(7),
            Capacity = 40,
            Project = project,
            ViewState = true
        };
        _context.Sprints.Add(sprint);

        TaskId = _nextId++;
        var task = new TaskItem
        {
            Id = TaskId,
            UserStoryId = UserStoryId,
            Title = "Sample task",
            Status = TaskStatusType.ToDo,
            Priority = TaskPriorityType.Medium,
            UserStory = story,
            ViewState = true
        };
        _context.TaskItems.Add(task);

        return this;
    }

    public TestDataBuilder WithProjectMember(int userId, ProjectRoleType role)
    {
        _context.ProjectMembers.Add(new ProjectMember
        {
            Id = _nextId++,
            ProjectId = ProjectId,
            ApplicationUserId = userId,
            Role = role,
            WeeklyCapacityHours = 40,
            ViewState = true
        });

        return this;
    }

    public TestDataBuilder WithTask(
        Action<TaskItem> configure,
        out int taskId)
    {
        taskId = _nextId++;
        var task = new TaskItem
        {
            Id = taskId,
            UserStoryId = UserStoryId,
            Title = $"Task {taskId}",
            Status = TaskStatusType.ToDo,
            Priority = TaskPriorityType.Medium,
            ViewState = true
        };

        configure(task);
        _context.TaskItems.Add(task);

        return this;
    }

    public TestDataBuilder WithDependency(int taskItemId, int dependsOnTaskId, bool isRequired)
    {
        _context.TaskDependencies.Add(new TaskDependency
        {
            Id = _nextId++,
            TaskItemId = taskItemId,
            DependsOnTaskItemId = dependsOnTaskId,
            IsRequired = isRequired,
            ViewState = true
        });

        return this;
    }
}
