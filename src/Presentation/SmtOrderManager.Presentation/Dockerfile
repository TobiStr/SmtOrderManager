# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# copy csproj and restore
COPY SmtOrderManager.sln .
COPY src/Presentation/SmtOrderManager.Presentation/SmtOrderManager.Presentation.csproj src/Presentation/SmtOrderManager.Presentation/
COPY src/Application/SmtOrderManager.Application/SmtOrderManager.Application.csproj src/Application/SmtOrderManager.Application/
COPY src/Domain/SmtOrderManager.Domain/SmtOrderManager.Domain.csproj src/Domain/SmtOrderManager.Domain/
COPY src/Infrastructure/SmtOrderManager.Infrastructure/SmtOrderManager.Infrastructure.csproj src/Infrastructure/SmtOrderManager.Infrastructure/
RUN dotnet restore src/Presentation/SmtOrderManager.Presentation/SmtOrderManager.Presentation.csproj

# copy source
COPY src ./src

RUN dotnet publish src/Presentation/SmtOrderManager.Presentation/SmtOrderManager.Presentation.csproj -c Release -o /app/publish

# runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "SmtOrderManager.Presentation.dll"]
