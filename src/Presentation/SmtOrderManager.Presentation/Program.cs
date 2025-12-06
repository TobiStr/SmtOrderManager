using MediatR;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using SmtOrderManager.Application.Features.Users.Commands.RegisterUser;
using SmtOrderManager.Application.Interfaces;
using SmtOrderManager.Domain.Repositories;
using SmtOrderManager.Infrastructure.CosmosDb;
using SmtOrderManager.Infrastructure.Services;
using SmtOrderManager.Presentation.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// HTTP Context Accessor (for ICurrentUserService)
builder.Services.AddHttpContextAccessor();

// MediatR (Application Layer)
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(RegisterUserCommand).Assembly));

// CosmosDB Configuration
builder.Services.Configure<CosmosDbOptions>(
    builder.Configuration.GetSection("CosmosDb"));

builder.Services.AddSingleton(sp =>
{
    var options = sp.GetRequiredService<IOptions<CosmosDbOptions>>().Value;
    return new CosmosClient(options.ConnectionString);
});

// Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IComponentRepository, ComponentRepository>();
builder.Services.AddScoped<IBoardRepository, BoardRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

// Services
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// Password Hasher (for User authentication)
builder.Services.AddScoped<Microsoft.AspNetCore.Identity.IPasswordHasher<SmtOrderManager.Domain.Entities.User>,
    Microsoft.AspNetCore.Identity.PasswordHasher<SmtOrderManager.Domain.Entities.User>>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();
app.UseAntiforgery();
app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
