using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Zilor.AICopilot.EntityFrameworkCore;

namespace Zilor.AICopilot.MigrationWorkApp;

public class Worker(
    IServiceProvider serviceProvider,
    IHostApplicationLifetime hostApplicationLifetime) : BackgroundService
{
    public const string ActivitySourceName = "Migrations";
    private static readonly ActivitySource ActivitySource = new(ActivitySourceName);

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        using var activity = ActivitySource.StartActivity("Migrating database", ActivityKind.Client);

        try
        {
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AiCopilotDbContext>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
            await RunMigrationAsync(dbContext, cancellationToken);
            await SeedDataAsync(roleManager, userManager, cancellationToken);
        }
        catch (Exception ex)
        {
            activity?.AddException(ex);
            throw;
        }

        hostApplicationLifetime.StopApplication();
    }

    private static async Task RunMigrationAsync(AiCopilotDbContext dbContext, CancellationToken cancellationToken)
    {
        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () => { await dbContext.Database.MigrateAsync(cancellationToken); });
    }

    private static async Task SeedDataAsync(
        RoleManager<IdentityRole> roleManager, 
        UserManager<IdentityUser> userManager,
        CancellationToken cancellationToken)
    {
        // 创建默认角色
        var roles = new[] { "Admin", "User" };

        foreach (var role in roles)
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));

        // 创建默认管理员账户
        const string adminUserName = "admin";
        const string adminPassword = "Admin123!";

        var adminUser = await userManager.FindByNameAsync(adminUserName);
        if (adminUser == null)
        {
            adminUser = new IdentityUser
            {
                UserName = adminUserName
            };

            var result = await userManager.CreateAsync(adminUser, adminPassword);
            if (result.Succeeded)
                await userManager.AddToRoleAsync(adminUser, "Admin");
            else
                Console.WriteLine("创建管理员失败：" + string.Join(",", result.Errors.Select(e => e.Description)));
        }
    }
}