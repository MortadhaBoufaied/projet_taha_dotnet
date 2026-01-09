using AiClientManager.Web.Auth;
using AiClientManager.Web.Models;
using AiClientManager.Web.Services;
using AspNetCore.Identity.MongoDbCore.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://localhost:5000");

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddOptions<MongoSettings>()
    .Bind(builder.Configuration.GetSection("Mongo"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddOptions<OpenAiSettings>()
    .Bind(builder.Configuration.GetSection("OpenAI"));

builder.Services.AddSingleton<MongoContext>();
builder.Services.AddSingleton<ClientRepository>();
builder.Services.AddSingleton<CvFileService>();
builder.Services.AddSingleton<AiAnalysisService>();
builder.Services.AddSingleton<DashboardService>();
builder.Services.AddHostedService<SeedHostedService>();

// Identity (MongoDB)
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
    {
        options.Password.RequireDigit = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 6;
        options.User.RequireUniqueEmail = true;
    })
    .AddMongoDbStores<ApplicationUser, ApplicationRole, Guid>(
        builder.Configuration.GetSection("Mongo").GetValue<string>("ConnectionString") ?? "mongodb://localhost:27017",
        builder.Configuration.GetSection("Mongo").GetValue<string>("DatabaseName") ?? "AiClientManagerDb")
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Health check for MongoDB
app.MapGet("/health/db", async (MongoContext mongo, CancellationToken ct) =>
{
    var ok = await mongo.PingAsync(ct);
    return ok ? Results.Ok(new { status = "ok" }) : Results.Problem("MongoDB not reachable");
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Dashboard}/{id?}");

app.MapRazorPages();
app.Run();
