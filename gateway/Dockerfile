FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src

COPY nuget.config .
COPY gateway.csproj .
COPY . .
RUN dotnet publish gateway.csproj -c Release -o /app/publish \
    /p:NUGET_PACKAGES=/root/.nuget/packages

FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS runtime

RUN addgroup -S gateway && adduser -S gateway -G gateway
USER gateway

WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "gateway.dll"]
