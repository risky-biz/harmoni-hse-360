# Elsa Studio Blazor WASM Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Harmoni360.ElsaStudio/Harmoni360.ElsaStudio.csproj", "src/Harmoni360.ElsaStudio/"]
RUN dotnet restore "src/Harmoni360.ElsaStudio/Harmoni360.ElsaStudio.csproj"
COPY . .
WORKDIR "/src/src/Harmoni360.ElsaStudio"
RUN dotnet build "Harmoni360.ElsaStudio.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Harmoni360.ElsaStudio.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Harmoni360.ElsaStudio.dll"]