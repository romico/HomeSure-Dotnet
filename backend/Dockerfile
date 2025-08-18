# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy solution file and project files
COPY RealEstate.sln .
COPY RealEstate.API/*.csproj ./RealEstate.API/
COPY RealEstate.Core/*.csproj ./RealEstate.Core/
COPY RealEstate.Infrastructure/*.csproj ./RealEstate.Infrastructure/
COPY RealEstate.Tests/*.csproj ./RealEstate.Tests/

# Restore dependencies
RUN dotnet restore

# Copy everything else and build
COPY . .
WORKDIR /app/RealEstate.API
RUN dotnet build -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

COPY --from=publish /app/publish .

# Create logs directory
RUN mkdir -p /app/logs

# Expose ports
EXPOSE 8080 8081

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "RealEstate.API.dll"]
