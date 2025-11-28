# Development Dockerfile
# Optimized for hot reload, debugging, and fast iteration

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS development

# Install debugger
RUN apt-get update && apt-get install -y unzip && \
    curl -sSL https://aka.ms/getvsdbgsh | bash /dev/stdin -v latest -l /vsdbg

WORKDIR /app

# Expose ports
# 8080: Application HTTP port
EXPOSE 8080

# Copy project files for restore
COPY ["Directory.Packages.props", "./"]
COPY ["Library/Library.csproj", "Library/"]

# Restore dependencies
RUN dotnet restore "Library/Library.csproj"

# Copy everything else
COPY . .

# Set working directory to project
WORKDIR /app/Library

# Development environment variables
ENV ASPNETCORE_ENVIRONMENT=Development
ENV DOTNET_USE_POLLING_FILE_WATCHER=true
ENV ASPNETCORE_URLS=http://+:8080

# Start with watch for hot reload
ENTRYPOINT ["dotnet", "watch", "run", "--no-restore", "--urls", "http://0.0.0.0:8080"]
