# Use the official .NET 8.0 runtime as base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Use the official .NET 8.0 SDK for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the project file and restore dependencies
COPY ["Web.csproj", "."]
RUN dotnet restore "Web.csproj"

# Copy the rest of the application code
COPY . .

# Build the application
RUN dotnet build "Web.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Create the final runtime image
FROM base AS final
WORKDIR /app

# Install Python for running the dummy data script
RUN apt-get update && apt-get install -y python3 python3-pip && rm -rf /var/lib/apt/lists/*

# Copy the published application
COPY --from=publish /app/publish .

# Copy the dummy data script
COPY insert_complete_dummy_data.py .

# Create a directory for the SQLite database
RUN mkdir -p /app/data

# Set environment variables for production
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080

# Expose port 8080 (Render uses this port)
EXPOSE 8080

# Create a startup script that starts the app, then populates dummy data
RUN echo '#!/bin/bash\n\
echo "Starting application..."\n\
dotnet Web.dll &\n\
APP_PID=$!\n\
echo "Waiting for application to start and create database..."\n\
sleep 15\n\
echo "Populating database with dummy data..."\n\
python3 insert_complete_dummy_data.py\n\
echo "Dummy data populated. Application is running."\n\
wait $APP_PID' > /app/start.sh && chmod +x /app/start.sh

# Start the application
ENTRYPOINT ["/app/start.sh"]