FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Imagen de build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiar csproj primero para cache de dependencias
COPY ["AuthService.ApplicationApi/AuthService.ApplicationApi.csproj", "AuthService.ApplicationApi/"]
COPY ["AuthService.Domain/AuthService.Domain.csproj", "AuthService.Domain/"]
COPY ["AuthService.Infrastructure/AuthService.Infrastructure.csproj", "AuthService.Infrastructure/"]

RUN dotnet restore "AuthService.ApplicationApi/AuthService.ApplicationApi.csproj"

# Copiar todo el código
COPY . .

WORKDIR "/src/AuthService.ApplicationApi"
RUN dotnet build "AuthService.ApplicationApi.csproj" -c Release -o /app/build

# Publicación optimizada
FROM build AS publish
RUN dotnet publish "AuthService.ApplicationApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Imagen final runtime
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "AuthService.ApplicationApi.dll"]