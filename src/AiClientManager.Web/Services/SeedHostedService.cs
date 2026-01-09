using AiClientManager.Web.Auth;
using Microsoft.AspNetCore.Identity;

namespace AiClientManager.Web.Services;

public sealed class SeedHostedService : IHostedService
{
    private readonly IServiceProvider _sp;
    private readonly IConfiguration _cfg;

    public SeedHostedService(IServiceProvider sp, IConfiguration cfg)
    {
        _sp = sp;
        _cfg = cfg;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _sp.CreateScope();
        var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        // Roles
        foreach (var role in new[] { "Admin", "User" })
        {
            if (!await roleMgr.RoleExistsAsync(role))
            {
                await roleMgr.CreateAsync(new ApplicationRole(role));
            }
        }

        // Admin seed
        var email = _cfg.GetSection("Seed").GetValue<string>("AdminEmail") ?? "admin@example.com";
        var pwd = _cfg.GetSection("Seed").GetValue<string>("AdminPassword") ?? "ChangeMe123!";

        var existing = await userMgr.FindByEmailAsync(email);
        if (existing is null)
        {
            var user = new ApplicationUser(email, email)
            {
                Email = email,
                UserName = email,
                EmailConfirmed = true
            };
            var result = await userMgr.CreateAsync(user, pwd);
            if (result.Succeeded)
            {
                await userMgr.AddToRoleAsync(user, "Admin");
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
