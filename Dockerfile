FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files
COPY ["DainnUserManagement/DainnUserManagement.csproj", "./DainnUserManagement/"]
COPY ["DainnUserManagement.API/DainnUserManagement.API.csproj", "./DainnUserManagement.API/"]

# Copy source files (exclude Tests, bin, obj via .dockerignore)
COPY ["DainnUserManagement/", "./DainnUserManagement/"]
COPY ["DainnUserManagement.API/", "./DainnUserManagement.API/"]

# Remove any obj/bin folders that might have been copied
RUN find /src -type d -name "obj" -exec rm -rf {} + 2>/dev/null || true && \
    find /src -type d -name "bin" -exec rm -rf {} + 2>/dev/null || true

# Restore dependencies for the API project
WORKDIR "/src/DainnUserManagement.API"
RUN dotnet restore "DainnUserManagement.API.csproj"

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
WORKDIR "/src/DainnUserManagement.API"
# Publish directly (this will build and publish in one step)
RUN dotnet publish "DainnUserManagement.API.csproj" -c $BUILD_CONFIGURATION -p:GeneratePackageOnBuild=false -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app

# Install curl for healthchecks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

COPY --from=publish /app/publish .

# Create data directory (optional, for any file-based storage)
RUN mkdir -p /app/data

ENTRYPOINT ["dotnet", "DainnUserManagement.API.dll"]

