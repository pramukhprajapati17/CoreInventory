# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files first for better layer caching
COPY CoreInventory.sln ./
COPY MVC/MVC.csproj MVC/
COPY Repositories/Repositories.csproj Repositories/

RUN dotnet restore CoreInventory.sln

# Copy the rest of the source
COPY . ./

WORKDIR /src/MVC
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish ./

# Render expects the app to bind to $PORT on 0.0.0.0
EXPOSE 10000

# Use shell to expand $PORT at runtime (Render sets PORT, default 10000)
ENTRYPOINT ["/bin/sh", "-c", "dotnet MVC.dll --urls http://0.0.0.0:${PORT:-10000}"]
