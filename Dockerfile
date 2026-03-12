# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy solution and project files
COPY OrderProcessingApp.sln .
COPY src/OrderProcessingApp.Api/OrderProcessingApp.Api.csproj src/OrderProcessingApp.Api/
COPY tests/OrderProcessingApp.Tests/OrderProcessingApp.Tests.csproj tests/OrderProcessingApp.Tests/

# Restore dependencies
RUN dotnet restore

# Copy all source files
COPY . .

# Build and publish
WORKDIR /app/src/OrderProcessingApp.Api
RUN dotnet publish -c Release -o /app/out

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Copy published output from build stage
COPY --from=build /app/out .

# Expose port
EXPOSE 5000

# Set environment variables
ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production

# Run the application
ENTRYPOINT ["dotnet", "OrderProcessingApp.Api.dll"]
