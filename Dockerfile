FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app

# Definir puerto que usará ASP.NET
ENV ASPNETCORE_HTTP_PORTS=8080

EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["AuthService.ApplicationApi/AuthService.ApplicationApi.csproj", "AuthService.ApplicationApi/"]
COPY ["AuthService.Domain/AuthService.Domain.csproj", "AuthService.Domain/"]
COPY ["AuthService.Infrastructure/AuthService.Infrastructure.csproj", "AuthService.Infrastructure/"]

RUN dotnet restore "AuthService.ApplicationApi/AuthService.ApplicationApi.csproj"

COPY . .
WORKDIR "/src/AuthService.ApplicationApi"
RUN dotnet build "AuthService.ApplicationApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AuthService.ApplicationApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "AuthService.ApplicationApi.dll"]