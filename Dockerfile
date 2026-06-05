# ── Stage 1 : build ──────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy every .csproj first so Docker can cache the restore layer
COPY Tools/Tools.csproj                           Tools/
COPY Domain/Domain.csproj                         Domain/
COPY Application/Application.csproj               Application/
COPY Infrastructure/Infrastructure.csproj         Infrastructure/
COPY Api/Api.csproj                               Api/

RUN dotnet restore Api/Api.csproj

# Copy the rest of the source
COPY Tools/        Tools/
COPY Domain/       Domain/
COPY Application/  Application/
COPY Infrastructure/ Infrastructure/
COPY Api/          Api/

WORKDIR /src/Api
RUN dotnet publish Api.csproj \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

# ── Stage 2 : runtime ────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app

# Non-root user (already defined in the base image as UID 1654)
USER $APP_UID

COPY --from=build /app/publish .

# SQLite data directory — mount a volume here to persist the database
VOLUME ["/app/data"]

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Api.dll"]