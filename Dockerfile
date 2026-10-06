# Gulora API — multi-stage build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY backend/Gulora.sln ./
COPY backend/src/Gulora.Domain/Gulora.Domain.csproj backend/src/Gulora.Domain/
COPY backend/src/Gulora.Application/Gulora.Application.csproj backend/src/Gulora.Application/
COPY backend/src/Gulora.Infrastructure/Gulora.Infrastructure.csproj backend/src/Gulora.Infrastructure/
COPY backend/src/Gulora.Contracts/Gulora.Contracts.csproj backend/src/Gulora.Contracts/
COPY backend/src/Gulora.API/Gulora.API.csproj backend/src/Gulora.API/

RUN dotnet restore backend/src/Gulora.API/Gulora.API.csproj

COPY backend/ ./backend/
RUN dotnet publish backend/src/Gulora.API/Gulora.API.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

COPY --from=build /app/publish .

EXPOSE 8080
HEALTHCHECK --interval=30s --timeout=5s --start-period=20s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "Gulora.API.dll"]
