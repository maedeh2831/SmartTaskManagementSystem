using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartTask.Web.Common.Filters;
using SmartTask.Web.Data.Context;
using SmartTask.Web.Hubs;
using SmartTask.Web.Infrastructure.BackgroundJobs;
using SmartTask.Web.Infrastructure.Interfaces;
using SmartTask.Web.Infrastructure.Repositories;
using SmartTask.Web.Infrastructure.Seed;
using SmartTask.Web.Infrastructure.Services;
using SmartTask.Web.Models.Entities;
using SmartTask.Web.Services.Email;
using SmartTask.Web.Services.Files;
using SmartTask.Web.Services.Implementations;
using SmartTask.Web.Services.Interfaces;
using SmartTask.Web.Services.AI;

namespace SmartTask.Web
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection")));

            // ASP.NET Core Identity
            builder.Services
                .AddIdentity<ApplicationUser, IdentityRole<int>>(options =>
                {
                    options.Password.RequireDigit = true;
                    options.Password.RequiredLength = 6;
                    options.Password.RequireUppercase = false;
                    options.Password.RequireLowercase = false;
                    options.Password.RequireNonAlphanumeric = false;

                    options.User.RequireUniqueEmail = true;
                })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Account/Login";
                options.AccessDeniedPath = "/Account/AccessDenied";
            });

            // MVC
            //builder.Services
            //.AddAuthentication()
            //.AddGoogle(options =>
            //{
            //    options.ClientId = builder.Configuration["Authentication:Google:ClientId"]!;
            //    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;
            //});

            builder.Services
           .AddControllersWithViews();

            builder.Services.AddSignalR();

            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly", policy =>
                {
                    policy.RequireRole("Admin");
                });

                options.AddPolicy("ProjectManagerOnly", policy =>
                {
                    policy.RequireRole("ProjectManager");
                });

                options.AddPolicy("MemberOnly", policy =>
                {
                    policy.RequireRole("Member");
                });
            });

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Account/Login";
                options.AccessDeniedPath = "/Account/AccessDenied";
                options.LogoutPath = "/Account/Logout";

                options.Cookie.Name = "SmartTask";

                options.SlidingExpiration = true;

                options.ExpireTimeSpan = TimeSpan.FromDays(7);
            });

            builder.Services.AddHostedService<ReminderBackgroundService>();
            builder.Services.AddHostedService<OverdueCascadeBackgroundService>();

            // Repository
            builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Services
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
            builder.Services.AddScoped<IWorkspaceService, WorkspaceService>();
            builder.Services.AddScoped<IWorkspaceInvitationService, WorkspaceInvitationService>();
            builder.Services.AddScoped<IWorkspaceMemberService, WorkspaceMemberService>();
            builder.Services.AddScoped<IWorkspaceDashboardService, WorkspaceDashboardService>();
            builder.Services.AddScoped<IFileUploadService, SmartTask.Web.Services.Files.FileUploadService>();
            builder.Services.AddScoped<ITeamService, SmartTask.Web.Services.Implementations.TeamService>();
            builder.Services.AddScoped<ITeamMemberService, SmartTask.Web.Services.Implementations.TeamMemberService>();
            builder.Services.AddScoped<IProjectMemberService, ProjectMemberService>();
            builder.Services.AddScoped<IProjectDashboardService, ProjectDashboardService>();
            builder.Services.AddScoped<ICommentService, CommentService>();
            builder.Services.AddScoped<IAttachmentService, AttachmentService>();
            builder.Services.AddScoped<ILabelService, LabelService>();
            builder.Services.AddScoped<ITaskLabelService, TaskLabelService>();
            builder.Services.AddScoped<IChecklistService, ChecklistService>();
            builder.Services.AddScoped<ITimeLogService, TimeLogService>();
            builder.Services.AddScoped<INotificationService, NotificationService>();
            builder.Services.AddScoped<IReminderService, ReminderService>();
            builder.Services.AddScoped<IBacklogService, BacklogService>();
            builder.Services.AddScoped<IWorkspaceReportService, WorkspaceReportService>();
            builder.Services.AddScoped<IProjectReportService, ProjectReportService>();
            builder.Services.AddScoped<ISubTaskService, SubTaskService>();
            builder.Services.AddScoped<IActivityLogService, ActivityLogService>();
            builder.Services.AddScoped<IUserStoryService, UserStoryService>();
            builder.Services.AddScoped<ITaskAssignmentService, TaskAssignmentService>();
            builder.Services.AddScoped<IProjectTeamService, ProjectTeamService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IProjectService, ProjectService>();
            builder.Services.AddScoped<ISprintService, SprintService>();
            builder.Services.AddScoped<ITaskService, TaskService>();
            builder.Services.AddScoped<IWorkspaceReportService, WorkspaceReportService>();
            builder.Services.AddScoped<IProjectReportService, ProjectReportService>();
            builder.Services.AddScoped<IReportExportService, ReportExportService>();
            builder.Services.AddScoped<IUserDashboardService, UserDashboardService>();
            builder.Services.AddScoped<IAdminDashboardService, AdminDashboardService>();
            builder.Services.AddScoped<IOffroadTaskService, OffroadTaskService>();
            builder.Services.AddScoped<IWorkloadAnalysisService, WorkloadAnalysisService>();
            builder.Services.AddScoped<ITaskDependencyService, TaskDependencyService>();
            builder.Services.AddScoped<IPriorityEngineService, PriorityEngineService>();
            builder.Services.AddScoped<ITaskBreakdownService, TaskBreakdownService>();
            builder.Services.AddScoped<ICurrentContextService, CurrentContextService>();
            builder.Services.AddScoped<IDelayRiskService, DelayRiskService>();
            builder.Services.AddScoped<IProjectHealthService, ProjectHealthService>();
            builder.Services.AddScoped<ISprintReportAiService, SprintReportAiService>();
            builder.Services.AddScoped<ITaskTradeService, TaskTradeService>();
            builder.Services.AddScoped<ISettingsService, SettingsService>();
            builder.Services.AddScoped<IUserSessionTracker, UserSessionTracker>();
            builder.Services.AddScoped<IDateFormatService, DateFormatService>();
            builder.Services.AddScoped<IChatService, ChatService>();
            builder.Services.AddScoped<IWebpushrService, WebpushrService>();

            // ردیابی حضور کاربران در حافظه؛ باید Singleton باشد.
            builder.Services.AddSingleton<IPresenceTracker, PresenceTracker>();

            builder.Services.Configure<EmailSettings>(
            builder.Configuration.GetSection("EmailSettings"));
            builder.Services.AddTransient<IEmailService, EmailService>();
            builder.Services.Configure<OpenAiSettings>(builder.Configuration.GetSection("OpenAI"));
            builder.Services.AddHttpClient<IAiClientService, AiClientService>();         
            builder.Services.AddHttpClient<IAiClientService, AiClientService>(client =>
            {
                client.Timeout = TimeSpan.FromMinutes(5);
            }); builder.Services.AddScoped<ITaskBreakdownService, TaskBreakdownService>();

            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromHours(2);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });
            builder.Services.AddControllersWithViews(options =>
            {
                options.Filters.Add<CurrentContextFilter>();
            });

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseRouting();

            app.UseSession();

            app.UseAuthentication();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.MapHub<NotificationHub>("/hubs/notification");

            app.MapHub<ChatHub>("/hubs/chat");

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;

                var roleManager =
                    services.GetRequiredService<RoleManager<IdentityRole<int>>>();

                var userManager =
                    services.GetRequiredService<UserManager<ApplicationUser>>();

                // create Rols
                await RoleSeeder.SeedAsync(roleManager);

                // create Admin
                await AdminSeeder.SeedAsync(userManager);
            }

            app.Run();
        }
    }
}