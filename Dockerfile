# Stage 1: билд
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# копируем csproj и подтягиваем зависимости
COPY dotnet_postgresql.sln .
COPY dotnet_postgresql.csproj ./
RUN dotnet restore

# копируем весь код и билдим
COPY . .
RUN dotnet publish -c Release -o out

# Stage 2: runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/out ./
ENTRYPOINT ["dotnet", "dotnet_postgresql.dll"]
