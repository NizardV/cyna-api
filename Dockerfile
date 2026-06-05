# ── Stage 1 : build ──────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Tools/Tools.csproj                   Tools/
COPY Domain/Domain.csproj                 Domain/
COPY Application/Application.csproj       Application/
COPY Infrastructure/Infrastructure.csproj Infrastructure/
COPY Api/Api.csproj                       Api/

RUN dotnet restore Api/Api.csproj

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

# Create data dir and give ownership to the non-root app user before switching
RUN mkdir -p /app/data && chown -R $APP_UID /app/data

COPY --from=build /app/publish .

VOLUME ["/app/data"]

ENV ASPNETCORE_URLS=http://+:8080

USER $APP_UID

EXPOSE 8080

ENTRYPOINT ["dotnet", "Api.dll"]