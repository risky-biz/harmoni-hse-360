# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Install Node.js for React build
RUN curl -fsSL https://deb.nodesource.com/setup_20.x | bash - && \
    apt-get install -y nodejs

# Copy csproj files and restore
COPY ["src/HarmoniHSE360.Web/HarmoniHSE360.Web.csproj", "HarmoniHSE360.Web/"]
COPY ["src/HarmoniHSE360.Application/HarmoniHSE360.Application.csproj", "HarmoniHSE360.Application/"]
COPY ["src/HarmoniHSE360.Domain/HarmoniHSE360.Domain.csproj", "HarmoniHSE360.Domain/"]
COPY ["src/HarmoniHSE360.Infrastructure/HarmoniHSE360.Infrastructure.csproj", "HarmoniHSE360.Infrastructure/"]
RUN dotnet restore "HarmoniHSE360.Web/HarmoniHSE360.Web.csproj"

# Copy everything else
COPY src/ .

# Build React app
WORKDIR /src/HarmoniHSE360.Web/ClientApp
RUN npm ci
RUN npm run build

# Build .NET app
WORKDIR /src/HarmoniHSE360.Web
RUN dotnet build "HarmoniHSE360.Web.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "HarmoniHSE360.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Install cultures for globalization
RUN apt-get update && apt-get install -y locales

# Create non-root user
RUN groupadd -r appgroup && useradd -r -g appgroup appuser

# Copy published files
COPY --from=publish /app/publish .

# Create uploads directory and set permissions
RUN mkdir -p uploads && chown -R appuser:appgroup uploads

# Set user
USER appuser

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "HarmoniHSE360.Web.dll"]