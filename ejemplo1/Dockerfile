# Estructura base para ejecutar la app
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Etapa de compilación
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["ejemplo1.csproj", "."]
RUN dotnet restore "./ejemplo1.csproj"
COPY . .
RUN dotnet build "ejemplo1.csproj" -c Release -o /app/build

# Etapa de publicación
FROM build AS publish
RUN dotnet publish "ejemplo1.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Configuración final
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ejemplo1.dll"]