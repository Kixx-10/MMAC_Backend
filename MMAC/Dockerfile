# Stage 1: Base Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# 👇 FileWatcher/inotify Error 
ENV DOTNET_USE_POLLING_FILE_WATCHER=true
ENV ASPNETCORE_HOSTINGSTARTUPASSEMBLIES=""

# Stage 2: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY . .
RUN dotnet restore
RUN dotnet publish -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Stage 3: Final App Run
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MMAC.dll"]