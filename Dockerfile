# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy solution and project files
COPY OrderProcessingService.sln .
COPY src/OrderProcessingService.Api/OrderProcessingService.csproj src/OrderProcessingService.Api/
COPY tests/OrderProcessingService.Tests/OrderProcessingServiceTests.csproj tests/OrderProcessingService.Tests/

# Restore dependencies
RUN dotnet restore

# Copy all source files
COPY . .

# Build and publish
WORKDIR /app/src/OrderProcessingService.Api
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
ENTRYPOINT ["dotnet", "OrderProcessingService.Api.dll"]
