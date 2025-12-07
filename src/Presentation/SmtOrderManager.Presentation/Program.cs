using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Serilog;
using SmtOrderManager.Application.Features.Users.Commands.RegisterUser;
using SmtOrderManager.Application.Interfaces;
using SmtOrderManager.Domain.Entities;
using SmtOrderManager.Domain.Repositories;
using SmtOrderManager.Domain.Services;
using SmtOrderManager.Infrastructure.BlobStorage;
using SmtOrderManager.Infrastructure.CosmosDb;
using SmtOrderManager.Infrastructure.Services;
using SmtOrderManager.Presentation.Components;
using SmtOrderManager.Presentation.Services;
using SmtOrderManager.Presentation.ViewModels;
using User = SmtOrderManager.Domain.Entities.User;

var builder = WebApplication.CreateBuilder(args);

// Load environment variables from a local .env (for developer convenience) and the host environment
LoadDotEnv(builder.Environment.ContentRootPath);
builder.Configuration.AddEnvironmentVariables();

builder.Host.UseSerilog((context, services, loggerConfiguration) =>
{
    loggerConfiguration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext();
});

AddPresentationServices(builder);
AddApplicationServices(builder);
AddInfrastructureServices(builder);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseSerilogRequestLogging();
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();
app.UseAntiforgery();
app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

static void AddPresentationServices(WebApplicationBuilder builder)
{
    builder.Services.AddRazorComponents()
        .AddInteractiveServerComponents();

    builder.Services.AddHttpContextAccessor();

    builder.Services.AddAuthentication();
    builder.Services.AddAuthorization();
    builder.Services.AddCascadingAuthenticationState();

    builder.Services.AddScoped<CustomAuthenticationStateProvider>();
    builder.Services.AddScoped<AuthenticationStateProvider>(
        sp => sp.GetRequiredService<CustomAuthenticationStateProvider>());

    builder.Services.AddScoped<LoginViewModel>();
    builder.Services.AddScoped<RegisterViewModel>();
    builder.Services.AddScoped<ComponentListViewModel>();
    builder.Services.AddScoped<ComponentDetailViewModel>();
    builder.Services.AddScoped<ComponentCreateViewModel>();
    builder.Services.AddScoped<BoardListViewModel>();
    builder.Services.AddScoped<BoardDetailViewModel>();
    builder.Services.AddScoped<BoardCreateViewModel>();
    builder.Services.AddScoped<OrderListViewModel>();
    builder.Services.AddScoped<OrderDetailViewModel>();
    builder.Services.AddScoped<OrderCreateViewModel>();
    builder.Services.AddScoped<UserProfileViewModel>();
    builder.Services.AddScoped<HomeViewModel>();
}

static void AddApplicationServices(WebApplicationBuilder builder)
{
    builder.Services.AddMediatR(cfg =>
        cfg.RegisterServicesFromAssembly(typeof(RegisterUserCommand).Assembly));
}

static void AddInfrastructureServices(WebApplicationBuilder builder)
{
    builder.Services.Configure<CosmosDbOptions>(
        builder.Configuration.GetSection("CosmosDb"));

    builder.Services.AddSingleton(sp =>
    {
        var options = sp.GetRequiredService<IOptions<CosmosDbOptions>>().Value;
        return new CosmosClient(options.ConnectionString);
    });

    builder.Services.AddScoped<IUserRepository, UserRepository>();
    builder.Services.AddScoped<IComponentRepository, ComponentRepository>();
    builder.Services.AddScoped<IBoardRepository, BoardRepository>();
    builder.Services.AddScoped<IOrderRepository, OrderRepository>();

    builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

    builder.Services.Configure<BlobStorageOptions>(
        builder.Configuration.GetSection("BlobStorage"));
    builder.Services.AddScoped<IBlobStorageService, BlobStorageService>();

    builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
}

static void LoadDotEnv(string contentRootPath)
{
    var envFilePath = Path.Combine(contentRootPath, ".env");
    if (!File.Exists(envFilePath))
    {
        return;
    }

    foreach (var rawLine in File.ReadAllLines(envFilePath))
    {
        var line = rawLine.Trim();

        if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#", StringComparison.Ordinal))
        {
            continue;
        }

        var separatorIndex = line.IndexOf('=', StringComparison.Ordinal);
        if (separatorIndex <= 0)
        {
            continue;
        }

        var key = line[..separatorIndex].Trim();
        var value = line[(separatorIndex + 1)..].Trim().Trim('"');

        if (!string.IsNullOrWhiteSpace(key))
        {
            Environment.SetEnvironmentVariable(key, value);
        }
    }
}
