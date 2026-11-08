FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source

COPY backend/src/TaskFlow.Api/TaskFlow.Api.csproj backend/src/TaskFlow.Api/
COPY backend/src/TaskFlow.Application/TaskFlow.Application.csproj backend/src/TaskFlow.Application/
COPY backend/src/TaskFlow.Domain/TaskFlow.Domain.csproj backend/src/TaskFlow.Domain/
COPY backend/src/TaskFlow.Infrastructure/TaskFlow.Infrastructure.csproj backend/src/TaskFlow.Infrastructure/
RUN dotnet restore backend/src/TaskFlow.Api/TaskFlow.Api.csproj

COPY backend/src/ backend/src/
RUN dotnet publish backend/src/TaskFlow.Api/TaskFlow.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "TaskFlow.Api.dll"]
