# ─────────────────────────────────────────────
# Stage 1 : Build
# ─────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copie des .csproj pour layer-cache NuGet restore
COPY Api/Api.csproj                               Api/
COPY Api.IntegrationTests/Api.IntegrationTests.csproj Api.IntegrationTests/
COPY Application/Application.csproj               Application/
COPY Domain/Domain.csproj                         Domain/
COPY Infrastructure/Infrastructure.csproj         Infrastructure/
COPY UnitTests/UnitTests.csproj                   UnitTests/
COPY Tools/Tools.csproj                           Tools/
COPY CynaApi.sln                                  ./

RUN dotnet restore CynaApi.sln

# Copie du reste du code source
COPY . .

# Publish de l'API en Release
RUN dotnet publish Api/Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# ─────────────────────────────────────────────
# Stage 2 : Runtime
# ─────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Utilisation de l'utilisateur non-root par défaut de .NET
USER app

COPY --from=build /app/publish .

# Port exposé (ASP.NET Core écoute sur 8080 par convention dans les conteneurs)
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Api.dll"]