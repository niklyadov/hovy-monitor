FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["HovyMonitor.Api/HovyMonitor.Api.csproj", "HovyMonitor.Api/"]
RUN dotnet restore "HovyMonitor.Api/HovyMonitor.Api.csproj" 
COPY . .
RUN dotnet build "HovyMonitor.Api/HovyMonitor.Api.csproj" -c Release -o /app/build -r linux-x64

FROM build AS publish
RUN dotnet publish "HovyMonitor.Api/HovyMonitor.Api.csproj" -c Release -o /app/publish -r linux-x64

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HovyMonitor.Api.dll"]
