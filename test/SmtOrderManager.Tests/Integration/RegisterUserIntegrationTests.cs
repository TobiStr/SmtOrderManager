using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SmtOrderManager.Application.Features.Users.Commands.RegisterUser;
using SmtOrderManager.Domain.Repositories;

namespace SmtOrderManager.Tests.Integration;

public class RegisterUserIntegrationTests
{
    [IntegrationFact]
    public async Task RegisterUserCommand_PersistsUser_WhenUserDoesNotExist()
    {
        var serviceProvider = IntegrationTestSetup.CreateServiceProvider();

        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var command = new RegisterUserCommand("integration@test.com", "Integration Tester", "Sup3rSecret!");

        var result = await mediator.Send(command);

        Assert.True(result.Success);

        var repository = serviceProvider.GetRequiredService<IUserRepository>();
        var storedUser = await repository.GetByEmailAsync(command.Email);

        Assert.True(storedUser.Success);
        var user = storedUser.GetOk();
        Assert.Equal(command.Email.ToLowerInvariant(), user.Email);
        Assert.Equal(command.Name, user.Name);
        Assert.False(string.IsNullOrWhiteSpace(user.PasswordHash));
    }
}
