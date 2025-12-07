using Serilog;
using SmtOrderManager.Presentation.Components;
using SmtOrderManager.Presentation;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using SmtOrderManager.Application.Features.Users.Commands.LoginUser;
using SmtOrderManager.Presentation.Models.DTOs;
using MediatR;
using SmtOrderManager.Presentation.Services;

var builder = DependencyInjection.CreateBuilder(args);

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
app.UseAuthentication();
app.UseAuthorization();
app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapPost("/auth/login", async (
    LoginRequest request,
    IMediator mediator,
    HttpContext httpContext) =>
{
    var result = await mediator.Send(new LoginUserCommand(request.Email, request.Password));
    if (!result.Success)
    {
        return Results.Unauthorized();
    }

    var user = result.GetOk();
    var claims = new List<Claim>
    {
        new(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new(ClaimTypes.Name, user.Name),
        new(ClaimTypes.Email, user.Email)
    };

    var claimsIdentity = new ClaimsIdentity(claims, CustomAuthenticationStateProvider.Scheme);
    var authProperties = new AuthenticationProperties
    {
        IsPersistent = request.RememberMe,
        AllowRefresh = true
    };

    await httpContext.SignInAsync(
        CustomAuthenticationStateProvider.Scheme,
        new ClaimsPrincipal(claimsIdentity),
        authProperties);

    var userDto = new UserDto
    {
        Id = user.Id,
        Email = user.Email,
        Name = user.Name,
        LastLoginAt = user.LastLoginAt,
        CreatedAt = user.CreatedAt
    };

    return Results.Ok(userDto);
}).AllowAnonymous().DisableAntiforgery();

app.MapPost("/auth/logout", async (HttpContext httpContext) =>
{
    await httpContext.SignOutAsync(CustomAuthenticationStateProvider.Scheme);
    return Results.Ok();
}).AllowAnonymous().DisableAntiforgery();

app.MapGet("/auth/logout", async (HttpContext httpContext) =>
{
    await httpContext.SignOutAsync(CustomAuthenticationStateProvider.Scheme);
    return Results.Redirect("/login");
}).AllowAnonymous().DisableAntiforgery();

app.Run();

internal sealed record LoginRequest(string Email, string Password, bool RememberMe);

