# Use the official .NET 8.0 runtime as base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Use the official .NET 8.0 SDK for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the project file and restore dependencies
COPY ["Web/Web.csproj", "Web/"]
RUN dotnet restore "Web/Web.csproj"

# Copy the rest of the application code
COPY Web/ Web/
WORKDIR "/src/Web"

# Build the application
RUN dotnet build "Web.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Create the final runtime image
FROM base AS final
WORKDIR /app

# Copy the published application
COPY --from=publish /app/publish .

# Create a directory for the SQLite database
RUN mkdir -p /app/data

# Set environment variables for production
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080

# Expose port 8080 (Render uses this port)
EXPOSE 8080

# Start the application
ENTRYPOINT ["dotnet", "Web.dll"]
