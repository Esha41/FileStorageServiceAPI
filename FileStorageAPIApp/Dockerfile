# Use the official .NET 8.0 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution file and restore dependencies
COPY ["FileStorage.API/FileStorage.API.csproj", "FileStorage.API/"]
COPY ["FileStorage.Application/FileStorage.Application.csproj", "FileStorage.Application/"]
COPY ["FileStorage.Domain/FileStorage.Domain.csproj", "FileStorage.Domain/"]
COPY ["FileStorage.Infrastructure/FileStorage.Infrastructure.csproj", "FileStorage.Infrastructure/"]

RUN dotnet restore "FileStorage.API/FileStorage.API.csproj"

# Copy the rest of the source code
COPY . .

# Build the application
WORKDIR "/src/FileStorage.API"
RUN dotnet build "FileStorage.API.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "FileStorage.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Use the official .NET 8.0 runtime image for running
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Create directory for storage and logs with proper permissions
RUN mkdir -p /app/_storage /app/Logs && \
    chmod -R 755 /app/_storage /app/Logs

# Copy published application
COPY --from=publish /app/publish .

# Expose the port the app runs on
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Run the application
ENTRYPOINT ["dotnet", "FileStorage.API.dll"]

