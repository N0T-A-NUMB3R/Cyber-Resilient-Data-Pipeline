FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src

COPY nuget.config .
COPY analysis-service.csproj .
COPY src/ ./src/
COPY appsettings.json .
RUN dotnet publish analysis-service.csproj -c Release -o /app/publish \
    /p:NUGET_PACKAGES=/root/.nuget/packages

FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS runtime

RUN addgroup -S analysis && adduser -S analysis -G analysis
USER analysis

WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8082
ENTRYPOINT ["dotnet", "analysis-service.dll"]
