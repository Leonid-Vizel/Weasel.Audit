using Microsoft.EntityFrameworkCore;
using Weasel.Audit.Example;
using Weasel.Audit.DependencyInjection.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
string? connectionString = builder.Environment.EnvironmentName switch
{
    "Docker" => builder.Configuration.GetConnectionString("DockerConnection"),
    "Production" => builder.Configuration.GetConnectionString("DeployConnection"),
    _ => builder.Configuration.GetConnectionString("DefaultConnection"),
};
builder.Services.AddDbContext<MainDbContext>(options =>
{
    options.UseNpgsql(connectionString, npgsql =>
    {
        npgsql.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
    });
});

builder.Services.AddAudit<MainDbContext, AuditActionFactory, AuditAction, AuditType, AuditColor>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
